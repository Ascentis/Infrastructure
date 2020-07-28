using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable ConvertToLambdaExpression

namespace Ascentis.Infrastructure
{
    public class BoundedParallel
    {
        #region Public general declarations

        //public delegate System.Threading.Tasks.ParallelLoopResult ParallelLoopDelegate(int allowedThreadCount);
        public delegate void ParallelInvokeDelegate(int allowedThreadCount);
        public const int DefaultMaxParallelInvocations = 2;
        public const int Unlimited = -1; // -1 equals to no limit in the number of threads or invocations that *could* run in parallel before going serial
        public const int MinThreadCountToGrantParallelism = 2;

        #endregion

        #region Private declarations

        private static readonly ParallelLoopResult DefaultCompletedParallelLoopResult = new ParallelLoopResult(true, null);

        private readonly ConcurrentIncrementableResettableInt _concurrentInvocationsCount;
        private readonly ConcurrentIncrementableResettableInt _concurrentThreadsCount;
        
        #endregion

        #region Public properties

        public BoundedParallelStats Stats { get; }
        public int ConcurrentInvocationsCount => _concurrentInvocationsCount.Value;
        public int ConcurrentThreadsCount => _concurrentThreadsCount.Value;
        public bool AbortInvocationsOnSerialInvocationException { get; set; } // Default value is DIFFERENT than Parallel class normal behavior. Set to false to match behavior of Parallel class
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
            Stats = new BoundedParallelStats();
            _concurrentInvocationsCount = new ConcurrentIncrementableResettableInt();
            _concurrentThreadsCount = new ConcurrentIncrementableResettableInt();
        }

        public void ResetAllStats()
        {
            Stats.ResetTotalParallelRunCount();
            Stats.ResetTotalSerialRunCount();
            Stats.ResetTotalParallelsThreadConsumed();
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
            // Only if we can fit more than MinThreadCountToGrantParallelism threads we will return the delta
            return deltaAllowedThreads >= MinThreadCountToGrantParallelism ? deltaAllowedThreads : requestedThreadCount;
        }

        private bool TryParallel(ParallelInvokeDelegate bodyParallelCall, int threadCount)
        {
            // If MinThreadCountToGrantParallelism threads or less are requested, we will shortcut any evaluation to attempt parallelism
            if (threadCount < MinThreadCountToGrantParallelism)
                return false;

            using var concurrentInvocationsCountSnapshot = _concurrentInvocationsCount.Increment();
            using var concurrentThreadsCountSnapshot = _concurrentThreadsCount.Increment(threadCount);

            var allowedThreadCount = GetAllowedThreadCount(concurrentThreadsCountSnapshot.Value, threadCount);

            if (!IsGateLimitOpen(MaxParallelInvocations, concurrentInvocationsCountSnapshot.Value) ||
                threadCount == allowedThreadCount && !IsGateLimitOpen(MaxParallelThreads, concurrentThreadsCountSnapshot.Value)) 
                return false;
            Stats.IncrementParallelThreadsConsumed(allowedThreadCount);
            Stats.IncrementParallelRunCount();
            bodyParallelCall(allowedThreadCount);
            return true;
        }

        private void IterateAndInvokeActionsSerially<T>(IEnumerable<T> items, Action<T> body)
        {
            Stats.IncrementSerialRunCount();
            List<Exception> exceptions = null;
            var itemsQueue = new Queue<T>(items);

            while (itemsQueue.Count > 0)
            {
                var item = itemsQueue.Dequeue();
                try
                {
                    body.Invoke(item);
                }
                catch (Exception e)
                {
                    AutoInit.Ref(ref exceptions).Add(e);
                    if (AbortInvocationsOnSerialInvocationException)
                        break;
                }
                try
                {
                    // After each serial invocation in caller's thread we will try to run Parallel again with the remaining items in the queue
                    if (!TryParallel(allowedThreadCount =>
                    {
                        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = allowedThreadCount };
                        Parallel.ForEach(itemsQueue, parallelOptions, body.Invoke);
                    }, itemsQueue.Count))
                        continue;
                    if (exceptions == null)
                        return;
                }
                catch (AggregateException aggregateException)
                {
                    AutoInit.Ref(ref exceptions).AddRange(aggregateException.InnerExceptions);
                }
                break;
            }

            if (exceptions != null) 
                throw new AggregateException(exceptions);
        }

        private static int MaxDegreeOfParallelism(ParallelOptions parallelOptions, int itemCount)
        {
            return parallelOptions.MaxDegreeOfParallelism != Unlimited ? Math.Min(parallelOptions.MaxDegreeOfParallelism, itemCount) : itemCount;
        }

        private static IEnumerable<long> IterateForLoop(long fromInclusive, long toExclusive)
        {
            while (fromInclusive < toExclusive)
                yield return fromInclusive++;
        }

        private static void CheckForNullArguments<TE>(IEnumerable<object> args) where TE : Exception, new()
        {
            if (args.All(arg => arg != null)) 
                return;
            throw new TE();
        }

        #endregion

        #region System Parallel class replacement methods

        public void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            CheckForNullArguments<ArgumentNullException>(new object[] {parallelOptions, actions});
            CheckForNullArguments<ArgumentException>(actions.ToArray<object>());
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
            Invoke(new ParallelOptions(), actions);
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            CheckForNullArguments<ArgumentNullException>(new object[] {source, parallelOptions, body});
            var sourceCopy = new List<TSource>(source);
            if (!TryParallel(allowedThreadCount =>
                {
                    parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
                    Parallel.ForEach(sourceCopy, parallelOptions, body);
                }, MaxDegreeOfParallelism(parallelOptions, sourceCopy.Count))) 
                IterateAndInvokeActionsSerially(sourceCopy, body);
            return DefaultCompletedParallelLoopResult;
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            return ForEach(source, new ParallelOptions(), body);
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body)
        {
            CheckForNullArguments<ArgumentNullException>(new object[] {parallelOptions, body});
            if (!TryParallel(allowedThreadCount =>
                {
                    parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
                    Parallel.For(fromInclusive, toExclusive, parallelOptions, body);
                }, MaxDegreeOfParallelism(parallelOptions, (int)(toExclusive - fromInclusive)))) 
                IterateAndInvokeActionsSerially(IterateForLoop(fromInclusive, toExclusive), body);
            return DefaultCompletedParallelLoopResult;
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
        {
            return For(fromInclusive, toExclusive, new ParallelOptions(), body);
        }

        #endregion
    }
}