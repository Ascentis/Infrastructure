using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Manual
{
    public class DataPipelineBlockingQueue : DataPipeline<PoolEntry<object[]>>
    {
        private SourceAdapterBlockingQueue _sourceAdapter;
        private readonly ManualResetEventSlim _startedRunning;

        public DataPipelineBlockingQueue() 
        {
            _startedRunning = new ManualResetEventSlim(false);
        }

        private void Insert(PoolEntry<object[]> entry)
        {
            _startedRunning.Wait();
            if (_sourceAdapter == null)
                throw new InvalidOperationException("Can't call DataPipelineBlockingQueue.Insert(). Pump is not running");
            _sourceAdapter.Insert(entry);
        }

        public void Insert(object[] obj)
        {
            Insert(new PoolEntry<object[]>(obj));
        }

        public void Insert(IEnumerable<object[]> objs)
        {
            foreach (var obj in objs)
                Insert(obj);
        }

        public Task InsertAsync(object[] obj)
        {
            var entry = new PoolEntry<object[]>(obj);
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.None);
            entry.OnReleaseOne += poolEntry => tcs.SetResult(true);
            Insert(entry);
            return tcs.Task;
        }

        public Task InsertAsync(IEnumerable<object[]> objs)
        {
            var objectsCount = 0;
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.None);
            foreach (var obj in objs)
            {
                objectsCount++;
                var entry = new PoolEntry<object[]>(obj);
                entry.OnReleaseOne += poolEntry =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    if (Interlocked.Decrement(ref objectsCount) <= 0)
                        tcs.SetResult(true);
                };
                Insert(entry);
            }

            return tcs.Task;
        }

        public void Finish(bool wait, int timeout = -1)
        {
            _startedRunning.Wait(timeout);
            if (_sourceAdapter == null)
                throw new InvalidOperationException("Can't call DataPipelineBlockingQueue.Finish(). Pump is not running");
            _sourceAdapter.Finish();
            if (wait)
                FinishedEvent.WaitOne(timeout);
        }

        public override void Pump(ISourceAdapter<PoolEntry<object[]>> sourceAdapter,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters)
        {
            _sourceAdapter = sourceAdapter as SourceAdapterBlockingQueue ?? 
                             throw new InvalidOperationException($"sourceAdapter must be of class {nameof(SourceAdapterBlockingQueue)}");
            _startedRunning.Set();
            base.Pump(sourceAdapter, dataPipelineTargetAdapters);
        }
    }
}
