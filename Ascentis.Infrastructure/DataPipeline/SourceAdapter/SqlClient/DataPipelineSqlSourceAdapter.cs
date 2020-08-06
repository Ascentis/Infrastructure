using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient
{
    public class DataPipelineSqlSourceAdapter : DataPipelineSourceAdapter<object[]>
    {
        public const int DefaultRowsCapacity = 1000;

        public event DataPipeline<object[]>.RowErrorDelegate OnSourceAdapterRowReadError;

        private readonly Pool<object[]> _rowsPool;
        private readonly SqlDataReader _sqlDataReader;

        public DataPipelineSqlSourceAdapter(SqlDataReader sqlDataReader, int rowsPoolCapacity)
        {
            _sqlDataReader = sqlDataReader;
            _rowsPool = new Pool<object[]>(rowsPoolCapacity, () => new object[_sqlDataReader.FieldCount]);
        }

        public DataPipelineSqlSourceAdapter(SqlDataReader sqlDataReader) : this(sqlDataReader, DefaultRowsCapacity) { }
        
        public override void ReleaseRow(object[] row)
        {
            _rowsPool.Release(row);
        }

        public override IEnumerable<object[]> RowsEnumerable
        {
            get
            {
                while (_sqlDataReader.Read())
                {
                    var row = _rowsPool.Acquire();
                    _sqlDataReader.GetValues(row);
                    yield return row;
                }
            }
        }

        public override int FieldCount => _sqlDataReader.FieldCount;

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void Prepare()
        {
            base.Prepare();

            if (ColumnMetadatas != null)
                return;

            var schemaTable = _sqlDataReader.GetSchemaTable();
            base.ColumnMetadatas = new DataPipelineColumnMetadata[FieldCount];

            var columnIndex = 0;
            foreach (DataRow field in schemaTable.Rows)
            {
                ColumnMetadatas[columnIndex] = new DataPipelineColumnMetadata();
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

        public override DataPipelineColumnMetadata[] ColumnMetadatas {
            get => base.ColumnMetadatas;
            set => throw new InvalidOperationException($"Can't set ColumnMetadatas for {GetType().Name}");
        }
    }
}
