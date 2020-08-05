using System.IO;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text
{
    public class DataPipelineTargetAdapterDelimited : DataPipelineTargetAdapterText
    {
        public bool OutputHeaders { get; set; }
        public string Delimiter { get; set; } = ",";

        public override void Prepare(IDataPipelineSourceAdapter<object[]> source, Stream target)
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
            target.Write(bytes, 0, bytesWritten);
        }
    }
}
