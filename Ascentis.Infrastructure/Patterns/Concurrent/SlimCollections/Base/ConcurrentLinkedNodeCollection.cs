using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class ConcurrentLinkedNodeCollection<T, TNode> : 
        ICollection<T>, 
        IReadOnlyCollection<T>, 
        IProducerConsumerCollection<T>,
        IConcurrentBag<T>
        where TNode : BaseLinkedNode<T, TNode>
    {
        protected bool KeepCount;
        // ReSharper disable once InconsistentNaming
        protected volatile int _count;
        protected volatile TNode Head;

        public object SyncRoot => throw new NotSupportedException("SyncRoot not supported");
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;
        public bool IsEmpty => Head.Next == null;
        public int Count => KeepCount ? _count : this.Count(_ => true);
        public int Length => KeepCount
            ? _count
            : throw new InvalidOperationException("KeepCount must be enabled to use Length property. You could fall back to Count at a cost of a full scan");

        protected ConcurrentLinkedNodeCollection(bool keepCount = false)
        {
            KeepCount = keepCount;
            Init();
        }

        private void Init()
        {
            Head = BuildNode();
            _count = 0;
        }

        public virtual void Clear() => Init();

        public void AddRange(T[] items)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(items, nameof(items));
            AddRange(items, 0, items.Length);
        }

        public void AddRange(T[] items, int startIndex, int count)
        {
            ValidatePushPopRangeInput(items, startIndex, count);
            if (count <= 0)
                return;
            AddRangeInternal(items, startIndex, count);
        }

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public void Add(T value)
        {
            var node = BuildNode();
            Add(value, node, node);
            if (KeepCount)
                Interlocked.Increment(ref _count);
        }

        private static void ValidatePushPopRangeInput(T[] items, int startIndex, int count)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(items, nameof(items));
            ArgsChecker.Check<ArgumentOutOfRangeException>(count >= 0, nameof(count), "Count out of range calling PushRange()");
            var itemsLength = items.Length;
            ArgsChecker.Check<ArgumentOutOfRangeException>(startIndex < itemsLength && startIndex >= 0, nameof(startIndex), "StartIndex out of range calling PushRange()");
            ArgsChecker.Check<ArgumentException>(itemsLength - count >= startIndex, "Invalid count calling PushRange()");
        }

        public bool Contains(T item) => this.Any(value => value.Equals(item));

        public void CopyTo(T[] array, int index)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(array, nameof(array));
            foreach (var value in this)
                array[index++] = value;
        }

        public void CopyTo(Array array, int index)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(array, nameof(array));
            foreach (var value in this)
                array.SetValue(value, index++);
        }

        public T[] ToArray() => new List<T>(this).ToArray();
 
        public T Take()
        {
            if (!TryTake(out var retVal))
                throw new InvalidOperationException("Bag is empty");
            return retVal;
        }

        public bool TryTake(out T retVal)
        {
            SpinWait? spinner = null;
            TNode localHead = null;
            do
            {
                if (localHead != null)
                    Spinner.Spin(ref spinner);
                if ((localHead = Head).Next != null)
                    continue;

                retVal = default;
                return false;
            } while (Interlocked.CompareExchange(ref Head, localHead.Next, localHead) != localHead);

            if (KeepCount)
                Interlocked.Decrement(ref _count);
            retVal = localHead.Value;
            return true;
        }

        public bool TryPeek(out T retVal)
        {
            var localHead = Head;
            if (localHead.Next == null)
            {
                retVal = default;
                return false;
            }

            retVal = localHead.Value;
            return true;
        }

        public bool Remove(T item) => throw new NotSupportedException("Remove() not supported");

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var node = Head; node.Next != null; node = node.Next)
                yield return node.Value;
        }

        protected abstract void AddRangeInternal(T[] items, int startIndex, int count);
        protected abstract void Add(T value, TNode rangeHead, TNode rangeTail);
        protected abstract TNode BuildNode();
    }
}
