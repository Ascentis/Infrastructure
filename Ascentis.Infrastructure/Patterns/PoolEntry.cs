using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class PoolEntry<T>
    {
        public T Value { get; }
        private readonly int _initialRefCount;
        private volatile int _refCount;

        public PoolEntry(T value, int initialRefCount = -1)
        {
            Value = value;
            _refCount = initialRefCount;
            _initialRefCount = initialRefCount;
        }

        public void ResetRefCount()
        {
            _refCount = _initialRefCount;
        }

        public bool Release()
        {
            return _refCount <= -1 || Interlocked.Decrement(ref _refCount) <= 0;
        }
    }
}
