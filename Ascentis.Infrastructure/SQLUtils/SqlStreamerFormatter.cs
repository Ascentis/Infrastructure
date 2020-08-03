using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class SqlStreamerFormatter : ISqlStreamerFormatter
    {
        protected int FieldCount { get; private set; }

        protected ColumnMetadata[] ColumnMetadatas { get; private set; }

        public virtual void Prepare(SqlDataReader reader, Stream stream)
        {
            FieldCount = reader.FieldCount;

            var schemaTable = reader.GetSchemaTable();
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
            }
        }

        public abstract void Process(object[] row, Stream stream);

        public virtual void UnPrepare(Stream stream) { }

        public virtual void AbortedWithException(Exception e) { }
    }
}
