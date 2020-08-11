using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class Pool<T> : IPool<T>
    {
        public delegate PoolEntry<T> Builder(Pool<T> pool);

        private readonly ConcurrentBag<PoolEntry<T>> _bag;
        private readonly ManualResetEventSlim _releasedEvent;
        private readonly Builder _builder;
        private volatile int _allowance;
        private readonly object _maxCapacityLock;
        private volatile int _maxCapacity;

        public int MaxCapacity
        {
            get => _maxCapacity;
            set
            {
                /* _maxCapacity will not change frequently so we will use a hard
                   lock to protect the variable from multiple writers */
                lock (_maxCapacityLock)
                {
                    if (_maxCapacity == value)
                        return;

                    int allowance;
                    do
                    {
                        allowance = _allowance;
                    } while (Interlocked.CompareExchange(ref _allowance, allowance + value - _maxCapacity, allowance) != allowance);

                    _maxCapacity = value;
                }
            }
        }

        public Pool(int maxCapacity, Builder builder)
        {
            _maxCapacityLock = new object();
            _allowance = maxCapacity;
            _bag = new ConcurrentBag<PoolEntry<T>>();
            _releasedEvent = new ManualResetEventSlim(false);
            _builder = builder;
            _maxCapacity = maxCapacity;
        }

        public PoolEntry<T> NewPoolEntry(T value, int initialRefCount = -1)
        {
            return new PoolEntry<T>(this, value, initialRefCount);
        }

        public PoolEntry<T> Acquire(int timeout = -1)
        {
            while (true)
            {
                if (_bag.TryTake(out var obj))
                    return obj;

                if (_allowance > 0)
                {
                    int allowance;
                    do
                    {
                        allowance = _allowance;
                        if (allowance <= 0)
                            break;
                    } while (Interlocked.CompareExchange(ref _allowance, allowance - 1, allowance) != allowance);
                    if (allowance > 0)
                        return _builder(this);
                }

                if (!_releasedEvent.Wait(timeout))
                    throw new TimeoutException("No object available in pool");
                _releasedEvent.Reset();
            }
        }

        public void Release(PoolEntry<T> obj)
        {
            if (!obj.ReleaseOne())
                return;
            if (_allowance < 0 && _bag.Count >= _maxCapacity)
            {
                int allowance;
                /* We will loop as long as allowance is negative. If another thread returns objects back to the pool so that
                   allowance is at or over zero we will cut the loop and add the object back to the pool */
                do
                {
                    allowance = _allowance;
                } while(allowance < 0 && Interlocked.CompareExchange(ref _allowance, allowance + 1, allowance) != allowance);

                /* We will check now if even after getting out of the loop decrementing _allowance we still are negative.
                   If this is the case we won't return the object back to the pool and let it be GCed */ 
                if (allowance < 0)
                    return;
            }
            obj.Reset();
            _bag.Add(obj);
            _releasedEvent.Set();
        }
    }
}
