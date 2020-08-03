using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlStreamerFormatterText : ISqlStreamerFormatter
    {
        protected string FormatString { get; set; }

        protected byte[] RowToBytes(object[] row)
        {
            var s = string.Format(FormatString, row);
            var buf = Encoding.UTF8.GetBytes(s);
            return buf;
        }

        public virtual void Prepare(SqlDataReader reader, Stream stream) { }

        public virtual void Process(object[] row, Stream stream)
        {
            var bytes = RowToBytes(row);
            stream.Write(bytes, 0, bytes.Length);
        }

        public virtual void UnPrepare(Stream stream) { }

        public virtual void AbortedWithException(Exception e) { }
    }
}
