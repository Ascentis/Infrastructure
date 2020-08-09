using System;
using System.Collections.Generic;
using System.Threading;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Manual
{
    public class DataPipelineManual<T> : DataPipeline<PoolEntry<T>>
    {
        private SourceAdapterManual<PoolEntry<T>> _sourceAdapter;
        private readonly ManualResetEventSlim _startedRunning;

        public DataPipelineManual() 
        {
            _startedRunning = new ManualResetEventSlim(false);
        }

        public void Insert(T obj)
        {
            _startedRunning.Wait();
            if (_sourceAdapter == null)
                throw new InvalidOperationException("Can't call DataPipelineManual.Insert(). Pump is not running");
            _sourceAdapter.Insert(new PoolEntry<T>(obj));
        }

        public void Insert(IEnumerable<T> objs)
        {
            foreach (var obj in objs)
                Insert(obj);
        }

        public void Finish(bool wait, int timeout = -1)
        {
            _startedRunning.Wait(timeout);
            if (_sourceAdapter == null)
                throw new InvalidOperationException("Can't call DataPipelineManual.Finish(). Pump is not running");
            _sourceAdapter.Finish();
            if (wait)
                FinishedEvent.WaitOne(timeout);
        }

        public override void Pump(ISourceAdapter<PoolEntry<T>> sourceAdapter,
            IEnumerable<ITargetAdapter<PoolEntry<T>>> dataPipelineTargetAdapters)
        {
            _sourceAdapter = sourceAdapter as SourceAdapterManual<PoolEntry<T>> ?? 
                             throw new InvalidOperationException($"sourceAdapter must be of class {nameof(SourceAdapterManual<PoolEntry<T>>)}");
            _startedRunning.Set();
            base.Pump(sourceAdapter, dataPipelineTargetAdapters);
        }
    }
}
