using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentStackSlim<T> : ConcurrentStackedBagSlim<T> {}

    public class ConcurrentStackedBagSlim<T> : ConcurrentCollectionSlim<T>, IConcurrentStack<T>
    {
        private volatile StackedBagNodeSlim<T> _head;

        public override bool IsEmpty => _head == null;

        public override void Add(T value)
        {
            var node = new StackedBagNodeSlim<T>(value);
            Add(node, node);
        }

        private void Add(StackedBagNodeSlim<T> newHead, StackedBagNodeSlim<T> tail)
        {
            StackedBagNodeSlim<T> localRoot;
            do
            {
                localRoot = _head;
                tail.Next = localRoot;
            } while (Interlocked.CompareExchange(ref _head, newHead, localRoot) != localRoot);
        }

        protected override void AddRangeInternal(T[] items, int startIndex, int count)
        {
            var tail = new StackedBagNodeSlim<T>(items[0]);
            var newHead = tail;
            for (var i = 1; i < count; i++)
                newHead = new StackedBagNodeSlim<T>(items[i]) { Next = newHead };
            Add(newHead, tail);
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
        }

        public override bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public override bool TryTake(out T retVal)
        {
            StackedBagNodeSlim<T> localRoot;
            do
            {
                localRoot = _head;
                if (localRoot != null) 
                    continue;
                retVal = default;
                return false;
            } while (Interlocked.CompareExchange(ref _head, localRoot.Next, localRoot) != localRoot);

            retVal = localRoot.Value;
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

        /* This algorithm is thread safe even though there's no locking of any sort
           Once we link nodes the Next property of StackedBagNodeSlim is immutable. Once storing _head in node local var
           GetEnumerator() walks a "snapshot" in time of the contents of the structure */
        public override IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator(_head);
        }
    }
}
