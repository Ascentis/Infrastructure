using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlStreamerFormatterCsv : SqlStreamerFormatterText
    {
        public bool OutputHeaders { get; set; }

        public override void Prepare(SqlDataReader reader, Stream stream)
        {
            FormatString = "";
            var fieldCount = reader.FieldCount;
            for (var i = 0; i < fieldCount; i++)
                FormatString += $"{{{i}}},";
            FormatString = FormatString.Remove(FormatString.Length - 1) + "\r\n";
            if (!OutputHeaders)
                return;
            var columnNames = new object[fieldCount];
            for (var i = 0; i < fieldCount; i++)
                columnNames[i] = reader.GetName(i);
            var bytes = RowToBytes(columnNames);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
