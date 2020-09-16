using System;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentStackSlim<T> : ConcurrentStackedBagSlim<T>
    {
        public ConcurrentStackSlim(bool keepCount = false) : base(keepCount) {}
    }

    public class ConcurrentStackedBagSlim<T> : ConcurrentCollectionSlim<T>, IConcurrentStack<T>
    {
        private volatile int _count;
        private readonly bool _keepCount;
        private volatile StackedBagNodeSlim<T> _head;

        public ConcurrentStackedBagSlim(bool keepCount = false)
        {
            _keepCount = keepCount;
        }

        public override bool IsEmpty => _head == null;
        public override int Count => _keepCount ? _count : base.Count;
        public override int Length => _keepCount
            ? _count
            : throw new InvalidOperationException("KeepCount must be enabled to use Length property. You could fall back to Count at a cost of a full scan");

        public override void Add(T value)
        {
            var node = new StackedBagNodeSlim<T>(value);
            Add(node, node);
            if (_keepCount)
                Interlocked.Increment(ref _count);
        }

        private void Add(StackedBagNodeSlim<T> rangeHead, StackedBagNodeSlim<T> rangeTail)
        {
            SpinWait? spinner = null;
            StackedBagNodeSlim<T> localHead = null;
            do
            {
                if (localHead != null)
                    StackedBagNodeSlim<T>.Spin(ref spinner);
                localHead = _head;
                rangeTail.Next = localHead;
            } while (Interlocked.CompareExchange(ref _head, rangeHead, localHead) != localHead);
        }

        protected override void AddRangeInternal(T[] items, int startIndex, int count)
        {
            var rangeTail = new StackedBagNodeSlim<T>(items[0]);
            var rangeHead = rangeTail;
            for (var i = 1; i < count; i++)
                rangeHead = new StackedBagNodeSlim<T>(items[i]) { Next = rangeHead };
            Add(rangeHead, rangeTail);
            if (_keepCount)
                Interlocked.Add(ref _count, count);
        }

        public void Push(T item)
        {
            Add(item);
        }

        public void PushRange(T[] items, int startIndex, int count)
        {
            AddRange(items, startIndex, count);
        }

        public void PushRange(T[] items)
        {
            AddRange(items);
        }

        public override void Clear()
        {
            _head = null;
            _count = 0;
        }

        public override bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public override bool TryTake(out T retVal)
        {
            SpinWait? spinner = null;
            StackedBagNodeSlim<T> localHead;
            do
            {
                localHead = _head;
                if (localHead != null)
                {
                    StackedBagNodeSlim<T>.Spin(ref spinner);
                    continue;
                }

                retVal = default;
                return false;
            } while (Interlocked.CompareExchange(ref _head, localHead.Next, localHead) != localHead);

            if (_keepCount)
                Interlocked.Decrement(ref _count);
            retVal = localHead.Value;
            return true;
        }

        public bool TryPop(out T result)
        {
            return TryTake(out result);
        }

        public T Pop()
        {
            return Take();
        }

        public override bool TryPeek(out T retVal)
        {
            var localHead = _head;
            if (localHead == null)
            {
                retVal = default;
                return false;
            }

            retVal = localHead.Value;
            return true;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator(_head);
        }
    }
}
