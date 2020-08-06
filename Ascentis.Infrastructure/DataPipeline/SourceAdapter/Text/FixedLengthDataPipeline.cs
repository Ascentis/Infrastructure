using System.IO;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class FixedLengthDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(
            TextReader source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineTextSourceAdapter.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DataPipelineFixedLengthSourceAdapter(source)
            {
                ColumnMetadatas = sourceMetadatas, 
                RowsPoolCapacity = rowsPoolCapacity
            };
            base.Pump(sourceAdapter, dataPipelineTargetAdapter);
        }
    }
}
