using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils
{
    public class ColumnMetadataList : List<ColumnMetadata>
    {
        public ColumnMetadataList() {}

        public ColumnMetadataList(IDataReader reader)
        {
            var schemaTable = reader.GetSchemaTable();
            if (schemaTable == null)
                throw new InvalidOperationException("reader.GetSchemaTable() returned null value");
            foreach (DataRow field in schemaTable.Rows)
            {
                var meta = new ColumnMetadata();
                Add(meta);
                foreach (DataColumn column in schemaTable.Columns)
                {
                    var prop = meta.GetType().GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                        continue;
                    var value = field[column];
                    meta.SetPropertyValue(prop, !(value is DBNull) ? value : null);
                }
            }
        }
    }
}
