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
        private readonly DbDataReader _sqlDataReader;

        public override int RowsPoolSize
        {
            get => _rowsPool.MaxCapacity; 
            set => _rowsPool.MaxCapacity = value;
        }

        protected SourceAdapterSqlBase(DbDataReader sqlDataReader, int rowsPoolCapacity)
        {
            _sqlDataReader = sqlDataReader;
            _rowsPool = new Pool<object[]>(rowsPoolCapacity, pool => pool.NewPoolEntry(new object[_sqlDataReader.FieldCount], ParallelLevel));
        }

        protected SourceAdapterSqlBase(DbDataReader sqlDataReader) : this(sqlDataReader, DefaultRowsCapacity) { }
        
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
            base.ColumnMetadatas = new ColumnMetadataList(_sqlDataReader);
        }

        public override ColumnMetadataList ColumnMetadatas {
            get => base.ColumnMetadatas;
            set => throw new InvalidOperationException($"Can't set ColumnMetadatas for {GetType().Name}");
        }
    }
}
