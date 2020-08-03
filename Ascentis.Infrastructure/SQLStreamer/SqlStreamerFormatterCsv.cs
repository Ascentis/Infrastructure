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
            base.Prepare(reader, stream);

            FormatString = "";
            for (var i = 0; i < FieldCount; i++)
                FormatString += $"{{{i}{ColumnFormatString(i)}}},";
            FormatString = FormatString.Remove(FormatString.Length - 1) + "\r\n";
            if (!OutputHeaders)
                return;
            var columnNames = new object[FieldCount];
            for (var i = 0; i < FieldCount; i++)
                columnNames[i] = reader.GetName(i);
            var bytes = RowToBytes(columnNames);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
