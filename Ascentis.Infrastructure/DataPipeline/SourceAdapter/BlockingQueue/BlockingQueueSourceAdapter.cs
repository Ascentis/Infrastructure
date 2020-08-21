using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.BlockingQueue
{
    public class BlockingQueueSourceAdapter : SourceAdapter<PoolEntry<object[]>>
    {
        public delegate void SourceAdapterDelegate(BlockingQueueSourceAdapter adapter);
        public const int DefaultPoolCapacity = 1000;

        private readonly ConcurrentQueue<PoolEntry<object[]>> _dataQueue;
        private readonly ManualResetEventSlim _dataAvailable;
        private Pool<object[]> _rowsPool;
        private volatile bool _finished;
        private readonly ManualResetEventSlim _preparedEvent;

        public event SourceAdapterDelegate OnWaitForDataTimeout;

        public int WaitForDataTimeout { get; set; }

        public BlockingQueueSourceAdapter()
        {
            _dataAvailable = new ManualResetEventSlim(false);
            _dataQueue = new ConcurrentQueue<PoolEntry<object[]>>();
            _preparedEvent = new ManualResetEventSlim(false);
            WaitForDataTimeout = -1;
        }

        public BlockingQueueSourceAdapter(IEnumerable sourceCollection) : this()
        {
            foreach (var obj in sourceCollection)
            {
                switch (obj)
                {
                    case PoolEntry<object[]> entry:
                        _dataQueue.Enqueue(entry);
                        break;
                    case object[] objects:
                    {
                        var poolEntry = new PoolEntry<object[]>(objects);
                        _dataQueue.Enqueue(poolEntry);
                        break;
                    }
                    default:
                    {
                        var objsArray = new [] {obj};
                        _dataQueue.Enqueue(new PoolEntry<object[]>(objsArray));
                        break;
                    }
                }
            }
            Finish();
        }

        public override IEnumerable<PoolEntry<object[]>> RowsEnumerable
        {
            get
            {
                while (true)
                {
                    if (!_dataQueue.TryDequeue(out var obj))
                    {
                        if (_finished)
                            yield break;
                        if (!_dataAvailable.Wait(WaitForDataTimeout))
                            OnWaitForDataTimeout?.Invoke(this);
                        _dataAvailable.Reset();
                        continue;
                    }

                    yield return obj;
                }
            }
        }

        public PoolEntry<object[]> AcquireEntry()
        {
            return _rowsPool.Acquire();
        }

        public void WaitPrepared()
        {
            _preparedEvent.Wait();
        }

        public void Insert(object[] obj, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne)
        {
            WaitPrepared();
            var entry = AcquireEntry();
            entry.Value = obj;
            Insert(entry, onReleaseOne);
        }

        public void Insert(PoolEntry<object[]> entry, PoolEntry<object[]>.PoolEntryDelegate onReleaseOne)
        {
            WaitPrepared();
            entry.OnReleaseOne += onReleaseOne;
            _dataQueue.Enqueue(entry);
            _dataAvailable.Set();
        }

        public override void Prepare()
        {
            base.Prepare();
            _rowsPool = new Pool<object[]>(DefaultPoolCapacity, pool => pool.NewPoolEntry(null, ParallelLevel));
            _preparedEvent.Set();
        }

        public void Finish()
        {
            _finished = true;
            _dataAvailable.Set();
        }

        public override void ReleaseRow(PoolEntry<object[]> row)
        {
            _rowsPool.Release(row);
        }
    }
}
