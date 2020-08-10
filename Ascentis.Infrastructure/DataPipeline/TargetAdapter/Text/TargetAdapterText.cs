using System;
using System.Globalization;
using System.IO;
using System.Text;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Generic;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text
{
    public class TargetAdapterText : TargetAdapter<PoolEntry<object[]>>
    {
        protected Stream Target { get; }
        protected string FormatString { get; set; }
        protected byte[] WriteBuffer { get; set; }

        public string[] ColumnFormatStrings { get; set; }
        public Encoding OutputEncoding { get; set; } = Encoding.UTF8;
        public CultureInfo FormatCultureInfo { get; set; } = CultureInfo.InvariantCulture;

        public TargetAdapterText(Stream target)
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

        protected static int ColumnTypeToBufferSize(ColumnMetadata meta)
        {
            if (meta.DataType == typeof(string) && meta.ColumnSize != null)
                return (int)meta.ColumnSize;
            return TypeSizeMap.Map.TryGetValue(meta.DataType, out var result) ? result : 16;
        }

        protected string ColumnFormatString(int index)
        {
            return ColumnFormatStrings != null && ColumnFormatStrings[index] != "" ? ":" + ColumnFormatStrings[index] : "";
        }

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            if (ColumnFormatStrings != null && ColumnFormatStrings.Length != Source.FieldCount)
                throw new DataPipelineException("When ColumnFormatStrings is provided its length must match result set field count");
        }

        public override void Process(PoolEntry<object[]> row)
        {

            if (InvokeBeforeTargetAdapterProcessRowEvent(row) == BeforeProcessRowResult.Abort)
                return;

            byte[] bytes;
            int bytesWritten;
            try
            {

                bytes = RowToBytes(row.Value, out bytesWritten);
            }
            catch (Exception e)
            {
                if (AbortOnProcessException ?? false)
                    throw;
                InvokeProcessErrorEvent(row, e);
                return;
            }
            Target.Write(bytes, 0, bytesWritten);

            InvokeAfterTargetAdapterProcessRowEvent(row);
        }
    }
}
