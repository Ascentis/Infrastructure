using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlStreamerFormatterText : ISqlStreamerFormatter
    {
        protected int FieldCount { get; private set; }
        protected string FormatString { get; set; }
        public string[] ColumnFormatStrings { get; set; }

        protected byte[] RowToBytes(object[] row)
        {
            var s = string.Format(FormatString, row);
            var buf = Encoding.UTF8.GetBytes(s);
            return buf;
        }

        protected string ColumnFormatString(int index)
        {
            return ColumnFormatStrings != null && ColumnFormatStrings[index] != "" ? ":" + ColumnFormatStrings[index] : "";
        }

        public virtual void Prepare(SqlDataReader reader, Stream stream)
        {
            FieldCount = reader.FieldCount;
            if (ColumnFormatStrings != null && ColumnFormatStrings.Length != FieldCount)
                throw new SqlStreamerFormatterException("When ColumnFormatStrings is provided its length must match result set field count");
        }

        public virtual void Process(object[] row, Stream stream)
        {
            var bytes = RowToBytes(row);
            stream.Write(bytes, 0, bytes.Length);
        }

        public virtual void UnPrepare(Stream stream) { }

        public virtual void AbortedWithException(Exception e) { }
    }
}
