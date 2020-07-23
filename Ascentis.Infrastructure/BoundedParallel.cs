using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable ConvertToLambdaExpression

namespace Ascentis.Infrastructure
{
    public class BoundedParallel
    {
        #region Public general declarations

        public delegate System.Threading.Tasks.ParallelLoopResult ParallelLoopDelegate(int allowedThreadCount);
        public delegate void ParallelInvokeDelegate(int allowedThreadCount);
        public const int DefaultMaxParallelInvocations = 2;
        public const int Unlimited = -1; // -1 equals to no limit in the number of threads or invocations that *could* run in parallel before going serial

        #endregion
        #region Private declarations

        private static readonly System.Threading.Tasks.ParallelLoopResult DefaultSystemParallelLoopResult = new System.Threading.Tasks.ParallelLoopResult();
        private static readonly ParallelLoopResult DefaultCompletedParallelLoopResult = new ParallelLoopResult(true, null);
        private static readonly ParallelLoopResult DefaultNotCompletedParallelLoopResult = new ParallelLoopResult(false, null);
        private static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();

        private volatile int _totalSerialRunCount; // This primarily of use when unit testing
        private volatile int _totalParallelThreadsConsumed; // This primarily of use when unit testing
        private volatile int _concurrentInvocationsCount;
        private volatile int _concurrentThreadsCount;
        
        #endregion
        #region Public properties

        public int TotalSerialRunCount => _totalSerialRunCount;
        public int TotalParallelsThreadConsumed => _totalParallelThreadsConsumed;
        public int ConcurrentInvocationsCount => _concurrentInvocationsCount;
        public int ConcurrentThreadsCount => _concurrentThreadsCount;
        public bool AbortInvocationsOnSerialInvocationException { get; set; } // Default value if DIFFERENT than Parallel class. Set to false to match behavior of Parallel class
        /* If MaxParallelInvocations is exceeded OR MaxParallelThreads is exceeded execution of Action array will be done serially */
        public int MaxParallelInvocations { get; set; } // Set to Unlimited (-1) to disable check on number of parallel invocations
        public int MaxParallelThreads { get; set; } // Set to Unlimited (-1) to disable check on number of potential active - utilized - threads
        
        #endregion
        #region Constructor and public methods

        public BoundedParallel(int maxParallelInvocations = DefaultMaxParallelInvocations, int defaultMaxParallelThreads = Unlimited)
        {
            MaxParallelInvocations = maxParallelInvocations;
            MaxParallelThreads = defaultMaxParallelThreads;
            AbortInvocationsOnSerialInvocationException = true;
        }

        public void ResetTotalSerialRunCount()
        {
            _totalSerialRunCount = 0;
        }

        public void ResetTotalParallelsThreadConsumed()
        {
            _totalParallelThreadsConsumed = 0;
        }

        #endregion
        #region Internals
        private static bool IsGateLimitOpen(int limit, int current)
        {
            return limit == Unlimited || current <= limit;
        }

        private int GetAllowedThreadCount(int currentConcurrentThreadCount, int requestedThreadCount)
        {
            if (MaxParallelThreads == Unlimited)
                return requestedThreadCount;
            if (currentConcurrentThreadCount <= MaxParallelThreads)
                return requestedThreadCount;
            var deltaAllowedThreads = MaxParallelThreads - currentConcurrentThreadCount + requestedThreadCount;
            return deltaAllowedThreads > 1 ? deltaAllowedThreads : requestedThreadCount; // Only if we can fit more than 1 thread we will return the delta
        }

