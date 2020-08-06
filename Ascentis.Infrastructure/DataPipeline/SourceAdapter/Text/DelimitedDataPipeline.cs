using System.Collections.Generic;
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

        public void Pump(
            Stream source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            string delimiter = DataPipelineDelimitedSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineTextSourceAdapter.DefaultRowsPoolCapacity)
        {
            using var reader = new StreamReader(source);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapter, delimiter, rowsPoolCapacity);
        }

        public void Pump(
            TextReader source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IEnumerable<IDataPipelineTargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            string delimiter = DataPipelineDelimitedSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineTextSourceAdapter.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DataPipelineDelimitedSourceAdapter(source)
            {
                ColumnMetadatas = sourceMetadatas,
                RowsPoolCapacity = rowsPoolCapacity,
                Delimiter = delimiter
            };
            base.Pump(sourceAdapter, dataPipelineTargetAdapters);
        }

        public void Pump(
            Stream source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IEnumerable<IDataPipelineTargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            string delimiter = DataPipelineDelimitedSourceAdapter.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineTextSourceAdapter.DefaultRowsPoolCapacity)
        {
            using var reader = new StreamReader(source);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapters, delimiter, rowsPoolCapacity);
        }
    }
}
