using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipeline<TRow>
    {
        public delegate void RowErrorDelegate(object data, Exception e);

        public event RowErrorDelegate OnSourceAdapterRowReadError;
        public event RowErrorDelegate OnTargetAdapterRowProcessError;
        public bool AbortOnSourceAdapterException { get; set; }
        public bool AbortOnTargetAdapterException { get; set; }

        private void SetupAdaptersEvents(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter,
            IDataPipelineTargetAdapter<TRow> dataPipelineTargetAdapter)
        {
            dataPipelineSourceAdapter.OnSourceAdapterRowReadError += OnSourceAdapterRowReadError;
            dataPipelineSourceAdapter.AbortOnReadException = AbortOnSourceAdapterException;

            dataPipelineTargetAdapter.OnTargetAdapterRowProcessError += OnTargetAdapterRowProcessError;
            dataPipelineTargetAdapter.AbortOnProcessException = AbortOnTargetAdapterException;
        }

        public void Pump(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter, 
            IDataPipelineTargetAdapter<TRow> dataPipelineTargetAdapter)
        {
            SetupAdaptersEvents(dataPipelineSourceAdapter, dataPipelineTargetAdapter);

            dataPipelineSourceAdapter.Prepare();
            try
            {
                dataPipelineTargetAdapter.Prepare(dataPipelineSourceAdapter);
                var targetAdapterConveyor = new Conveyor<TRow>(row =>
                {
                    dataPipelineTargetAdapter.Process(row);
                    dataPipelineSourceAdapter.ReleaseRow(row);
                });
                try
                {
                    targetAdapterConveyor.Start();

                    var sourceRows = dataPipelineSourceAdapter.RowsEnumerable;
                    foreach (var row in sourceRows)
                        targetAdapterConveyor.InsertPacket(row);

                    targetAdapterConveyor.StopAndWait();
                    dataPipelineTargetAdapter.UnPrepare();
                }
                catch (Exception e)
                {
                    try
                    {
                        targetAdapterConveyor.Stop();
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    dataPipelineTargetAdapter.AbortedWithException(e);
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
