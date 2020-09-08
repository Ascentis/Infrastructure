using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SpinLockedField<T>
    {
        private SpinLock _lock;
        private T _field;

        public SpinLockedField()
        {
            _lock = new SpinLock();
        }

        public T Get()
        {
            var lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                return _field;
            }
            finally
            {
                if (lockTaken)
                    _lock.Exit();
            }
        }

        public void Set(T value)
        {
            var lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                _field = value;
            }
            finally
            {
                if (lockTaken)
                    _lock.Exit();
            }
        }
    }
}
