using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipeline<TRow>
    {
        public void Pump(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter, IDataPipelineTargetAdapter<TRow> dataPipelineTargetAdapter)
        {
            dataPipelineTargetAdapter.Prepare(dataPipelineSourceAdapter);
            try
            {
                var targetAdapterConveyor = new Conveyor<TRow>(row =>
                {
                    dataPipelineTargetAdapter.Process(row);
                    dataPipelineSourceAdapter.ReleaseRow(row);
                });
                targetAdapterConveyor.Start();

                var sourceRows = dataPipelineSourceAdapter.RowsEnumerable;
                foreach(var row in sourceRows)
                    targetAdapterConveyor.InsertPacket(row);

                targetAdapterConveyor.StopAndWait();
                dataPipelineTargetAdapter.UnPrepare();
            }
            catch (Exception e)
            {
                dataPipelineTargetAdapter.AbortedWithException(e);
                throw;
            }
        }
    }
}
