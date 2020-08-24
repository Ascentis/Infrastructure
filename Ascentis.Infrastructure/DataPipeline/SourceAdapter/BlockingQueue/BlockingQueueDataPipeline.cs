using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer;

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

        private void InsertGeneric<T>(T obj, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne, Serializer<T> serializer)
        {
            switch (obj)
            {
                case object[] objArray:
                    InsertArray(objArray, onReleaseOne);
                    return;
                default:
                    WaitForPumpStartAndSourceAdapterPrepared();
                    var entry = _blockingQueueSourceAdapter.AcquireEntry();
                    entry.Value = serializer?.ToValues(obj, entry.Value) ?? (typeof(T) == typeof(object) 
                        ? Serializer<object>.ToValues(obj, entry.Value)
                        : Serializer<T>.ToValues(obj, entry.Value));
                    _blockingQueueSourceAdapter.Insert(entry, onReleaseOne);
                    return;
            }
        }

        public void Insert<T>(T obj, Serializer<T> serializer = null)
        {
            switch (obj)
            {
                case object[] objArray:
                    InsertGeneric(objArray, null, null);
                    return;
                case IEnumerable<object[]> objectArrayEnumerable:
                    foreach (var objArray in objectArrayEnumerable)
                        InsertGeneric(objArray, null, null);
                    return;
                case IEnumerable<object> objects:
                    foreach (var aObj in objects)
                        InsertGeneric(aObj, null, (Serializer<object>)serializer);
                    return;
                default:
                    InsertGeneric(obj, null, serializer);
                    return;
            }
        }

        public void Insert<T>(IEnumerable<T> objs, Serializer<T> serializer = null)
        {
            foreach (var obj in objs)
                InsertGeneric(obj, null, serializer);
        }
        #endregion

        #region Insert async
        private Task InsertAsyncGeneric<T>(ICollection<T> objs, Serializer<T> serializer)
        {
            var objectsCount = objs.Count;
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.None);
            foreach (var obj in objs)
            {
                InsertGeneric(obj, poolEntry =>
                {
                    if (Interlocked.Decrement(ref objectsCount) == 0)
                        tcs.SetResult(true);
                }, serializer);
            }

            return tcs.Task;
        }

        public Task InsertAsync<T>(T obj, Serializer<T> serializer = null)
        {
            switch (obj)
            {
                case object[] objArray:
                    return InsertAsyncGeneric(objArray, null);
                case IEnumerable<object[]> objectArrayEnumerable:
                    return InsertAsyncGeneric(objectArrayEnumerable.ToList(), null);
                case IEnumerable<object> objects:
                    return InsertAsyncGeneric(objects.ToList(), (Serializer<object>)serializer);
                case IEnumerable<T> genericObjects:
                    return InsertAsyncGeneric(genericObjects.ToList(), serializer);
                default:
                    ICollection<T> objs = new[] { obj };
                    return InsertAsyncGeneric(objs, serializer);
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
