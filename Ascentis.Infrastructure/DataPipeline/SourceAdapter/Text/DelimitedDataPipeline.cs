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
            string delimiter = DataPipelineSourceAdapterDelimited.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineSourceAdapterText.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DataPipelineSourceAdapterDelimited(source)
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
            string delimiter = DataPipelineSourceAdapterDelimited.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineSourceAdapterText.DefaultRowsPoolCapacity)
        {
            using var reader = new StreamReader(source);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapter, delimiter, rowsPoolCapacity);
        }

        public void Pump(
            string sourceFileName,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            string delimiter = DataPipelineSourceAdapterDelimited.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineSourceAdapterText.DefaultRowsPoolCapacity)
        {
            using var stream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapter, delimiter, rowsPoolCapacity);
        }

        public void Pump(
            TextReader source,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IEnumerable<IDataPipelineTargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            string delimiter = DataPipelineSourceAdapterDelimited.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineSourceAdapterText.DefaultRowsPoolCapacity)
        {
            var sourceAdapter = new DataPipelineSourceAdapterDelimited(source)
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
            string delimiter = DataPipelineSourceAdapterDelimited.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineSourceAdapterText.DefaultRowsPoolCapacity)
        {
            using var reader = new StreamReader(source);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapters, delimiter, rowsPoolCapacity);
        }

        public void Pump(
            string sourceFileName,
            DataPipelineColumnMetadata[] sourceMetadatas,
            IEnumerable<IDataPipelineTargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            string delimiter = DataPipelineSourceAdapterDelimited.DefaultDelimiter,
            int rowsPoolCapacity = DataPipelineSourceAdapterText.DefaultRowsPoolCapacity)
        {
            using var stream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            Pump(reader, sourceMetadatas, dataPipelineTargetAdapters, delimiter, rowsPoolCapacity);
        }
    }
}
