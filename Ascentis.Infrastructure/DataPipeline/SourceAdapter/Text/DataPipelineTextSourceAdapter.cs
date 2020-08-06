using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text
{
    public abstract class DataPipelineTextSourceAdapter : IDataPipelineSourceAdapter<object[]>
    {
        public const int DefaultRowsPoolCapacity = 1000;

        public int RowsPoolCapacity { get; set; } = DefaultRowsPoolCapacity;

        protected delegate object TextToObject(string text);

        protected TextReader Reader { get; }

        protected Pool<object[]> RowsPool { get; private set; }

        protected DataPipelineTextSourceAdapter(TextReader textReader)
        {
            Reader = textReader;
        }

        protected TextToObject[] BuildConversionArray()
        {
            var result = new TextToObject[ColumnMetadatas.Length];
            for (var i = 0; i < ColumnMetadatas.Length; i++)
            {
                if (ColumnMetadatas[i].DataType == typeof(string))
                    result[i] = text => text.Trim();
                if (ColumnMetadatas[i].DataType == typeof(char))
                    result[i] = text => text.Trim() != "" ? text.Trim()[0] : ' ';
                else if (ColumnMetadatas[i].DataType == typeof(int))
                    result[i] = text => int.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(short))
                    result[i] = text => short.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(byte))
                    result[i] = text => byte.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(long))
                    result[i] = text => long.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(bool))
                    result[i] = text => bool.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(double))
                    result[i] = text => double.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(float))
                    result[i] = text => float.Parse(text.Trim());
                else if (ColumnMetadatas[i].DataType == typeof(DateTime))
                    result[i] = text => DateTime.Parse(text.Trim());
            }

            return result;
        }

        public virtual void ReleaseRow(object[] row)
        {
            RowsPool.Release(row);
        }

        public virtual void Prepare()
        {
            RowsPool = new Pool<object[]>(RowsPoolCapacity, () => new object[FieldCount]);
        }

        public virtual void UnPrepare() { }

        public abstract IEnumerable<object[]> RowsEnumerable { get; }

        public virtual int FieldCount {
            get
            {
                ArgsChecker.CheckForNull<NullReferenceException>(ColumnMetadatas, nameof(ColumnMetadatas));
                return ColumnMetadatas.Length;
            }
        }

        public DataPipelineColumnMetadata[] ColumnMetadatas { get; set; }
    }
}
