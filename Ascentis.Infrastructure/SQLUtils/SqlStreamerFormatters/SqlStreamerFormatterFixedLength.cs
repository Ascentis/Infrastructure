using System;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlStreamerFormatterFixedLength : SqlStreamerFormatterText
    {
        public enum OverflowStringFieldWidthBehavior { Error, Truncate }
        public int[] FieldSizes { get; set; }
        public OverflowStringFieldWidthBehavior[] OverflowStringFieldWidthBehaviors { get; set; }

        private int _rowSize;

        public override void Prepare(SqlDataReader reader, Stream stream)
        {
            const string crLf = "\r\n";
            base.Prepare(reader, stream);

            ArgsChecker.CheckForNull<NullReferenceException>(FieldSizes, nameof(FieldSizes));
            if (FieldSizes.Length != FieldCount)
                throw new SqlStreamerFormatterException("Provided FieldSizes array has a different length than result set column count");
            if (OverflowStringFieldWidthBehaviors != null && OverflowStringFieldWidthBehaviors.Length != FieldCount)
                throw new SqlStreamerFormatterException("When OverflowStringFieldWidthBehaviors is provided its length must match result set field count");

            _rowSize = 0;
            FormatString = "";
            for (var i = 0; i < FieldCount; i++)
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
                throw new SqlStreamerFormatterException("Total row size exceeds specified row size based on fixed column widths");
            return buf;
        }

        public override void Process(object[] row, Stream stream)
        {
            if (OverflowStringFieldWidthBehaviors != null)
                for (var i = 0; i < row.Length; i++)
                {
                    if (!(row[i] is string) || ((string) row[i]).Length <= Math.Abs(FieldSizes[i]))
                        continue;
                    var strValue = (string) row[i];
                    if (OverflowStringFieldWidthBehaviors[i] == OverflowStringFieldWidthBehavior.Error)
                        throw new SqlStreamerFormatterException(
                            $"Field {ColumnMetadatas[i].ColumnName} size overflow streaming using fixed length streamer");
                    row[i] = strValue.Remove(FieldSizes[i], strValue.Length - FieldSizes[i]);
                }

            base.Process(row, stream);
        }
    }
}
