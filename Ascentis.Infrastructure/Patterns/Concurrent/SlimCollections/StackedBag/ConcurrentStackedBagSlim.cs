using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentStackSlim<T> : ConcurrentStackedBagSlim<T> {}

    public class ConcurrentStackedBagSlim<T> : 
        IProducerConsumerCollection<T>, 
        ICollection<T>, 
        IReadOnlyCollection<T>,
        IConcurrentBag<T>,
        IConcurrentStack<T>
    {
        private volatile SlimNode<T> _head;

        public bool Empty => _head == null;
        public int Count => this.Count(_ => true);
        public object SyncRoot => throw new NotSupportedException("SyncRoot not supported");
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;

        public void Add(T value)
        {
            var node = new SlimNode<T>(value);
            AddNode(node, node);
        }

        private void AddNode(SlimNode<T> newHead, SlimNode<T> tail)
        {
            SlimNode<T> localRoot;
            do
            {
                localRoot = _head;
                tail.Next = localRoot;
            } while (Interlocked.CompareExchange(ref _head, newHead, localRoot) != localRoot);
        }

        public void AddRange(T[] items, int startIndex, int count)
        {
            ValidatePushPopRangeInput(items, startIndex, count);
            if (count <= 0)
                return;
            var tail = new SlimNode<T>(items[0]);
            var newHead = tail;
            for (var i = 1; i < count; i++)
                newHead = new SlimNode<T>(items[i]) { Next = newHead };
            AddNode(newHead, tail);
        }

        public void AddRange(T[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            PushRange(items, 0, items.Length);
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

        private static void ValidatePushPopRangeInput(T[] items, int startIndex, int count)
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

        public void Clear()
        {
            _head = null;
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

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public bool Remove(T item)
        {
            var found = false;
            var tmpList = new List<T>();
            foreach (var value in this)
            {
                if (!found && item!.Equals(value))
                {
                    found = true;
                    continue;
                }
                tmpList.Add(value);
            }

            if (!found)
                return false;

            SlimNode<T> tmpHeadNode = null;
            for (var i = tmpList.Count - 1; i >= 0; i--)
                tmpHeadNode = new SlimNode<T>(tmpList[i]) {Next = tmpHeadNode};

            _head = tmpHeadNode;

            return true;
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            foreach (var value in this)
                array.SetValue(value, index++);
        }

        public bool TryTake(out T retVal)
        {
            SlimNode<T> localRoot;
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

        public T[] ToArray()
        {
            var list = new List<T>(this);
            return list.ToArray();
        }

        public bool TryPop(out T result)
        {
            return TryTake(out result);
        }

        public T Pop()
        {
            return Take();
        }

        public bool TryPeek(out T retVal)
        {
            var localRoot = _head;
            if (localRoot == null)
            {
                retVal = default;
                return false;
            }

            retVal = localRoot.Value;
            return true;
        }

        public T Take()
        {
            if(!TryTake(out var retVal))
                throw new InvalidOperationException("Bag is empty");
            return retVal;
        }

        /* This algorithm is thread safe even though there's no locking of any sort
           Once we link nodes the Next property of SlimNode is immutable. Once storing _head in node local var
           GetEnumerator() walks a "snapshot" in time of the contents of the structure */
        public IEnumerator<T> GetEnumerator()
        {
            var node = _head;
            do
            {
                if (node != null)
                    yield return node.Value;
                else
                    break;
                node = node.Next;
            } while (true);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
