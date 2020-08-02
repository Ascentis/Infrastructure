using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConcurrentReservoir<T>
    {
        public delegate T Builder();
        private readonly ConcurrentBag<T> _reservoir;
        private readonly ManualResetEventSlim _releasedEvent;
        private readonly Builder _builder;
        private volatile int _allowance;

        public ConcurrentReservoir(int maxCapacity, Builder builder)
        {
            _allowance = maxCapacity;
            _reservoir = new ConcurrentBag<T>();
            _releasedEvent = new ManualResetEventSlim(false);
            _builder = builder;
        }

        public T Acquire(int timeout = -1)
        {
            T obj;
            do
            {
                if (_reservoir.TryTake(out obj))
                    break;
                if (_allowance > 0)
                {
                    var allowance = Interlocked.Decrement(ref _allowance);
                    if (allowance >= 0)
                    {
                        obj = _builder();
                        break;
                    }
                    Interlocked.Increment(ref _allowance);
                }
                if (!_releasedEvent.Wait(timeout))
                    throw new TimeoutException("No object available in Reservoir");
                _releasedEvent.Reset();
            } while (true);

            return obj;
        }

        public void Release(T obj)
        {
            _reservoir.Add(obj);
            _releasedEvent.Set();
        }
    }
}
