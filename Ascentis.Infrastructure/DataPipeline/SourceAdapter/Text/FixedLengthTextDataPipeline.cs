using System.IO;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class FixedLengthTextDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(
            TextReader source,
            ColumnMetadata[] sourceMetadatas,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = TextSourceAdapter.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new FixedLengthTextSourceAdapter(source)
            {
                ColumnMetadatas = sourceMetadatas, 
                RowsPoolSize = rowsPoolCapacity
            };
            base.Pump(sourceAdapter, targetAdapter);
        }
    }
}
