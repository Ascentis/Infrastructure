using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.BlockingQueue
{
    public class BlockingQueueDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        private BlockingQueueSourceAdapter _blockingQueueSourceAdapter;
        private readonly ManualResetEventSlim _startedRunning;
        
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

        private void InsertSingle(object obj, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne)
        {
            WaitForPumpStartAndSourceAdapterPrepared();
            var entry = _blockingQueueSourceAdapter.AcquireEntry();
            entry.Value = SerializerObjectToValues.ObjectToValuesArray(obj, entry.Value);
            _blockingQueueSourceAdapter.Insert(entry, onReleaseOne);
        }

        private void InsertGeneric<T>(T obj, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne)
        {
            switch (obj)
            {
                case IEnumerable<T> genericObjects:
                    foreach (var genericObject in genericObjects)
                        InsertGeneric(genericObject, onReleaseOne);
                    return;
                case IEnumerable<object[]> objectArrayEnumerable:
                    foreach (var objArray in objectArrayEnumerable)
                        InsertArray(objArray, onReleaseOne);
                    return;
                case object[] objArray:
                    InsertArray(objArray, onReleaseOne);
                    return;
                case IEnumerable<object> objects:
                    foreach (var aObj in objects)
                        InsertSingle(aObj, onReleaseOne);
                    return;
                default:
                    WaitForPumpStartAndSourceAdapterPrepared();
                    var entry = _blockingQueueSourceAdapter.AcquireEntry();
                    entry.Value = SerializerObjectToValues<T>.ObjectToValuesArray(obj, entry.Value);
                    _blockingQueueSourceAdapter.Insert(entry, onReleaseOne);
                    return;
            }
        }

        public void Insert<T>(T obj)
        {
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
                case IEnumerable<T> genericObjects:
                    return InsertAsyncGeneric(genericObjects.ToList());
                case IEnumerable<object[]> objectArrayEnumerable:
                    return InsertAsyncGeneric(objectArrayEnumerable.ToList());
                case object[] objArray:
                    return InsertAsyncGeneric(objArray);
                case IEnumerable<object> objects:
                    return InsertAsyncGeneric(objects.ToList());
                default:
                    ICollection<T> objs = new[] { obj };
                    return InsertAsyncGeneric(objs);
            }
        }
        #endregion

        public void Finish(bool wait, int timeout = -1)
        {
            _startedRunning.Wait(timeout);
            if (_blockingQueueSourceAdapter == null)
                throw new InvalidOperationException("Can't call BlockingQueueDataPipeline.Finish(). Pump is not running");
            _blockingQueueSourceAdapter.Finish();
            if (wait)
                FinishedEvent.WaitOne(timeout);
        }

        public override void Pump(ISourceAdapter<PoolEntry<object[]>> sourceAdapter,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> targetAdapters)
        {
            _blockingQueueSourceAdapter = sourceAdapter as BlockingQueueSourceAdapter ?? 
                             throw new InvalidOperationException($"sourceAdapter must be of class {nameof(BlockingQueueSourceAdapter)}");
            _startedRunning.Set();
            base.Pump(sourceAdapter, targetAdapters);
        }
    }
}
