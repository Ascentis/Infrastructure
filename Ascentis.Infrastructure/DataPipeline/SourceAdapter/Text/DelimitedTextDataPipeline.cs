using System.Collections.Generic;
using System.IO;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public class DelimitedTextDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(
            TextReader source,
            ColumnMetadata[] sourceMetadatas,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            string delimiter = DelimitedTextSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = TextSourceAdapter.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DelimitedTextSourceAdapter(source)
            {
                ColumnMetadatas = sourceMetadatas,
                RowsPoolSize = rowsPoolCapacity,
                Delimiter = delimiter
            };
            base.Pump(sourceAdapter, targetAdapter);
        }

        public void Pump(
            Stream source,
            ColumnMetadata[] sourceMetadatas,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            string delimiter = DelimitedTextSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = TextSourceAdapter.DefaultRowsPoolCapacity)
        {
            using var reader = new StreamReader(source);
            Pump(reader, sourceMetadatas, targetAdapter, delimiter, rowsPoolCapacity);
        }

        public void Pump(
            string sourceFileName,
            ColumnMetadata[] sourceMetadatas,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            string delimiter = DelimitedTextSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = TextSourceAdapter.DefaultRowsPoolCapacity)
        {
            using var stream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            Pump(reader, sourceMetadatas, targetAdapter, delimiter, rowsPoolCapacity);
        }

        public void Pump(
            TextReader source,
            ColumnMetadata[] sourceMetadatas,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            string delimiter = DelimitedTextSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = TextSourceAdapter.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DelimitedTextSourceAdapter(source)
            {
                ColumnMetadatas = sourceMetadatas,
                RowsPoolSize = rowsPoolCapacity,
                Delimiter = delimiter
            };
            base.Pump(sourceAdapter, dataPipelineTargetAdapters);
        }

        public void Pump(
            Stream source,
            ColumnMetadata[] sourceMetadatas,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            string delimiter = DelimitedTextSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = TextSourceAdapter.DefaultRowsPoolCapacity)
        {
            using var reader = new StreamReader(source);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapters, delimiter, rowsPoolCapacity);
        }

        public void Pump(
            string sourceFileName,
            ColumnMetadata[] sourceMetadatas,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            string delimiter = DelimitedTextSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = TextSourceAdapter.DefaultRowsPoolCapacity)
        {
            using var stream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapters, delimiter, rowsPoolCapacity);
        }
    }
}
