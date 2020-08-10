using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient
{
    public class SourceAdapterSql : SourceAdapter<PoolEntry<object[]>>
    {
        public const int DefaultRowsCapacity = 1000;

        public event DataPipeline<object[]>.RowErrorDelegate OnSourceAdapterRowReadError;

        private readonly Pool<object[]> _rowsPool;
        private readonly SqlDataReader _sqlDataReader;

        public override int RowsPoolSize
        {
            get => _rowsPool.MaxCapacity; 
            set => _rowsPool.MaxCapacity = value;
        }

        public SourceAdapterSql(SqlDataReader sqlDataReader, int rowsPoolCapacity)
        {
            _sqlDataReader = sqlDataReader;
            _rowsPool = new Pool<object[]>(rowsPoolCapacity, pool => pool.NewPoolEntry(new object[_sqlDataReader.FieldCount], ParallelLevel));
        }

        public SourceAdapterSql(SqlDataReader sqlDataReader) : this(sqlDataReader, DefaultRowsCapacity) { }
        
        public override void ReleaseRow(PoolEntry<object[]> row)
        {
            _rowsPool.Release(row);
        }

        public override IEnumerable<PoolEntry<object[]>> RowsEnumerable
        {
            get
            {
                while (_sqlDataReader.Read())
                {
                    var row = _rowsPool.Acquire();
                    _sqlDataReader.GetValues(row.Value);
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
            base.ColumnMetadatas = new ColumnMetadata[FieldCount];

            var columnIndex = 0;
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

        public override ColumnMetadata[] ColumnMetadatas {
            get => base.ColumnMetadatas;
            set => throw new InvalidOperationException($"Can't set ColumnMetadatas for {GetType().Name}");
        }
    }
}
