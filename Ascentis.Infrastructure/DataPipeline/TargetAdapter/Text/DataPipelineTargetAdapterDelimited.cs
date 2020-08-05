using System;
using System.Collections.Generic;
using System.IO;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text
{
    public class DataPipelineTargetAdapterDelimited : DataPipelineTargetAdapterText
    {
        public bool OutputHeaders { get; set; }
        public string Delimiter { get; set; } = ",";

        private static readonly Dictionary<Type, int> TypeToBufferSize;

        static DataPipelineTargetAdapterDelimited()
        {
            TypeToBufferSize = new Dictionary<Type, int>
            {
                {typeof(char), 1},
                {typeof(byte), 3},
                {typeof(bool), 5},
                {typeof(ushort), 5},
                {typeof(short), 6},
                {typeof(uint), 13},
                {typeof(int), 14},
                {typeof(string), 16}
            };
        }

        private static int ColumnTypeToBufferSize(DataStreamerColumnMetadata meta)
        {
            if (meta.DataType == typeof(string) && meta.ColumnSize != null)
                return (int) meta.ColumnSize;
            return TypeToBufferSize.TryGetValue(meta.DataType, out var result) ? result : 16;
        }

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
