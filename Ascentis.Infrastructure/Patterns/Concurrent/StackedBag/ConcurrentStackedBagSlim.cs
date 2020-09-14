using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    internal class StackedBagSlimNode<T>
    {
        internal T Value { get; }
        internal StackedBagSlimNode<T> Next;

        internal StackedBagSlimNode(T value)
        {
            Value = value;
        }
    }

    public class ConcurrentBagSlim<T> : ConcurrentStackedBagSlim<T> {}
    public class ConcurrentStackSlim<T> : ConcurrentStackedBagSlim<T> {}

    public class ConcurrentStackedBagSlim<T> : 
        IProducerConsumerCollection<T>, 
        ICollection<T>, 
        IReadOnlyCollection<T>,
        IConcurrentBag<T>,
        IConcurrentStack<T>
    {
        private volatile StackedBagSlimNode<T> _head;

        public bool Empty => _head == null;
        public int Count => this.Count(_ => true);
        public object SyncRoot => throw new NotSupportedException("SyncRoot not supported");
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;

        public void Add(T value)
        {
            var node = new StackedBagSlimNode<T>(value);
            StackedBagSlimNode<T> localRoot;
            do
            {
                localRoot = _head;
                node.Next = localRoot;
            } while (Interlocked.CompareExchange(ref _head, node, localRoot) != localRoot);
        }

        public void Push(T item)
        {
            Add(item);
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

            StackedBagSlimNode<T> tmpHeadNode = null;
            for (var i = tmpList.Count - 1; i >= 0; i--)
                tmpHeadNode = new StackedBagSlimNode<T>(tmpList[i]) {Next = tmpHeadNode};

            _head = tmpHeadNode;

            return true;
        }

        public void CopyTo(Array array, int index)
        {
            foreach (var value in this)
                array.SetValue(value, index++);
        }

        public bool TryTake(out T retVal)
        {
            StackedBagSlimNode<T> localRoot;
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
           Once we link nodes the Next property of StackedBagSlimNode is immutable. Once storing _head in node local var
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
