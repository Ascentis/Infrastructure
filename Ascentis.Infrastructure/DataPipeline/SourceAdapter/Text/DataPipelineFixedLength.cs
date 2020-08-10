using System.IO;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class DataPipelineFixedLength : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(
            TextReader source,
            ColumnMetadata[] sourceMetadatas,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterText.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new SourceAdapterFixedLength(source)
            {
                ColumnMetadatas = sourceMetadatas, 
                RowsPoolSize = rowsPoolCapacity
            };
            base.Pump(sourceAdapter, targetAdapter);
        }
    }
}
