using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    // ReSharper disable once IdentifierTypo
    public class ConcurrentIncrementableResettableInt
    {
        private volatile int _value;
        public int Value => _value;

        public Resettable<int> Increment(int addend = 1)
        {
            return new Resettable<int>(Interlocked.Add(ref _value, addend), 
                value => Interlocked.Add(ref _value, -addend));
        }
    }
}
