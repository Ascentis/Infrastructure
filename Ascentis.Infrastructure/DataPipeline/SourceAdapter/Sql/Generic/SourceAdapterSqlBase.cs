using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic
{
    public abstract class SourceAdapterSqlBase : SourceAdapter<PoolEntry<object[]>>
    {
        public const int DefaultRowsCapacity = 1000;

        private readonly Pool<object[]> _rowsPool;
        private DbDataReader _sqlDataReader;
        private readonly string _connectionString;
        private readonly string _sqlCommandText;
        private DbConnection _connection;
        private DbCommand _command;
        private bool _ownsReader;
        private readonly IClassSqlBuilder _sqlBuilder;

        public override int RowsPoolSize
        {
            get => _rowsPool.MaxCapacity; 
            set => _rowsPool.MaxCapacity = value;
        }

        protected SourceAdapterSqlBase(DbDataReader sqlDataReader, int rowsPoolCapacity)
        {
            _sqlDataReader = sqlDataReader;
            _rowsPool = new Pool<object[]>(rowsPoolCapacity, pool => pool.NewPoolEntry(new object[_sqlDataReader.FieldCount], ParallelLevel));
            _sqlBuilder = this;
        }

        protected SourceAdapterSqlBase(DbDataReader sqlDataReader) : this(sqlDataReader, DefaultRowsCapacity) { }

        protected SourceAdapterSqlBase(
            string connectionString, 
            string sqlCommandText,
            int rowsPoolCapacity = DefaultRowsCapacity) : this(null, rowsPoolCapacity)
        {
            _connectionString = connectionString;
            _sqlCommandText = sqlCommandText;
        }
        
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
            if (_sqlDataReader == null)
            {
                _connection = _sqlBuilder.BuildConnection(_connectionString);
                _connection.Open();
                _command = _sqlBuilder.BuildCommand(_sqlCommandText, _connection);
                _sqlDataReader = _command.ExecuteReader();
                _ownsReader = true;
            }

            if (ColumnMetadatas != null)
                return;
            base.ColumnMetadatas = new ColumnMetadataList(_sqlDataReader);
        }

        public override void UnPrepare()
        {
            _sqlDataReader?.Close();
            if (_ownsReader)
                _sqlDataReader?.Dispose();
            _command?.Dispose();
            _connection?.Dispose();
            base.UnPrepare();
        }

        public override ColumnMetadataList ColumnMetadatas {
            get => base.ColumnMetadatas;
            set => throw new InvalidOperationException($"Can't set ColumnMetadatas for {GetType().Name}");
        }
    }
}
