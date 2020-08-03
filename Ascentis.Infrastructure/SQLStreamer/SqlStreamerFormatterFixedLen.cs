using System;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlStreamerFormatterFixedLen : SqlStreamerFormatterText
    {
        public enum OverflowStringFieldWidthBehavior { Error, Truncate }
        public int[] FieldSizes { get; set; }
        public OverflowStringFieldWidthBehavior[] OverflowStringFieldWidthBehaviors { get; set; }

        public override void Prepare(SqlDataReader reader, Stream stream)
        {
            ArgsChecker.CheckForNull<NullReferenceException>(FieldSizes, nameof(FieldSizes));
            FormatString = "";
            
            var fieldCount = reader.FieldCount;
            if (FieldSizes.Length != fieldCount)
                throw new SqlStreamerFormatterException("Provided FieldSizes array has a different length than result set column count");
            if (OverflowStringFieldWidthBehaviors != null && OverflowStringFieldWidthBehaviors.Length != fieldCount)
                throw new SqlStreamerFormatterException("When OverflowStringFieldWidthBehaviors is provided its length must match result set field count");
            for (var i = 0; i < fieldCount; i++)
                FormatString += $"{{{i},{FieldSizes[i]}}}";
            FormatString += "\r\n";
        }

        public override void Process(object[] row, Stream stream)
        {
            for (var i = 0; i < row.Length; i++)
            {
                if (!(row[i] is string) || ((string) row[i]).Length <= FieldSizes[i]) 
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
