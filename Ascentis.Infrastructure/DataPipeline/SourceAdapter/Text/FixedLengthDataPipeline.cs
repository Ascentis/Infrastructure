using System.IO;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class FixedLengthDataPipeline<T> : DataPipeline<object[]>
    {
        public void Pump(
            TextReader source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IDataPipelineTargetAdapter<object[]> dataPipelineTargetAdapter,
            int rowsPoolCapacity = 1000)
        {
            var sourceAdapter = new DataPipelineFixedLengthSourceAdapter(source) {ColumnMetadatas = sourceMetadatas};
            base.Pump(sourceAdapter, dataPipelineTargetAdapter);
        }
    }
}
