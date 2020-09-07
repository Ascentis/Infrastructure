using System.Collections.Generic;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Utils;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Bulk
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteAdapterBulkCommand : TargetAdapterSqlBulkBase<SQLiteCommand, SQLiteTransaction, SQLiteConnection>, 
        ITargetAdapterFlushable, 
        ITargetAdapterSQLite
    {
        public SQLiteAdapterBulkCommand(string sqlCommandText,
            IEnumerable<string> sourceColumnNames,
            SQLiteConnection conn,
            int batchSize = SQLiteUtils.DefaultBatchSize) : base(sqlCommandText, sourceColumnNames, conn, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
            conn.SetLimitOption(SQLiteLimitOpsEnum.SQLITE_LIMIT_VARIABLE_NUMBER, SQLiteUtils.DefaultMaxSQLiteParams);
        }

        public override string ValueToSqlLiteralText(object obj)
        {
            return SQLiteUtils.ValueToSqlLiteralText(obj);
        }

        public override void Flush()
        {
            try
            {
                InternalFlush();
            }
            finally
            {
                InternalReleaseRows();
            }
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SQLiteCommand sqlCommand, int rowCount)
        {
            var metaToParamSettings = new MetaToParamSettings
            {
                Columns = ColumnNames,
                Metadatas = Source.ColumnMetadatas,
                AnsiStringParameters = AnsiStringParameters,
                ColumnToIndexMap = paramToMetaIndex,
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
