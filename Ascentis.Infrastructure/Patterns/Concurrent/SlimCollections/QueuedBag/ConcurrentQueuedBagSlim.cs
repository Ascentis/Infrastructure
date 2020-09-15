using System.Collections.Generic;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentQueueSlim<T> : ConcurrentQueuedBagSlim<T> {}

    public class ConcurrentQueuedBagSlim<T> : SlimCollection<T>
       
    {
        private volatile SlimNode<T> _head;
        private volatile SlimNode<T> _tail;

        public override bool Empty => _head == null;

        public void Enqueue(T value)
        {
            Add(value);
        }

        public bool TryDequeue(out T retVal)
        {
            return TryTake(out retVal);
        }

        public override void Add(T value)
        {
            SlimNode<T> firstNode = null;
            var node = new SlimNode<T>(value);
            do
            {
                var localTail = _tail;

                if (localTail != null && Interlocked.CompareExchange(ref localTail.Next, node, null) != null)
                    continue;

                if (localTail == null)
                {
                    if (Interlocked.CompareExchange(ref _tail, node, null) != null)
                        continue;

                    _head = node;
                    break;
                }

                _tail = node;
                break;
            } while (true);
        }

        public override void AddRange(T[] items, int startIndex, int count)
        {
            ValidatePushPopRangeInput(items, startIndex, count);
            if (count <= 0)
                return;
        }

        public override void Clear()
        {
            _head = null;
            _tail = null;
        }
        
        public override bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public override bool Remove(T item)
        {
            return false;
        }
        
        public override bool TryTake(out T retVal)
        {
            SlimNode<T> localHead;
            do
            {
                localHead = _head;
                if (localHead != null)
                    continue;
                retVal = default;
                return false;
            } while (Interlocked.CompareExchange(ref _head, localHead.Next, localHead) != localHead);

            if (localHead.Next == null)
                Interlocked.CompareExchange(ref _tail, null, localHead);

            retVal = localHead.Value;
            return true;
        }
        
        public override bool TryPeek(out T retVal)
        {
            retVal = default;
            return true;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator(_head);
        }
    }
}
