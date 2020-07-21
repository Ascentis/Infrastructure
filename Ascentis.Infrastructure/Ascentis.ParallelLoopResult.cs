namespace Ascentis.Infrastructure
{
    public readonly struct ParallelLoopResult
    {
        public bool IsCompleted { get; }
        public long? LowestBreakIteration { get; }

        internal ParallelLoopResult(bool isCompleted, long? lowestBreakIteration)
        {
            IsCompleted = isCompleted;
            LowestBreakIteration = lowestBreakIteration;
        }
    }
}
