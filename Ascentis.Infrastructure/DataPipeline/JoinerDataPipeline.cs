using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.PassThru;

namespace Ascentis.Infrastructure.DataPipeline
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public class JoinerDataPipeline<TRow>
    {
        public delegate void RowsJoinedDelegate(IList<TRow> rowsEnumerable);

        private ConcurrentQueue<TRow>[] _rowsQueues;
        private ManualResetEventSlim _itemAvailableEvent;
        private volatile bool _finishThread;
        private volatile Exception _threadException;
        private int[] _rowsProcessedCount;
        private bool[] _finished;

        private void ProcessRow(IAdapter adapter, TRow row)
        {
            if (_threadException != null)
                throw _threadException;

            if (row is IPoolEntry entry)
                entry.Retain();
            _rowsQueues[adapter.Id].Enqueue(row);
            _itemAvailableEvent.Set();
        }

        public void Pump(IList<ISourceAdapter<TRow>> sourceAdapters, RowsJoinedDelegate onRowsJoined)
        {
            try
            {
                _finishThread = false;
                _itemAvailableEvent = new ManualResetEventSlim(false);
                var targetAdapters = new List<PassThruTargetAdapter<TRow>>();
                foreach (var dummy in sourceAdapters)
                {
                    targetAdapters.Add(new PassThruTargetAdapter<TRow>(ProcessRow) {AbortOnProcessException = true});
                }

                _rowsQueues = new ConcurrentQueue<TRow>[targetAdapters.Count];
                _rowsProcessedCount = new int[targetAdapters.Count];
                _finished = new bool[targetAdapters.Count];

                for (var i = 0; i < _rowsQueues.Length; i++)
                    _rowsQueues[i] = new ConcurrentQueue<TRow>();

                var processingThread = new Thread(ProcessRows);
                processingThread.Start(onRowsJoined);
                
                var boundedParallel = new BoundedParallel(1, targetAdapters.Count);
                boundedParallel.For((long)0, sourceAdapters.Count, index =>
                {
                    var pipeline = new DataPipeline<TRow>();
                    targetAdapters[(int)index].Id = (int)index;
                    pipeline.AfterTargetAdapterProcessRow += (a, row) =>
                    {
                        _rowsProcessedCount[index]++;
                        for (var idx = 0; idx < _finished.Length; idx++)
                            if (_finished[idx] && _rowsProcessedCount[(int)index] > _rowsProcessedCount[idx])
                                throw new DataPipelineAbortedException();
                    };
                    pipeline.Pump(sourceAdapters[(int)index], targetAdapters[(int)index]);
                    _finished[(int)index] = true;
                });

                _finishThread = true;
                _itemAvailableEvent.Set();
                processingThread.Join();
                if (_threadException != null)
                    throw _threadException;
            } 
            finally
            {
                _threadException = null;
                _rowsQueues = null;
                _itemAvailableEvent = null;
            }
        }

        private void ProcessRows(object o)
        {
            try
            {
                var rowsJoinedDelegate = (RowsJoinedDelegate) o;
                TRow[] tipItems = null;
                var dequeueCount = 0;
                while (true)
                {
                    var queuesWithData = false;
                    foreach (var t in _rowsQueues)
                    {
                        if (t.IsEmpty) 
                            continue;
                        queuesWithData = true;
                        break;
                    }

                    if (!queuesWithData)
                    {
                        _itemAvailableEvent.Wait();
                        _itemAvailableEvent.Reset();
                    }

                    tipItems ??= new TRow[_rowsQueues.Length];
                    for (var i = 0; i < _rowsQueues.Length; i++)
                    {
                        if (tipItems[i] != null)
                            continue;
                        if (!_rowsQueues[i].TryDequeue(out var row))
                            continue;
                        tipItems[i] = row;
                        dequeueCount++;
                    }

                    if (dequeueCount < _rowsQueues.Length)
                    {
                        if (_finishThread)
                            break;
                        continue;
                    }

                    dequeueCount = 0;
                    rowsJoinedDelegate?.Invoke(tipItems);
                    for (var i = 0; i < tipItems.Length; i++)
                    {
                        if (tipItems[i] is IPoolEntry entry)
                            entry.Release();
                        tipItems[i] = default;
                    }
                }
            }
            catch (Exception e)
            {
                _threadException = e;
            }
        }
    }
}
