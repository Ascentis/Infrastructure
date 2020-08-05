using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class StreamerFormatter : IStreamerFormatter
    {
        protected int FieldCount { get; private set; }

        protected ColumnMetadata[] ColumnMetadatas { get; private set; }

        public virtual void Prepare(IStreamerAdapter source, object target)
        {
            var stream = (Stream) target;
            FieldCount = source.FieldCount;

            /*var schemaTable = reader.GetSchemaTable();
            ColumnMetadatas = new ColumnMetadata[FieldCount];

            var columnIndex = 0;
            // ReSharper disable once PossibleNullReferenceException
            foreach (DataRow field in schemaTable.Rows)
            {
                ColumnMetadatas[columnIndex] = new ColumnMetadata();
                foreach (DataColumn column in schemaTable.Columns)
                {
                    var prop = ColumnMetadatas[columnIndex].GetType().GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                        continue;
                    var value = field[column];
                    prop.SetValue(ColumnMetadatas[columnIndex], !(value is DBNull) ? value : null);
                }

                columnIndex++;
            }*/
        }

        public abstract void Process(object[] row, object target);

        public virtual void UnPrepare(object target) { }

        public virtual void AbortedWithException(Exception e) { }
    }
}
