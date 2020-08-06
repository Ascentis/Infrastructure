using System;
using System.IO;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text
{
    public class DataPipelineTargetAdapterFixedLength : DataPipelineTargetAdapterText
    {
        public enum OverflowStringFieldWidthBehavior { Error, Truncate }
        public int[] FieldSizes { get; set; }
        public OverflowStringFieldWidthBehavior[] OverflowStringFieldWidthBehaviors { get; set; }

        private int _rowSize;

        public  DataPipelineTargetAdapterFixedLength(Stream stream) : base(stream) {}

        private void InitializeFieldSizesWithDefaults()
        {
            FieldSizes = new int[Source.FieldCount];
            var columnIndex = 0;
            foreach (var columnMetadata in Source.ColumnMetadatas)
                FieldSizes[columnIndex++] = ColumnTypeToBufferSize(columnMetadata);
        }

        public override void Prepare(IDataPipelineSourceAdapter<object[]> source)
        {
            const string crLf = "\r\n";
            base.Prepare(source);

            if (FieldSizes == null || FieldSizes.Length <= 0)
                InitializeFieldSizesWithDefaults();
            if (FieldSizes.Length != Source.FieldCount)
                throw new DataPipelineException("Provided FieldSizes array has a different length than result set column count");
            if (OverflowStringFieldWidthBehaviors != null && OverflowStringFieldWidthBehaviors.Length != Source.FieldCount)
                throw new DataPipelineException("When OverflowStringFieldWidthBehaviors is provided its length must match result set field count");

            _rowSize = 0;
            FormatString = "";
            for (var i = 0; i < Source.FieldCount; i++)
            {
                _rowSize += Math.Abs(FieldSizes[i]);
                FormatString += $"{{{i},{FieldSizes[i]}{ColumnFormatString(i)}}}";
            }

            _rowSize += crLf.Length;
            WriteBuffer = new byte[_rowSize];
            FormatString += crLf;
        }

        protected override byte[] RowToBytes(object[] row, out int bytesWritten)
        {
            var buf = base.RowToBytes(row, out bytesWritten);
            if (bytesWritten > _rowSize)
                throw new DataPipelineException("Total row size exceeds specified row size based on fixed column widths");
            return buf;
        }

        public override void Process(object[] row)
        {
            if (OverflowStringFieldWidthBehaviors != null)
                for (var i = 0; i < row.Length; i++)
                {
                    if (!(row[i] is string) || ((string) row[i]).Length <= Math.Abs(FieldSizes[i]))
                        continue;
                    var strValue = (string) row[i];
                    if (OverflowStringFieldWidthBehaviors[i] == OverflowStringFieldWidthBehavior.Error)
                        throw new DataPipelineException(
                            $"Field {Source.ColumnMetadatas[i].ColumnName} size overflow streaming using fixed length streamer");
                    row[i] = strValue.Remove(FieldSizes[i], strValue.Length - FieldSizes[i]);
                }

            base.Process(row);
        }
    }
}
