﻿using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Utils;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Bulk
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteAdapterBulkInsert : TargetAdapterBulkInsertBase<SQLiteCommand, SQLiteTransaction, SQLiteConnection, DbException>, 
        ITargetAdapterFlushable, 
        ITargetAdapterSQLite
    {
        public SQLiteAdapterBulkInsert(string tableName,
            IEnumerable<string> columnNames,
            SQLiteConnection conn,
            int batchSize = SQLiteUtils.DefaultBatchSize) : base(tableName, columnNames, conn, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
            conn.SetLimitOption(SQLiteLimitOpsEnum.SQLITE_LIMIT_VARIABLE_NUMBER, SQLiteUtils.DefaultMaxSQLiteParams);
        }

        public override string ValueToSqlLiteralText(object obj)
        {
            return SQLiteUtils.ValueToSqlLiteralText(obj);
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SQLiteCommand sqlCommand, int rowCount)
        {
            var metaToParamSettings = new MetaToParamSettings
            {
                Columns = ColumnNames,
                Metadatas = Source.ColumnMetadatas,
                AnsiStringParameters = AnsiStringParameters,
                ColumnToIndexMap = ColumnNameToMetadataIndexMap,
                Target = sqlCommand.Parameters,
                UseShortParam = true
            };
            SQLiteUtils.ParamMapper.Map(metaToParamSettings, rowCount);
        }

        public override object GetNativeValue(object value)
        {
            return SQLiteUtils.GetNativeValue(value);
        }
    }
}
