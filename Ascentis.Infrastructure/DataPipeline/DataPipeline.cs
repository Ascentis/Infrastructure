using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipeline<TRow>
    {
        public void Pump(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter, IDataPipelineTargetAdapter<TRow> dataPipelineTargetAdapter)
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

                dataPipelineSourceAdapter.Prepare();
                try
                {
                    var sourceRows = dataPipelineSourceAdapter.RowsEnumerable;
                    foreach (var row in sourceRows)
                        targetAdapterConveyor.InsertPacket(row);
                }
                finally
                {
                    dataPipelineSourceAdapter.UnPrepare();
                }

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
                    // We will catch and ignore invalid attempt to Stop a not yet running Conveyor
                }

                dataPipelineTargetAdapter.AbortedWithException(e);
                throw;
            }
        }
    }
}
