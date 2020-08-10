using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class BoundedParallelStats
    {
        private volatile int _totalSerialRunCount;
        private volatile int _totalParallelRunCount;
        private volatile int _totalParallelThreadsConsumed;

        public int TotalSerialRunCount => _totalSerialRunCount;
        public int TotalParallelRunCount => _totalParallelRunCount;
        public int TotalParallelsThreadConsumed => _totalParallelThreadsConsumed;

        public void IncrementSerialRunCount()
        {
            Interlocked.Increment(ref _totalSerialRunCount);
        }

        public void IncrementParallelRunCount()
        {
            Interlocked.Increment(ref _totalParallelRunCount);
        }

        public void IncrementParallelThreadsConsumed(int threadsConsumed)
        {
            Interlocked.Add(ref _totalParallelThreadsConsumed, threadsConsumed);
        }

        public void ResetTotalSerialRunCount()
        {
            _totalSerialRunCount = 0;
        }

        public void ResetTotalParallelsThreadConsumed()
        {
            _totalParallelThreadsConsumed = 0;
        }

        public void ResetTotalParallelRunCount()
        {
            _totalParallelRunCount = 0;
        }
    }
}
