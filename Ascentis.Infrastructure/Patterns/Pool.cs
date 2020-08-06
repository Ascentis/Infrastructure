using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class Pool<T>
    {
        public delegate PoolEntry<T> Builder(Pool<T> pool);
        private readonly ConcurrentBag<PoolEntry<T>> _bag;
        private readonly ManualResetEventSlim _releasedEvent;
        private readonly Builder _builder;
        private volatile int _allowance;

        public Pool(int maxCapacity, Builder builder)
        {
            _allowance = maxCapacity;
            _bag = new ConcurrentBag<PoolEntry<T>>();
            _releasedEvent = new ManualResetEventSlim(false);
            _builder = builder;
        }

        public PoolEntry<T> NewPoolEntry(T value, int initialRefCount = -1)
        {
            return new PoolEntry<T>(value, initialRefCount);
        }

        public PoolEntry<T> Acquire(int timeout = -1)
        {
            PoolEntry<T> obj;
            while (true)
            {
                if (_bag.TryTake(out obj))
                    break;
                if (_allowance > 0)
                {
                    var allowance = Interlocked.Decrement(ref _allowance);
                    if (allowance >= 0)
                    {
                        obj = _builder(this);
                        break;
                    }
                    Interlocked.Increment(ref _allowance);
                }
                if (!_releasedEvent.Wait(timeout))
                    throw new TimeoutException("No object available in pool");
                _releasedEvent.Reset();
            }

            return obj;
        }

        public void Release(PoolEntry<T> obj)
        {
            if (!obj.Release())
                return;
            obj.ResetRefCount();
            _bag.Add(obj);
            _releasedEvent.Set();
        }
    }
}
