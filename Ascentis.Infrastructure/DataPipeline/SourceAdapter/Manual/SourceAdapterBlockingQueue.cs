using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Manual
{
    public class SourceAdapterBlockingQueue : SourceAdapter<PoolEntry<object[]>>
    {
        private readonly ConcurrentQueue<PoolEntry<object[]>> _dataQueue;
        private readonly ManualResetEventSlim _dataAvailable;
        private volatile bool _finished;

        public SourceAdapterBlockingQueue()
        {
            _dataAvailable = new ManualResetEventSlim(false);
            _dataQueue = new ConcurrentQueue<PoolEntry<object[]>>();
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
                        _dataAvailable.Wait();
                        _dataAvailable.Reset();
                        continue;
                    }

                    yield return obj;
                }
            }
        }

        public void Insert(PoolEntry<object[]> obj)
        {
            _dataQueue.Enqueue(obj);
            _dataAvailable.Set();
        }

        public void Finish()
        {
            _finished = true;
            _dataAvailable.Set();
        }

        public override void ReleaseRow(PoolEntry<object[]> row)
        {
            row.ReleaseOne();
        }
    }
}
