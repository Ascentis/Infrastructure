using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class StreamerTargetFormatterDelimited : StreamerTargetFormatterText
    {
        public bool OutputHeaders { get; set; }
        public string Delimiter { get; set; } = ",";

        private static int ColumnTypeToBufferSize(ColumnMetadata meta)
        {
            if (meta.DataType == typeof(string))
                return meta.ColumnSize ?? 16;
            if (meta.DataType == typeof(char))
                return 1;
            if (meta.DataType == typeof(bool))
                return 5;
            if (meta.DataType == typeof(byte))
                return 3;
            if (meta.DataType == typeof(int) || meta.DataType == typeof(uint))
                return 14;
            if (meta.DataType == typeof(short) || meta.DataType == typeof(ushort))
                return 6; 
            return 16;
        }

        public override void Prepare(IStreamerSourceAdapter source, object target)
        {
            const string crLf = "\r\n";
            base.Prepare(source, target);

            FormatString = "";
            var bufferSize = 0;
            for (var i = 0; i < Source.FieldCount; i++)
            {
                FormatString += $"{{{i}{ColumnFormatString(i)}}}{Delimiter}";
                bufferSize += ColumnTypeToBufferSize(Source.ColumnMetadatas[i]) + Delimiter.Length;
            }
            FormatString = FormatString.Remove(FormatString.Length - 1) + "\r\n";
            WriteBuffer = new byte[bufferSize + crLf.Length];

            if (!OutputHeaders)
                return;
            var columnNames = new object[Source.FieldCount];
            for (var i = 0; i < Source.FieldCount; i++)
                columnNames[i] = Source.ColumnMetadatas[i].ColumnName;
            var bytes = RowToBytes(columnNames, out var bytesWritten);
            ((Stream) target).Write(bytes, 0, bytesWritten);
        }
    }
}
