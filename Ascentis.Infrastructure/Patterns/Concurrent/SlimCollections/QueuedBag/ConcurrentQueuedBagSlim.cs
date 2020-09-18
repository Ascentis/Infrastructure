using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentQueueSlim<T> : ConcurrentQueuedBagSlim<T>
    {
        public ConcurrentQueueSlim(bool keepCount = false) : base(keepCount) {}
    }

    public class ConcurrentQueuedBagSlim<T> : 
        ConcurrentLinkedNodeCollection<T, QueuedBagNodeSlim<T>>, 
        IConcurrentQueue<T>
    {
        private volatile QueuedBagNodeSlim<T> _tail;

        public ConcurrentQueuedBagSlim(bool keepCount = false) : base(keepCount) => _tail = Head;

        public override void Clear()
        {
            base.Clear();
            _tail = Head;
        }

        public void Enqueue(T value) => Add(value);
        public void EnqueueRange(T[] items) => AddRange(items);
        public void EnqueueRange(T[] items, int startIndex, int count) => AddRange(items, startIndex, count);
        public bool TryDequeue(out T retVal) => TryTake(out retVal);
        public T Dequeue() => Take();

        protected override void Add(T value, QueuedBagNodeSlim<T> rangeHead, QueuedBagNodeSlim<T> rangeTail)
        {
            SpinWait? spinner = null;
            while (Interlocked.CompareExchange(ref _tail.Next, rangeHead, null) != null)
                QueuedBagNodeSlim<T>.Spin(ref spinner);

            var oldTail = _tail;
            oldTail.Value = value;
            _tail = rangeTail;
            oldTail.Ground = false;
        }

        protected override void AddRangeInternal(T[] items, int startIndex, int count)
        {
            QueuedBagNodeSlim<T> rangeHead = null;
            QueuedBagNodeSlim<T> newNode = null;
            for (var i = 1; i < count; i++)
            {
                var prevTail = newNode;
                newNode = new QueuedBagNodeSlim<T> { Value = items[i], Ground = false };
                rangeHead ??= newNode;
                if (prevTail != null)
                    prevTail.Next = newNode;
            }

            var rangeTail = new QueuedBagNodeSlim<T>();
            if (newNode != null)
                newNode.Next = rangeTail;
            else
                rangeHead = rangeTail;
            Add(items[0], rangeHead, rangeTail);
            if (KeepCount)
                Interlocked.Add(ref _count, count);
        }

        protected override QueuedBagNodeSlim<T> BuildNode() => new QueuedBagNodeSlim<T>();
    }
}
