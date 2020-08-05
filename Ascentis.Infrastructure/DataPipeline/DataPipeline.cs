using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipeline<TTarget, TRow>
    {
        public void Pump(IDataPipelineSourceAdapter<TRow> dataPipelineSourceAdapter, 
            IDataPipelineTargetAdapter<TTarget, TRow> dataPipelineTargetAdapter, 
            TTarget target)
        {
            dataPipelineTargetAdapter.Prepare(dataPipelineSourceAdapter, target);
            try
            {
                var targetFormatterConveyor = new Conveyor<TRow>(row =>
                {
                    dataPipelineTargetAdapter.Process(row);
                    dataPipelineSourceAdapter.ReleaseRow(row);
                });
                targetFormatterConveyor.Start();

                var sourceRows = dataPipelineSourceAdapter.RowsEnumerable;
                foreach(var row in sourceRows)
                    targetFormatterConveyor.InsertPacket(row);

                targetFormatterConveyor.StopAndWait();
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
