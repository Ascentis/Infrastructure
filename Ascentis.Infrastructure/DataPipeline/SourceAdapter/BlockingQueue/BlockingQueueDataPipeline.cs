using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.BlockingQueue
{
    public class BlockingQueueDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        public delegate void FlushEventDelegate(IAdapter targetAdapter);

        public event FlushEventDelegate BeforeFlushEvent;
        public event FlushEventDelegate AfterFlushEvent;

        private BlockingQueueSourceAdapter _blockingQueueSourceAdapter;
        private readonly ManualResetEventSlim _startedRunning;
        private static readonly object FlushSignal = new object[1];
        
        public BlockingQueueDataPipeline() 
        {
            _startedRunning = new ManualResetEventSlim(false);
        }

        private void WaitForPumpStartAndSourceAdapterPrepared()
        {
            _startedRunning.Wait();
            if (_blockingQueueSourceAdapter == null)
                throw new InvalidOperationException("Can't call BlockingQueueDataPipeline.Insert(). Pump is not running");
            _blockingQueueSourceAdapter.WaitPrepared();
        }

        #region Insert sync
        private void InsertArray(object[] obj, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne)
        {
            WaitForPumpStartAndSourceAdapterPrepared();
            _blockingQueueSourceAdapter.Insert(obj, onReleaseOne);
        }

        private void InsertGeneric<T>(T obj, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne)
        {
            switch (obj)
            {
                case object[] objArray:
                    InsertArray(objArray, onReleaseOne);
                    return;
                default:
                    WaitForPumpStartAndSourceAdapterPrepared();
                    var entry = _blockingQueueSourceAdapter.AcquireEntry();
                    entry.Value = typeof(T) == typeof(object) 
                        ? SerializerObjectToValues.ObjectToValuesArray(obj, entry.Value) 
                        : SerializerObjectToValues<T>.ObjectToValuesArray(obj, entry.Value);
                    _blockingQueueSourceAdapter.Insert(entry, onReleaseOne);
                    return;
            }
        }

        public void Insert<T>(T obj)
        {
            switch (obj)
            {
                case object[] objArray:
                    InsertGeneric(objArray, null);
                    return;
                case IEnumerable<object[]> objectArrayEnumerable:
                    foreach (var objArray in objectArrayEnumerable)
                        InsertGeneric(objArray, null);
                    return;
                case IEnumerable<object> objects:
                    foreach (var aObj in objects)
                        InsertGeneric(aObj, null);
                    return;
                default:
                    InsertGeneric(obj, null);
                    return;
            }
        }

        public void Insert<T>(IEnumerable<T> objs)
        {
            foreach (var obj in objs)
                InsertGeneric(obj, null);
        }
        #endregion

        #region Insert async
        private Task InsertAsyncGeneric<T>(ICollection<T> objs)
        {
            var objectsCount = objs.Count;
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.None);
            foreach (var obj in objs)
            {
                InsertGeneric(obj, poolEntry =>
                {
                    if (Interlocked.Decrement(ref objectsCount) == 0)
                        tcs.SetResult(true);
                });
            }

            return tcs.Task;
        }

        public Task InsertAsync<T>(T obj)
        {
            switch (obj)
            {
                case object[] objArray:
                    return InsertAsyncGeneric(objArray);
                case IEnumerable<object[]> objectArrayEnumerable:
                    return InsertAsyncGeneric(objectArrayEnumerable.ToList());
                case IEnumerable<object> objects:
                    return InsertAsyncGeneric(objects.ToList());
                case IEnumerable<T> genericObjects:
                    return InsertAsyncGeneric(genericObjects.ToList());
                default:
                    ICollection<T> objs = new[] { obj };
                    return InsertAsyncGeneric(objs);
            }
        }
        #endregion

        public void InsertFlushEvent()
        {
            Insert(FlushSignal);
        }

        public void Finish(bool wait, int timeout = -1)
        {
            _startedRunning.Wait(timeout);
            if (_blockingQueueSourceAdapter == null)
                throw new InvalidOperationException("Can't call BlockingQueueDataPipeline.Finish(). Pump is not running");
            _blockingQueueSourceAdapter.Finish();
            if (wait)
                FinishedEvent.WaitOne(timeout);
        }

        private TargetAdapter.Base.TargetAdapter.BeforeProcessRowResult AttemptFlush(IAdapter targetAdapter, PoolEntry<object[]> row)
        {
            if (row.Value != FlushSignal)
                return TargetAdapter.Base.TargetAdapter.BeforeProcessRowResult.Continue;
            if (!(targetAdapter is ITargetAdapterFlushable flushable))
                return TargetAdapter.Base.TargetAdapter.BeforeProcessRowResult.Abort;
            BeforeFlushEvent?.Invoke(targetAdapter);
            flushable.Flush();
            AfterFlushEvent?.Invoke(targetAdapter);
            return TargetAdapter.Base.TargetAdapter.BeforeProcessRowResult.Abort;
        }

        public override void Pump(ISourceAdapter<PoolEntry<object[]>> sourceAdapter,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> targetAdapters)
        {
            _blockingQueueSourceAdapter = sourceAdapter as BlockingQueueSourceAdapter ?? 
                             throw new InvalidOperationException($"sourceAdapter must be of class {nameof(BlockingQueueSourceAdapter)}");
            _startedRunning.Set();
            BeforeTargetAdapterProcessRow += AttemptFlush;
            try
            {
                base.Pump(sourceAdapter, targetAdapters);
            }
            finally
            {
                BeforeTargetAdapterProcessRow -= AttemptFlush;
            }
        }
    }
}
