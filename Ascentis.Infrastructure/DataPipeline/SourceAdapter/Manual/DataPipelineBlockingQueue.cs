using System;
using System.Collections.Generic;
using System.Linq;
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

        private void WaitForPumpStart()
        {
            _startedRunning.Wait();
            if (_sourceAdapter == null)
                throw new InvalidOperationException("Can't call DataPipelineBlockingQueue.Insert(). Pump is not running");
        }

        private void Insert(object[] obj, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne)
        {
            WaitForPumpStart();
            _sourceAdapter.Insert(obj, onReleaseOne);
        }

        public void Insert(object[] obj)
        {
            Insert(obj, null);
        }

        public void Insert(IEnumerable<object[]> objs)
        {
            foreach (var obj in objs)
                Insert(obj);
        }

        public Task InsertAsync(object[] obj)
        {
            IEnumerable<object[]> objs = new [] { obj };
            return InsertAsync(objs);
        }

        public Task InsertAsync(IEnumerable<object[]> objs)
        {
            WaitForPumpStart();

            var objsList = objs.ToList();
            var objectsCount = objsList.Count;
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.None);
            foreach (var obj in objsList)
                Insert(obj, poolEntry =>
                {
                    if (Interlocked.Decrement(ref objectsCount) == 0)
                        tcs.SetResult(true);
                });

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
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> targetAdapters)
        {
            _sourceAdapter = sourceAdapter as SourceAdapterBlockingQueue ?? 
                             throw new InvalidOperationException($"sourceAdapter must be of class {nameof(SourceAdapterBlockingQueue)}");
            _startedRunning.Set();
            base.Pump(sourceAdapter, targetAdapters);
        }
    }
}
