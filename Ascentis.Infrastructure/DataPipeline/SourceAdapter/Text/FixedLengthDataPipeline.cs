using System.IO;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class FixedLengthDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(
            TextReader source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineSourceAdapterText.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DataPipelineSourceAdapterFixedLength(source)
            {
                ColumnMetadatas = sourceMetadatas, 
                RowsPoolSize = rowsPoolCapacity
            };
            base.Pump(sourceAdapter, dataPipelineTargetAdapter);
        }
    }
}
