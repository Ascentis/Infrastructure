using System.Globalization;
using System.IO;
using System.Text;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text
{
    public class DataPipelineTargetAdapterText : DataPipelineTargetAdapter<object[]>
    {
        protected Stream Target { get; }
        protected string FormatString { get; set; }
        protected byte[] WriteBuffer { get; set; }

        public string[] ColumnFormatStrings { get; set; }
        public Encoding OutputEncoding { get; set; } = Encoding.UTF8;
        public CultureInfo FormatCultureInfo { get; set; } = CultureInfo.InvariantCulture;

        public DataPipelineTargetAdapterText(Stream target)
        {
            Target = target;
        }

        protected virtual byte[] RowToBytes(object[] row, out int bytesWritten)
        {
            var buf = WriteBuffer;
            var s = string.Format(FormatCultureInfo, FormatString, row);
            bytesWritten = s.Length;
            if (buf == null || bytesWritten > buf.Length)
                buf = new byte[(int)(bytesWritten * 1.25)];
            OutputEncoding.GetBytes(s, 0, s.Length, buf, 0);
            return buf;
        }

        protected static int ColumnTypeToBufferSize(DataPipelineColumnMetadata meta)
        {
            if (meta.DataType == typeof(string) && meta.ColumnSize != null)
                return (int)meta.ColumnSize;
            return DataPipelineTypeSizeMap.Map.TryGetValue(meta.DataType, out var result) ? result : 16;
        }

        protected string ColumnFormatString(int index)
        {
            return ColumnFormatStrings != null && ColumnFormatStrings[index] != "" ? ":" + ColumnFormatStrings[index] : "";
        }

        public override void Prepare(IDataPipelineSourceAdapter<object[]> source)
        {
            base.Prepare(source);

            if (ColumnFormatStrings != null && ColumnFormatStrings.Length != Source.FieldCount)
                throw new DataPipelineException("When ColumnFormatStrings is provided its length must match result set field count");
        }

        public override void Process(object[] row)
        {
            var bytes = RowToBytes(row, out var bytesWritten);
            Target.Write(bytes, 0, bytesWritten);
        }
    }
}
