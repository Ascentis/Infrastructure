using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipeline<TRow>
    {
        public delegate void RowErrorDelegate(IAdapter adapter, object data, Exception e);

        public event RowErrorDelegate OnSourceAdapterRowReadError;
        public event RowErrorDelegate OnTargetAdapterRowProcessError;
        public bool AbortOnSourceAdapterException { get; set; }
        public bool AbortOnTargetAdapterException { get; set; }
        protected ManualResetEvent FinishedEvent { get; }

        public DataPipeline()
        {
            FinishedEvent = new ManualResetEvent(false);
        }

        private void SetupAndHealthCheckAdapters(ISourceAdapter<TRow> sourceAdapter,
            IEnumerable<ITargetAdapter<TRow>> dataPipelineTargetAdapters,
            out int targetAdaptersCount)
        {
            if (string.IsNullOrEmpty(sourceAdapter.Id))
                sourceAdapter.Id = "src";
            sourceAdapter.OnSourceAdapterRowReadError += OnSourceAdapterRowReadError;
            sourceAdapter.AbortOnReadException = AbortOnSourceAdapterException;

            targetAdaptersCount = 0;
            var totalTargetBufferSize = 0;
            foreach (var dataPipelineTargetAdapter in dataPipelineTargetAdapters)
            {
                dataPipelineTargetAdapter.OnTargetAdapterRowProcessError += OnTargetAdapterRowProcessError;
                dataPipelineTargetAdapter.AbortOnProcessException ??= AbortOnTargetAdapterException;
                targetAdaptersCount++;
                totalTargetBufferSize += dataPipelineTargetAdapter.BufferSize;
            }

            sourceAdapter.ParallelLevel = targetAdaptersCount;
            if (sourceAdapter.RowsPoolSize > 0 && sourceAdapter.RowsPoolSize < totalTargetBufferSize)
                throw new DataPipelineException("Source adapter rows pool size can't be lesser than the sum of all target adapters buffer sizes. Deadlock would occur upon call to Pump()");
        }

        public virtual void Pump(ISourceAdapter<TRow> sourceAdapter, 
            ITargetAdapter<TRow> targetAdapter)
        {
            var targetAdapters = new [] { targetAdapter};
            Pump(sourceAdapter, targetAdapters);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public virtual void Pump(ISourceAdapter<TRow> sourceAdapter,
            IEnumerable<ITargetAdapter<TRow>> dataPipelineTargetAdapters)
        {
            using var setEventFinished = new Resettable<ManualResetEvent>(FinishedEvent, evt => evt.Set());
            SetupAndHealthCheckAdapters(sourceAdapter, dataPipelineTargetAdapters, out var targetAdaptersCount);

            sourceAdapter.Prepare();
            using var resettableSourceAdapter = new Resettable<ISourceAdapter<TRow>>(sourceAdapter, adapter => adapter.UnPrepare());
            foreach (var dataPipelineTargetAdapter in dataPipelineTargetAdapters)
                dataPipelineTargetAdapter.Prepare(sourceAdapter);

            var targetAdapterIndex = 0;
            var targetAdapterConveyors = new Conveyor<TRow>[targetAdaptersCount];
            foreach (var dataPipelineTargetAdapter in dataPipelineTargetAdapters)
            {
                targetAdapterConveyors[targetAdapterIndex++] = new Conveyor<TRow>((row, context) =>
                {
                    ((ITargetAdapter<TRow>) context).Process(row);
                    sourceAdapter.ReleaseRow(row);
                }, dataPipelineTargetAdapter);
            }

            try
            {
                foreach (var targetAdapterConveyor in targetAdapterConveyors)
                    targetAdapterConveyor.Start();

                var sourceRows = sourceAdapter.RowsEnumerable;
                foreach (var row in sourceRows)
                foreach (var targetAdapterConveyor in targetAdapterConveyors)
                    targetAdapterConveyor.InsertPacket(row);

                foreach (var targetAdapterConveyor in targetAdapterConveyors)
                    targetAdapterConveyor.StopAndWait();
                dataPipelineTargetAdapters.ForEach(adapter => adapter.UnPrepare());
            }
            catch (Exception e)
            {
                targetAdapterConveyors.ForEach(conveyor => conveyor.Stop(), typeof(InvalidOperationException),
                    false);
                dataPipelineTargetAdapters.ForEach(adapter => adapter.AbortedWithException(e));
                throw;
            }
        }
    }
}
