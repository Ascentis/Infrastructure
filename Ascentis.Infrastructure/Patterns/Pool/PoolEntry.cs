using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class PoolEntry<T> : IPoolEntry
    {
        private class DefaultPool : IPool<T>
        {
            public void Release(PoolEntry<T> obj)
            {
                obj.ReleaseOne();
            }
        }

        private static readonly IPool<T> FallbackPool = new DefaultPool();

        public const int Taken = 1;
        public const int NotTaken = 0;

        public delegate void PoolEntryDelegate(PoolEntry<T> entry);

        public event PoolEntryDelegate OnReleaseOne;
        public event PoolEntryDelegate OnReset;
        public event PoolEntryDelegate OnRetain;
        public event PoolEntryDelegate OnTake;

        public T Value { get; set; }
        public IPool<T> Pool { get; }
        private readonly int _initialRefCount;
        private volatile int _refCount;
        private volatile int _taken;

        public PoolEntry(IPool<T> pool, T value, int initialRefCount = -1)
        {
            Value = value;
            Pool = pool ?? FallbackPool;
            _refCount = initialRefCount;
            _initialRefCount = initialRefCount;
        }

        public PoolEntry(T value) : this(null, value) { }

        public void Reset()
        {
            OnReset?.Invoke(this);
            _refCount = _initialRefCount;
            _taken = NotTaken;
        }

        public bool ReleaseOne()
        {
            var released = _refCount <= -1 || Interlocked.Decrement(ref _refCount) <= 0;
            if (released)
                OnReleaseOne?.Invoke(this);
            return released;
        }

        public void Retain()
        {
            OnRetain?.Invoke(this);
            if (_refCount > -1)
                Interlocked.Increment(ref _refCount);
        }

        public bool Take()
        {
            var taken = Interlocked.CompareExchange(ref _taken, Taken, NotTaken) == NotTaken;
            if (taken)
                OnTake?.Invoke(this);
            return taken;
        }

        public void Release()
        {
            Pool?.Release(this);
        }
    }
}
