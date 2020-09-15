using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentQueueSlim<T> : ConcurrentQueuedBagSlim<T> {}

    public class ConcurrentQueuedBagSlim<T> : ConcurrentCollectionSlim<T>, IConcurrentQueue<T>
       
    {
        private volatile QueuedBagSlimNode<T> _head;
        private volatile QueuedBagSlimNode<T> _tail;

        public ConcurrentQueuedBagSlim()
        {
            Init();
        }

        private void Init()
        {
            _tail = new QueuedBagSlimNode<T>(default);
            _head = _tail;
        }

        public override bool Empty => _head.Next == null;

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
            var node = new QueuedBagSlimNode<T>(default);
            Add(value, node, node);
        }

        private void Add(T value, QueuedBagSlimNode<T> newTailHead, QueuedBagSlimNode<T> newTail)
        {
            do
            {
                var localTail = _tail;
                if (Interlocked.CompareExchange(ref localTail.Next, newTailHead, null) != null)
                    continue;
                localTail.Value = value;
                _tail = newTail;
                localTail.Ground = false;
                return;
            } while (true);
        }

        protected override void AddRangeInternal(T[] items, int startIndex, int count)
        {
            QueuedBagSlimNode<T> newTailHead = null;
            QueuedBagSlimNode<T> newNode = null;
            for (var i = 1; i < count; i++)
            {
                var prevTail = newNode;
                newNode = new QueuedBagSlimNode<T>(items[i]) {Ground = false};
                newTailHead ??= newNode;
                if (prevTail != null)
                    prevTail.Next = newNode;
            }

            var newTail = new QueuedBagSlimNode<T>(default);
            if (newNode != null)
                newNode.Next = newTail;
            else
                newTailHead = newTail;
            Add(items[0], newTailHead, newTail);
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
            QueuedBagSlimNode<T> localHead;
            do
            {
                localHead = _head;
                if (localHead.Next != null)
                    continue;
                retVal = default;
                return false;
            } while (Interlocked.CompareExchange(ref _head, localHead.Next, localHead) != localHead);

            localHead.EnsureUngrounded();
            retVal = localHead.Value;
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

            localHead.EnsureUngrounded();
            retVal = localHead.Value;
            return true;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator(_head);
        }
    }
}
