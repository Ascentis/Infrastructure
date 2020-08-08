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
        private int _maxCapacity;

        public int MaxCapacity
        {
            get => _maxCapacity;
            set
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

        public Pool(int maxCapacity, Builder builder)
        {
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
            obj.Reset();
            _bag.Add(obj);
            _releasedEvent.Set();
        }
    }
}
