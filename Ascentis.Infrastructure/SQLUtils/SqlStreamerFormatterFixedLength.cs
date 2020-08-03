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

        public override void Prepare(SqlDataReader reader, Stream stream)
        {
            const string crLf = "\r\n";
            base.Prepare(reader, stream);

            ArgsChecker.CheckForNull<NullReferenceException>(FieldSizes, nameof(FieldSizes));
            if (FieldSizes.Length != FieldCount)
                throw new SqlStreamerFormatterException("Provided FieldSizes array has a different length than result set column count");
            if (OverflowStringFieldWidthBehaviors != null && OverflowStringFieldWidthBehaviors.Length != FieldCount)
                throw new SqlStreamerFormatterException("When OverflowStringFieldWidthBehaviors is provided its length must match result set field count");

            var bufferSize = 0;
            FormatString = "";
            for (var i = 0; i < FieldCount; i++)
            {
                bufferSize += Math.Abs(FieldSizes[i]);
                FormatString += $"{{{i},{FieldSizes[i]}{ColumnFormatString(i)}}}";
            }
            WriteBuffer = new byte[bufferSize + crLf.Length];
            FormatString += crLf;
        }

        public override void Process(object[] row, Stream stream)
        {
            for (var i = 0; i < row.Length; i++)
            {
                if (!(row[i] is string) || ((string) row[i]).Length <= Math.Abs(FieldSizes[i])) 
                    continue;
                var strValue = (string)row[i];
                if (OverflowStringFieldWidthBehaviors == null || OverflowStringFieldWidthBehaviors[i] == OverflowStringFieldWidthBehavior.Error)
                    throw new SqlStreamerFormatterException($"Field number {i} size overflow streaming using fixed length streamer");
                row[i] = strValue.Remove(FieldSizes[i], strValue.Length - FieldSizes[i]);
            }

            base.Process(row, stream);
        }
    }
}
