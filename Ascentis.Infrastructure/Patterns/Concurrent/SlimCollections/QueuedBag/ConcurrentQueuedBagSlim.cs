using System;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentQueueSlim<T> : ConcurrentQueuedBagSlim<T>
    {
        public ConcurrentQueueSlim(bool keepCount = false) : base(keepCount) {}
    }

    public class ConcurrentQueuedBagSlim<T> : ConcurrentCollectionSlim<T>, IConcurrentQueue<T>
    {
        private volatile int _count;
        private readonly bool _keepCount;
        private volatile QueuedBagNodeSlim<T> _head;
        private volatile QueuedBagNodeSlim<T> _tail;

        public ConcurrentQueuedBagSlim(bool keepCount = false)
        {
            Init();
            _keepCount = keepCount;
        }

        private void Init()
        {
            _head = _tail = new QueuedBagNodeSlim<T>();
            _count = 0;
        }

        public override bool IsEmpty => _head.Next == null;
        public override int Count => _keepCount ? _count : base.Count;

        public override int Length => _keepCount
            ? _count
            : throw new InvalidOperationException("KeepCount must be enabled to use Length property. You could fall back to Count at a cost of a full scan");

        public void Enqueue(T value)
        {
            Add(value);
        }

        public void EnqueueRange(T[] items)
        {
            AddRange(items);
        }

        public void EnqueueRange(T[] items, int startIndex, int count)
        {
            AddRange(items, startIndex, count);
        }

        public bool TryDequeue(out T retVal)
        {
            return TryTake(out retVal);
        }

        public override void Add(T value)
        {
            var node = new QueuedBagNodeSlim<T>();
            Add(value, node, node);
            if (_keepCount)
                Interlocked.Increment(ref _count);
        }

        private void Add(T value, QueuedBagNodeSlim<T> newTailHead, QueuedBagNodeSlim<T> newTail)
        {
            SpinWait? spinner = null;
            while (Interlocked.CompareExchange(ref _tail.Next, newTailHead, null) != null)
                QueuedBagNodeSlim<T>.Spin(ref spinner);

            var oldTail = _tail;
            oldTail.Value = value;
            _tail = newTail;
            oldTail.Ground = false;
        }

        protected override void AddRangeInternal(T[] items, int startIndex, int count)
        {
            QueuedBagNodeSlim<T> newTailHead = null;
            QueuedBagNodeSlim<T> newNode = null;
            for (var i = 1; i < count; i++)
            {
                var prevTail = newNode;
                newNode = new QueuedBagNodeSlim<T>(items[i]);
                newTailHead ??= newNode;
                if (prevTail != null)
                    prevTail.Next = newNode;
            }

            var newTail = new QueuedBagNodeSlim<T>();
            if (newNode != null)
                newNode.Next = newTail;
            else
                newTailHead = newTail;
            Add(items[0], newTailHead, newTail);
            if (_keepCount)
                Interlocked.Add(ref _count, count);
        }

        public T Dequeue()
        {
            return Take();
        }

        public override void Clear()
        {
            Init();
        }
        
        public override bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public override bool TryTake(out T retVal)
        {
            SpinWait? spinner = null;
            QueuedBagNodeSlim<T> localHead;
            do
            {
                localHead = _head;
                if (localHead.Next != null)
                {
                    QueuedBagNodeSlim<T>.Spin(ref spinner);
                    continue;
                }

                retVal = default;
                return false;
            } while (Interlocked.CompareExchange(ref _head, localHead.Next, localHead) != localHead);

            if (_keepCount)
                Interlocked.Decrement(ref _count);
            retVal = localHead.GetUngroundedValue();
            return true;
        }
        
        public override bool TryPeek(out T retVal)
        {
            var localHead = _head;
            if (localHead.Next == null)
            {
                retVal = default;
                return false;
            }

            retVal = localHead.GetUngroundedValue();
            return true;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator(_head);
        }
    }
}
