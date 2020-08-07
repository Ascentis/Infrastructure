using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipeline<TRow>
    {
        public delegate void RowErrorDelegate(object data, Exception e);

        public event RowErrorDelegate OnSourceAdapterRowReadError;
        public event RowErrorDelegate OnTargetAdapterRowProcessError;
        public bool AbortOnSourceAdapterException { get; set; }
        public bool AbortOnTargetAdapterException { get; set; }

        private void SetupAdapters(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter,
            IEnumerable<IDataPipelineTargetAdapter<TRow>> dataPipelineTargetAdapters,
            out int targetAdaptersCount)
        {
            dataPipelineSourceAdapter.OnSourceAdapterRowReadError += OnSourceAdapterRowReadError;
            dataPipelineSourceAdapter.AbortOnReadException = AbortOnSourceAdapterException;

            targetAdaptersCount = 0;
            foreach (var dataPipelineTargetAdapter in dataPipelineTargetAdapters)
            {
                dataPipelineTargetAdapter.OnTargetAdapterRowProcessError += OnTargetAdapterRowProcessError;
                dataPipelineTargetAdapter.AbortOnProcessException ??= AbortOnTargetAdapterException;
                targetAdaptersCount++;
            }

            dataPipelineSourceAdapter.ParallelLevel = targetAdaptersCount;
        }

        public void Pump(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter, 
            IDataPipelineTargetAdapter<TRow> dataPipelineTargetAdapter)
        {
            var targetAdapters = new [] { dataPipelineTargetAdapter};
            Pump(dataPipelineSourceAdapter, targetAdapters);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void Pump(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter,
            IEnumerable<IDataPipelineTargetAdapter<TRow>> dataPipelineTargetAdapters)
        {
            SetupAdapters(dataPipelineSourceAdapter, dataPipelineTargetAdapters, out var targetAdaptersCount);

            dataPipelineSourceAdapter.Prepare();
            try
            {
                foreach (var dataPipelineTargetAdapter in dataPipelineTargetAdapters)
                    dataPipelineTargetAdapter.Prepare(dataPipelineSourceAdapter);

                var targetAdapterIndex = 0;
                var targetAdapterConveyors = new Conveyor<TRow>[targetAdaptersCount];
                foreach (var dataPipelineTargetAdapter in dataPipelineTargetAdapters)
                {
                    targetAdapterConveyors[targetAdapterIndex++] = new Conveyor<TRow>((row, context) =>
                    {
                        ((IDataPipelineTargetAdapter<TRow>) context).Process(row);
                        dataPipelineSourceAdapter.ReleaseRow(row);
                    }, dataPipelineTargetAdapter);
                }

                try
                {
                    foreach (var targetAdapterConveyor in targetAdapterConveyors)
                        targetAdapterConveyor.Start();

                    var sourceRows = dataPipelineSourceAdapter.RowsEnumerable;
                    foreach (var row in sourceRows)
                        foreach(var targetAdapterConveyor in targetAdapterConveyors)
                            targetAdapterConveyor.InsertPacket(row);

                    foreach(var targetAdapterConveyor in targetAdapterConveyors)
                        targetAdapterConveyor.StopAndWait();
                    dataPipelineTargetAdapters.ForEach( adapter => adapter.UnPrepare());
                }
                catch (Exception e)
                {
                    targetAdapterConveyors.ForEach(conveyor => conveyor.Stop(), typeof(InvalidOperationException), false);
                    dataPipelineTargetAdapters.ForEach(adapter => adapter.AbortedWithException(e));
                    throw;
                }
            }
            finally
            {
                dataPipelineSourceAdapter.UnPrepare();
            }
        }
    }
}
