using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentStackSlim<T> : ConcurrentStackedBagSlim<T>
    {
        public ConcurrentStackSlim(bool keepCount = false) : base(keepCount) {}
    }

    public class ConcurrentStackedBagSlim<T> : 
        ConcurrentLinkedNodeCollection<T,  StackedBagNodeSlim<T>>, 
        IConcurrentStack<T>
    {
        public ConcurrentStackedBagSlim(bool keepCount = false) : base(keepCount) { }

        public void Push(T item) => Add(item);
        public void PushRange(T[] items, int startIndex, int count) => AddRange(items, startIndex, count);
        public void PushRange(T[] items) => AddRange(items);
        public bool TryPop(out T result) => TryTake(out result);
        public T Pop() => Take();
        
        protected override void Add(T value, StackedBagNodeSlim<T> rangeHead, StackedBagNodeSlim<T> rangeTail)
        {
            rangeHead.Value = value;
            SpinWait? spinner = null;
            StackedBagNodeSlim<T> localHead = null;
            do
            {
                if (localHead != null)
                    Spinner.Spin(ref spinner);
                rangeTail.Next = localHead = Head;
            } while (Interlocked.CompareExchange(ref Head, rangeHead, localHead) != localHead);
        }

        protected override void AddRangeInternal(T[] items, int startIndex, int count)
        {
            var rangeTail = new StackedBagNodeSlim<T> { Value = items[0] };
            var rangeHead = rangeTail;
            for (var i = 1; i < count - 1; i++)
                rangeHead = new StackedBagNodeSlim<T> { Value = items[i], Next = rangeHead };
            if (count > 1)
                rangeHead = new StackedBagNodeSlim<T> { Next = rangeHead };
            Add(items[count - 1], rangeHead, rangeTail);
            if (KeepCount)
                Interlocked.Add(ref _count, count);
        }

        protected override StackedBagNodeSlim<T> BuildNode() => new StackedBagNodeSlim<T>();
    }
}
