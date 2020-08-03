using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public readonly struct Resettable<T> : IDisposable
    {
        public delegate void ResetDelegate(T value);
        private readonly ResetDelegate _resetDelegate;

        public T Value { get; }

        public Resettable(T value, ResetDelegate resetDelegate)
        {
            _resetDelegate = resetDelegate;
            Value = value;
        }

        public void Dispose()
        {
            _resetDelegate(Value);
        }
    }
}
