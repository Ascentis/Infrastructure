using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class ConcurrentCollectionSlim<T> : 
        ICollection<T>, 
        IReadOnlyCollection<T>, 
        IProducerConsumerCollection<T>,
        IConcurrentBag<T>
    {
        public virtual int Count => this.Count(_ => true);
        public object SyncRoot => throw new NotSupportedException("SyncRoot not supported");
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;

        public void AddRange(T[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            AddRange(items, 0, items.Length);
        }

        public void AddRange(T[] items, int startIndex, int count)
        {
            ValidatePushPopRangeInput(items, startIndex, count);
            if (count <= 0)
                return;
            AddRangeInternal(items, startIndex, count);
        }

        protected static void ValidatePushPopRangeInput(T[] items, int startIndex, int count)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count out of range calling PushRange()");
            var length = items.Length;
            if (startIndex >= length || startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "StartIndex out of range calling PushRange()");
            if (length - count < startIndex)
                throw new ArgumentException("Invalid count calling PushRange()");
        }
        
        public T Take()
        {
            if (!TryTake(out var retVal))
                throw new InvalidOperationException("Bag is empty");
            return retVal;
        }

        public bool Contains(T item)
        {
            return this.Any(value => value.Equals(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            foreach (var value in this)
                array[arrayIndex++] = value;
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            foreach (var value in this)
                array.SetValue(value, index++);
        }

        public T[] ToArray()
        {
            var list = new List<T>(this);
            return list.ToArray();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected IEnumerator<T> GetEnumerator(BaseNodeSlim<T> startNode)
        {
            var node = startNode;
            do
            {
                if (node != null)
                    yield return node.Value;
                else
                    break;
                node = node.GetNext();
            } while (true);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("Remove() not supported");
        }
        
        public abstract void Add(T item);
        public abstract void Clear();
        public abstract bool IsEmpty { get; }
        public abstract bool TryAdd(T item);
        public abstract bool TryTake(out T item);
        public abstract bool TryPeek(out T retVal);
        protected abstract void AddRangeInternal(T[] items, int startIndex, int count);
        public abstract IEnumerator<T> GetEnumerator();
        public abstract int Length { get; }
    }
}
