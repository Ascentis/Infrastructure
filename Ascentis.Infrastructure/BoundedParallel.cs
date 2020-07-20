using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ascentis.Infrastructure
{
    public class BoundedParallel
    {
        private delegate ParallelLoopResult ParallelLoopDelegate();
        private delegate void ParallelInvokeDelegate();

        private static readonly ParallelLoopResult DefaultParallelLoopResult = new ParallelLoopResult();
        private static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();

        private volatile int _serialRunCount;
        public int SerialRunCount => _serialRunCount;

        private readonly int _maxParallelInvocations;

        public BoundedParallel(int maxParallelInvocations = 2)
        {
            _maxParallelInvocations = maxParallelInvocations;
        }

        public void ResetSerialRunCount()
        {
            _serialRunCount = 0;
        }

        private bool TryParallel(ParallelLoopDelegate bodyParallelCall, out ParallelLoopResult parallelLoopResult)
        {
            try
            {
                if (Interlocked.Increment(ref _concurrentInvocationsCount) <= _maxParallelInvocations)
                {
                    parallelLoopResult = bodyParallelCall();
                    return true;
                }
            }
            finally
            {
                Interlocked.Decrement(ref _concurrentInvocationsCount);
            }
            parallelLoopResult = DefaultParallelLoopResult;
            return false;
        }

        private bool TryParallel(ParallelInvokeDelegate bodyParallelCall)
        {
            // ReSharper disable once UnusedVariable
            return TryParallel(() =>
            {
                bodyParallelCall();
                return DefaultParallelLoopResult;
            }, out var parallelLoopResult);
        }

        private void IncrementSerialRunCount()
        {
            Interlocked.Increment(ref _serialRunCount);
        }

        private volatile int _concurrentInvocationsCount;
        public void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            if (TryParallel(() =>
            {
                Parallel.Invoke(parallelOptions, actions);
            })) return;
            IncrementSerialRunCount();
            foreach (var action in actions)
                action.Invoke();
        }

        public void Invoke(params Action[] actions)
        {
            Invoke(DefaultParallelOptions, actions);
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions,
            Action<TSource> body)
        {
            if (TryParallel(() => Parallel.ForEach(source, parallelOptions, body), out var parallelLoopResult)) return parallelLoopResult;
            IncrementSerialRunCount();
            foreach (var item in source)
                body.Invoke(item);
            return DefaultParallelLoopResult;
        }

        public ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            return ForEach(source, DefaultParallelOptions, body);
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body)
        {
            if (TryParallel(() => Parallel.For(fromInclusive, toExclusive, parallelOptions, body), out var parallelLoopResult)) return parallelLoopResult;
            IncrementSerialRunCount();
            for (var idx = fromInclusive; idx < toExclusive; idx++)
                body.Invoke(idx);
            return DefaultParallelLoopResult;
        }

        public ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
        {
            return For(fromInclusive, toExclusive, DefaultParallelOptions, body);
        }
    }
}
