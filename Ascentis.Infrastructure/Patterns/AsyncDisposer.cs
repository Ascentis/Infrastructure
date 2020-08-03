using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class AsyncDisposer
    {
        private static int _disposalInterval = 5000;
        public static int DisposeInterval
        {
            get => _disposalInterval;
            set
            {
                if (value == _disposalInterval)
                    return;
                _disposalInterval = value;
                DisposalTimer.Change(value, value);
            }
        }

        private static readonly Timer DisposalTimer;
        private static readonly ConcurrentObjectAccessor<ConcurrentQueue<IDisposable>> Disposables;

        static AsyncDisposer()
        {
            Disposables = new ConcurrentObjectAccessor<ConcurrentQueue<IDisposable>>();

            (DisposalTimer = new Timer(state =>
            {
                Disposables.SwapNewAndExecute(gateOpenDelegate:disposables => !disposables.IsEmpty, cleanupOldReference: disposables =>
                {
                    while (disposables.TryDequeue(out var item))
                        item.Dispose();
                });
            })).Change(DisposeInterval, DisposeInterval);
        }

        public AsyncDisposer()
        {
            throw new InvalidOperationException("Creating instances of AsyncDisposer() not supported. Class designed to be used as a global singleton");
        }

        public static void Enqueue(IDisposable disposable)
        {
            Disposables.ExecuteReadLocked(disposables =>
            {
                disposables.Enqueue(disposable);
            });
        }
    }
}
