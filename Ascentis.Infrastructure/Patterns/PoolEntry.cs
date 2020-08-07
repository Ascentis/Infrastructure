using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class PoolEntry<T>
    {
        public T Value { get; }
        public Pool<T> Pool { get; }
        private readonly int _initialRefCount;
        private volatile int _refCount;

        public PoolEntry(Pool<T> pool, T value, int initialRefCount = -1)
        {
            Value = value;
            Pool = pool;
            _refCount = initialRefCount;
            _initialRefCount = initialRefCount;
        }

        public void ResetRefCount()
        {
            _refCount = _initialRefCount;
        }

        public bool ReleaseOne()
        {
            return _refCount <= -1 || Interlocked.Decrement(ref _refCount) <= 0;
        }

        public void Retain()
        {
            if (_refCount > -1)
                Interlocked.Increment(ref _refCount);
        }
    }
}
