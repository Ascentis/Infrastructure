using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ascentis.Infrastructure
{
    /// <summary>
    ///
    /// BoundedParallel can thought as an extension to .NET Parallel class providing more granular control over Parallel behavior as related to the
    /// number of threads that are required to execute. With BoundedParallel concurrent invocations and number of threads can be controlled at the instance level.
    /// 
    /// .NET Parallel doesn't provide any control at a granular level. The only levers that a developer can use are global ThreadPool settings (min threads, max threads, etc.)
    /// Often this settings are not enough to control how parallel executions behave and they tend to be too coarse ultimately affecting the entire application behavior.
    /// In fact, the main motivation to create this class is to avoid runaway situations when multiple concurrent threads call Parallel.* around the same time leading to a race
    /// condition requiring ever more threads and ultimately causing the ThreadPool class to utilize "Hill Climbing" thread provisioning heuristics which limit the creation of
    /// new threads to 2 threads per second max causing queueing and blocking when attempting to schedule executions using ThreadPool jobs.
    ///  
    /// BoundedParallel semantics:
    ///     1. User can control the maximum number concurrent invocations (constructor param maxParallelInvocations) regardless of how many threads will be taken per invocation.
    ///         If the number of concurrent invocations is to be exceeded at any point the call will be served serially in the caller's thread context.
    ///     2. User can control the maximum number of threads consumed per BoundedParallel instance by any number of allowed concurrent invocations (constructor param maxParallelThreads).
    ///         If at any time the number of threads were to be exceeded by a given invocations the number of threads will be adjusted by using
    ///         ParallelOptions.MaxDegreeOfParallelism to fit exactly to the maximum number of threads allowed on the BoundedParallel instance. The minimum level of Parallelism
    ///         required when adjusting the number of threads is two (2) - it makes no sense to use Parallel.* if running only one (1) thread in Parallel while blocking the caller's thread.
    ///     3. If BoundedParallel goes serial on a particular invocation, it will retry to execute using Parallelism after each serial invocation. This allows for parallelism if the
    ///         threads within the ThreadPool are freed during the invocations done serially in another caller's thread.
    ///     4. BoundedParallel will perform the same argument checks than Parallel calls and throw the same exception types.
    ///     5. BoundedParallel will collect exceptions the same way than Parallel and raise a single AggregateException as long as AbortOnSerialInvocationException
    ///         is set to false. Beware that AbortOnSerialInvocationException default value is true, cutting control early in case an exception occurs when executing
    ///         actions serially.
    ///     6. If passing -1 to maxParallelInvocations and maxParallelThreads constructor parameters BoundedParallel will behave the same way as Parallel.* method calls.
    ///
    /// Remarks:
    ///     - Current version of BoundedParallel doesn't provide overload methods that allow for breaking execution the same way than standard Parallel For* methods do. Maybe a future
    ///         improvement.
    ///     - BoundedParallel.For* methods always return BoundedParallel.DefaultCompletedParallelLoopResult which has the following values set:
    ///         isCompleted = true
    ///         lowestBreakIteration = null
    /// 
    /// </summary>

    public class BoundedParallel
    {
        #region Public general declarations

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
        public bool AbortOnSerialInvocationException { get; set; } // Default value is DIFFERENT than Parallel class normal behavior. Set to false to match behavior of Parallel class
        /* If MaxParallelInvocations is exceeded OR MaxParallelThreads is exceeded execution of Action array will be done serially */
        public int MaxParallelInvocations { get; set; } // Set to Unlimited (-1) to disable check on number of parallel invocations
        public int MaxParallelThreads { get; set; } // Set to Unlimited (-1) to disable check on number of potential active - utilized - threads
        
        #endregion

        #region Constructor and public methods

        public BoundedParallel(int maxParallelInvocations = DefaultMaxParallelInvocations, int maxParallelThreads = Unlimited)
        {
            MaxParallelInvocations = maxParallelInvocations;
            MaxParallelThreads = maxParallelThreads;
            AbortOnSerialInvocationException = true;
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

        private static void SystemParallelForEach<T>(IEnumerable<T> source, int allowedThreadCount, ParallelOptions parallelOptions, Action<T> body)
        {
            parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
            Parallel.ForEach(source, parallelOptions, body);
        }

        private void IterateInvokingActionsSeriallyRecurrentlyRetryParallel<T>(IEnumerable<T> items, Action<T> body)
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
                    if (AbortOnSerialInvocationException)
                        break;
                }
                try
                {
                    // After each serial invocation in caller's thread context we will try to run Parallel again with the remaining items in the queue
                    if (!TryParallel(allowedThreadCount => SystemParallelForEach(itemsQueue, allowedThreadCount, new ParallelOptions(), body), itemsQueue.Count))
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

        #endregion

        #region System Parallel class replacement methods

        public void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(new []
            {
                ArgsChecker.Arg(parallelOptions, nameof(parallelOptions)), 
                ArgsChecker.Arg(actions, nameof(actions))
            });
            ArgsChecker.CheckForNull<ArgumentException>(actions.ToArray<object>(), "null action found calling BoundedParallel.Invoke(ParallelOptions, Action[]");
            if (TryParallel(allowedThreadCount =>
            {
                parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
                Parallel.Invoke(parallelOptions, actions);
            }, MaxDegreeOfParallelism(parallelOptions, actions.Length))) 
                return;
            IterateInvokingActionsSeriallyRecurrentlyRetryParallel(actions, action => action.Invoke());
        }

        public void Invoke(params Action[] actions)
        {
            Invoke(new ParallelOptions(), actions);
        }

        public ParallelLoopResult ForEach<T>(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T> body)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(new []
            {
                ArgsChecker.Arg(source, nameof(source)), 
                ArgsChecker.Arg(parallelOptions, nameof(parallelOptions)), 
                ArgsChecker.Arg(body, nameof(body))
            });
            var sourceCopy = new List<T>(source);
            if (!TryParallel(allowedThreadCount => SystemParallelForEach(sourceCopy, allowedThreadCount, parallelOptions, body), MaxDegreeOfParallelism(parallelOptions, sourceCopy.Count))) 
                IterateInvokingActionsSeriallyRecurrentlyRetryParallel(sourceCopy, body);
            return DefaultCompletedParallelLoopResult;
        }

        public ParallelLoopResult ForEach<T>(IEnumerable<T> source, Action<T> body)
        {
            return ForEach(source, new ParallelOptions(), body);
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(new []
            {
                ArgsChecker.Arg(parallelOptions, nameof(parallelOptions)), 
                ArgsChecker.Arg(body, nameof(body))
            });
            if (!TryParallel(allowedThreadCount =>
                {
                    parallelOptions.MaxDegreeOfParallelism = allowedThreadCount;
                    Parallel.For(fromInclusive, toExclusive, parallelOptions, body);
                }, MaxDegreeOfParallelism(parallelOptions, (int)(toExclusive - fromInclusive)))) 
                IterateInvokingActionsSeriallyRecurrentlyRetryParallel(IterateForLoop(fromInclusive, toExclusive), body);
            return DefaultCompletedParallelLoopResult;
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
        {
            return For(fromInclusive, toExclusive, new ParallelOptions(), body);
        }

        public ParallelLoopResult For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int> body)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(new object [] { body }, nameof(body));
            return For(fromInclusive, (long) toExclusive, parallelOptions, value => body.Invoke((int) value));
        }

        public ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int> body)
        {
            return For(fromInclusive, toExclusive, new ParallelOptions(), body);
        }

        #endregion
    }
}