        private bool TryParallel(ParallelLoopDelegate bodyParallelCall, out ParallelLoopResult parallelLoopResult, int threadCount)
        {
            using var concurrentInvocationCount = new Resettable<int>(Interlocked.Increment(ref _concurrentInvocationsCount),
                value => Interlocked.Decrement(ref _concurrentInvocationsCount));
            using var concurrentThreadCount = new Resettable<int>(Interlocked.Add(ref _concurrentThreadsCount, threadCount),
                value => Interlocked.Add(ref _concurrentThreadsCount, -threadCount));

            var allowedThreadCount = GetAllowedThreadCount(concurrentThreadCount.Value, threadCount);

            if (IsGateLimitOpen(MaxParallelInvocations, concurrentInvocationCount.Value) &&
                (threadCount != allowedThreadCount || IsGateLimitOpen(MaxParallelThreads, concurrentThreadCount.Value)))
            {
                Interlocked.Add(ref _totalParallelThreadsConsumed, allowedThreadCount);
                var systemParallelLoopResult = bodyParallelCall(allowedThreadCount);
                parallelLoopResult = new ParallelLoopResult(systemParallelLoopResult.IsCompleted, systemParallelLoopResult.LowestBreakIteration);
                return true;
            }

            parallelLoopResult = DefaultNotCompletedParallelLoopResult;
            return false;
        }

        private bool TryParallel(ParallelInvokeDelegate bodyParallelCall, int threadCount)
        {
            // ReSharper disable once UnusedVariable
            return TryParallel(allowedThreadCount =>
            {
                bodyParallelCall(allowedThreadCount);
                return DefaultSystemParallelLoopResult;
            }, out var parallelLoopResult, threadCount);
        }

        private ParallelLoopResult IterateAndInvokeActionsSerially<T>(IEnumerable<T> items, Action<T> body)
        {
            Interlocked.Increment(ref _totalSerialRunCount);
            List<Exception> exceptions = null;

            foreach (var item in items)
                try
                {
                    body.Invoke(item);
                }
                catch (Exception e)
                {
                    (exceptions ??= new List<Exception>()).Add(e);
                    if (AbortInvocationsOnSerialInvocationException)
                        break;
                }

            if (exceptions != null) 
                throw new AggregateException(exceptions);
            return DefaultCompletedParallelLoopResult;
        }

        private static int MaxDegreeOfParallelism(ParallelOptions parallelOptions, int itemCount)
        {
            return parallelOptions.MaxDegreeOfParallelism != Unlimited ? Math.Min(parallelOptions.MaxDegreeOfParallelism, itemCount) : itemCount;
        }

        private static IEnumerable<long> IterateForLoop(long fromInclusive, long toExclusive)
        {
            for (var idx = fromInclusive; idx < toExclusive; idx++)
                yield return idx;
        }

        #endregion
        #region System Parallel class replacement methods

        public void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            if (TryParallel(allowedThreadCount =>
            {
                parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
                Parallel.Invoke(parallelOptions, actions);
            }, MaxDegreeOfParallelism(parallelOptions, actions.Length))) 
                return;
            IterateAndInvokeActionsSerially(actions, action => action.Invoke());
        }

        public void Invoke(params Action[] actions)
        {
            Invoke(DefaultParallelOptions, actions);
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            var sourceCopy = new List<TSource>(source);
            return TryParallel(allowedThreadCount =>
                {
                    parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
                    return Parallel.ForEach(sourceCopy, parallelOptions, body);
                }, out var parallelLoopResult, MaxDegreeOfParallelism(parallelOptions, sourceCopy.Count))
                ? parallelLoopResult 
                : IterateAndInvokeActionsSerially(sourceCopy, body);
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            return ForEach(source, DefaultParallelOptions, body);
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body)
        {
            return TryParallel(allowedThreadCount =>
                {
                    parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
                    return Parallel.For(fromInclusive, toExclusive, parallelOptions, body);
                },
                out var parallelLoopResult, MaxDegreeOfParallelism(parallelOptions, (int)(toExclusive - fromInclusive)))
                ? parallelLoopResult 
                : IterateAndInvokeActionsSerially(IterateForLoop(fromInclusive, toExclusive), body);
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
        {
            return For(fromInclusive, toExclusive, DefaultParallelOptions, body);
        }

        #endregion
    }
}