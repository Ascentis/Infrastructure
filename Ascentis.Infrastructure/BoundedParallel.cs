using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable ConvertToLambdaExpression

namespace Ascentis.Infrastructure
{
    public class BoundedParallel
    {
        public const int DefaultMaxParallelInvocations = 2;
        public const int DefaultMaxParallelThreads = -1; // No limit in the number of threads that *could* run in parallel before going serial

        private delegate System.Threading.Tasks.ParallelLoopResult ParallelLoopDelegate();
        private delegate void ParallelInvokeDelegate();

        private static readonly System.Threading.Tasks.ParallelLoopResult DefaultSystemParallelLoopResult = new System.Threading.Tasks.ParallelLoopResult();
        private static readonly ParallelLoopResult DefaultParallelLoopResult = new ParallelLoopResult(true, null);
        private static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();

        private volatile int _serialRunCount;
        private volatile int _concurrentInvocationsCount;
        private volatile int _concurrentThreadsCount;

        public int SerialRunCount => _serialRunCount; // This primarily of use when unit testing
        public bool AbortInvocationsOnSerialInvocationException { get; set; }
        /* If MaxParallelInvocations is exceeded OR MaxParallelThreads is exceeded execution of Action array will be done serially */
        public int MaxParallelInvocations { get; set; }
        public int MaxParallelThreads { get; set; }

        public BoundedParallel(int maxParallelInvocations = DefaultMaxParallelInvocations, int defaultMaxParallelThreads = DefaultMaxParallelThreads)
        {
            MaxParallelInvocations = maxParallelInvocations;
            MaxParallelThreads = defaultMaxParallelThreads;
            AbortInvocationsOnSerialInvocationException = true;
        }

        public void ResetSerialRunCount()
        {
            _serialRunCount = 0;
        }

        private bool TryParallel(ParallelLoopDelegate bodyParallelCall, out ParallelLoopResult parallelLoopResult, int threadCount)
        {
            try
            {
                var concurrentInvocationCount = Interlocked.Increment(ref _concurrentInvocationsCount);
                var concurrentThreadCount = Interlocked.Add(ref _concurrentThreadsCount, threadCount);
                if ((MaxParallelInvocations == -1 || concurrentInvocationCount <= MaxParallelInvocations) && (MaxParallelThreads == -1 || concurrentThreadCount <= MaxParallelThreads))
                {
                    var result = bodyParallelCall();
                    parallelLoopResult = new ParallelLoopResult(result.IsCompleted, result.LowestBreakIteration);
                    return true;
                }
            }
            finally
            {
                Interlocked.Add(ref _concurrentThreadsCount, -threadCount);
                Interlocked.Decrement(ref _concurrentInvocationsCount);
            }
            parallelLoopResult = DefaultParallelLoopResult;
            return false;
        }

        private bool TryParallel(ParallelInvokeDelegate bodyParallelCall, int threadCount)
        {
            // ReSharper disable once UnusedVariable
            return TryParallel(() =>
            {
                bodyParallelCall();
                return DefaultSystemParallelLoopResult;
            }, out var parallelLoopResult, threadCount);
        }

        private ParallelLoopResult IterateAndInvokeActions<T>(IEnumerable<T> items, Action<T> body)
        {
            Interlocked.Increment(ref _serialRunCount);
            var exceptions = new List<Exception>();
            foreach (var item in items)
            {
                try
                {
                    body.Invoke(item);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (AbortInvocationsOnSerialInvocationException)
                        break;
                }
            }
            if (exceptions.Count > 0) 
                throw new AggregateException(exceptions);
            return DefaultParallelLoopResult;
        }

        private static int MaxDegreeOfParallelism(ParallelOptions parallelOptions, int itemCount)
        {
            return parallelOptions.MaxDegreeOfParallelism != -1 ? Math.Min(parallelOptions.MaxDegreeOfParallelism, itemCount) : itemCount;
        }

        public void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            if (TryParallel(() =>
            {
                Parallel.Invoke(parallelOptions, actions);
            }, MaxDegreeOfParallelism(parallelOptions, actions.Length))) 
                return;
            IterateAndInvokeActions(actions, action => action.Invoke());
        }

        public void Invoke(params Action[] actions)
        {
            Invoke(DefaultParallelOptions, actions);
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            var sourceCopy = new List<TSource>(source);
            return TryParallel(() =>
            {
                return Parallel.ForEach(sourceCopy, parallelOptions, body);
            }, out var parallelLoopResult, MaxDegreeOfParallelism(parallelOptions, sourceCopy.Count)) 
                ? parallelLoopResult 
                : IterateAndInvokeActions(sourceCopy, body);
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            return ForEach(source, DefaultParallelOptions, body);
        }

        private static IEnumerable<long> IterateForLoop(long fromInclusive, long toExclusive)
        {
            for (var idx = fromInclusive; idx < toExclusive; idx++)
                yield return idx;
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body)
        {
            return TryParallel(() =>
            {
                return Parallel.For(fromInclusive, toExclusive, parallelOptions, body);
            }, out var parallelLoopResult, MaxDegreeOfParallelism(parallelOptions, (int)(toExclusive - fromInclusive))) 
                ? parallelLoopResult 
                : IterateAndInvokeActions(IterateForLoop(fromInclusive, toExclusive), body);
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
        {
            return For(fromInclusive, toExclusive, DefaultParallelOptions, body);
        }
    }
}
