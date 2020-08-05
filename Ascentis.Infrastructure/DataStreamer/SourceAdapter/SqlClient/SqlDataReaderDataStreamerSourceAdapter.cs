using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Ascentis.Infrastructure.DataStreamer.SourceAdapter.SqlClient
{
    public class SqlDataReaderDataStreamerSourceAdapter : IDataStreamerSourceAdapter
    {
        public const int DefaultRowsCapacity = 1000;

        private readonly Pool<object[]> _rowsPool;
        private readonly SqlDataReader _sqlDataReader;
        private DataStreamerColumnMetadata[] _columnMetadatas;

        public SqlDataReaderDataStreamerSourceAdapter(SqlDataReader sqlDataReader, int rowsPoolCapacity)
        {
            _sqlDataReader = sqlDataReader;
            _rowsPool = new Pool<object[]>(rowsPoolCapacity, () => new object[_sqlDataReader.FieldCount]);
        }

        public SqlDataReaderDataStreamerSourceAdapter(SqlDataReader sqlDataReader) : this(sqlDataReader, DefaultRowsCapacity) {}

        public void ReleaseRow(object[] row)
        {
            _rowsPool.Release(row);
        }

        public IEnumerable<object[]> RowsEnumerable
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

        public int FieldCount => _sqlDataReader.FieldCount;

        public DataStreamerColumnMetadata[] ColumnMetadatas {
            get
            {
                if (_columnMetadatas != null)
                    return _columnMetadatas;

                var schemaTable = _sqlDataReader.GetSchemaTable();
                _columnMetadatas = new DataStreamerColumnMetadata[FieldCount];
    
                var columnIndex = 0;
                // ReSharper disable once PossibleNullReferenceException
                foreach (DataRow field in schemaTable.Rows)
                {
                    _columnMetadatas[columnIndex] = new DataStreamerColumnMetadata();
                    foreach (DataColumn column in schemaTable.Columns)
                    {
                        var prop = _columnMetadatas[columnIndex].GetType().GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
                        if (prop == null)
                            continue;
                        var value = field[column];
                        prop.SetValue(_columnMetadatas[columnIndex], !(value is DBNull) ? value : null);
                    }
    
                    columnIndex++;
                }

                return _columnMetadatas;
            }
        }
    }
}
