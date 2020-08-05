using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlDataReaderStreamerAdapter : IEnumerable<object[]>, IStreamerAdapter
    {
        public const int DefaultRowsCapacity = 1000;
        private readonly Pool<object[]> _rowsPool;
        private readonly SqlDataReader _sqlDataReader;

        public SqlDataReaderStreamerAdapter(SqlDataReader sqlDataReader, int rowsPoolCapacity)
        {
            _sqlDataReader = sqlDataReader;
            _rowsPool = new Pool<object[]>(rowsPoolCapacity, () => new object[_sqlDataReader.FieldCount]);
        }

        public SqlDataReaderStreamerAdapter(SqlDataReader sqlDataReader) : this(sqlDataReader, DefaultRowsCapacity) {}

        public IEnumerator<object[]> GetEnumerator()
        {
            while (_sqlDataReader.Read())
            {
                var row = _rowsPool.Acquire();
                _sqlDataReader.GetValues(row);
                yield return row;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ReleaseRow(object[] row)
        {
            _rowsPool.Release(row);
        }

        public IEnumerable<object[]> GetEnumerable()
        {
            return this;
        }

        public int FieldCount => _sqlDataReader.FieldCount;
    }
}
