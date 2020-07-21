using System;

namespace Ascentis.Infrastructure
{
    public readonly struct Cleanable<T> : IDisposable
    {
        public delegate void CleanupDelegate(T value);
        private readonly CleanupDelegate _cleanupDelegate;

        public T Value { get; }

        public Cleanable(T value, CleanupDelegate cleanupDelegate)
        {
            _cleanupDelegate = cleanupDelegate;
            Value = value;
        }

        public void Dispose()
        {
            _cleanupDelegate(Value);
        }
    }
}
