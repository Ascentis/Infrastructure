using System.IO;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class FixedLengthDataPipeline : DataPipeline<object[]>
    {
        public void Pump(
            TextReader source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IDataPipelineTargetAdapter<object[]> dataPipelineTargetAdapter,
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
