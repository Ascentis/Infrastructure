using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Manual
{
    public class SourceAdapterManual<T> : SourceAdapter<T>
    {
        private readonly ConcurrentQueue<T> _dataQueue;
        private readonly ManualResetEventSlim _dataAvailable;
        private volatile bool _finished;

        public SourceAdapterManual()
        {
            _dataAvailable = new ManualResetEventSlim(false);
            _dataQueue = new ConcurrentQueue<T>();
        }

        public override IEnumerable<T> RowsEnumerable
        {
            get
            {
                while (true)
                {
                    if(!_dataQueue.TryDequeue(out var obj))
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

        public void Insert(T obj)
        {
            _dataQueue.Enqueue(obj);
            _dataAvailable.Set();
        }

        public void Finish()
        {
            _finished = true;
            _dataAvailable.Set();
        }
    }
}
