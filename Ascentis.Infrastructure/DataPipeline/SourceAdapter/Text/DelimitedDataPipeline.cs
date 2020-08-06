using System.IO;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class DelimitedDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(
            TextReader source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            string delimiter = DataPipelineDelimitedSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineTextSourceAdapter.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DataPipelineDelimitedSourceAdapter(source)
            {
                ColumnMetadatas = sourceMetadatas,
                RowsPoolCapacity = rowsPoolCapacity,
                Delimiter = delimiter
            };
            base.Pump(sourceAdapter, dataPipelineTargetAdapter);
        }
    }
}
