using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class PoolEntry<T>
    {
        public const int Taken = 1;
        public const int NotTaken = 0;

        public delegate void PoolEntryDelegate(PoolEntry<T> entry);

        public event PoolEntryDelegate OnReleaseOne;
        public event PoolEntryDelegate OnReset;
        public event PoolEntryDelegate OnRetain;
        public event PoolEntryDelegate OnTake;

        public T Value { get; }
        public Pool<T> Pool { get; }
        private readonly int _initialRefCount;
        private volatile int _refCount;
        private volatile int _taken;

        public PoolEntry(Pool<T> pool, T value, int initialRefCount = -1)
        {
            Value = value;
            Pool = pool;
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
            OnReleaseOne?.Invoke(this);
            return _refCount <= -1 || Interlocked.Decrement(ref _refCount) <= 0;
        }

        public void Retain()
        {
            OnRetain?.Invoke(this);
            if (_refCount > -1)
                Interlocked.Increment(ref _refCount);
        }

        public bool Take()
        {
            OnTake?.Invoke(this);
            return Interlocked.CompareExchange(ref _taken, Taken, NotTaken) == NotTaken;
        }
    }
}
