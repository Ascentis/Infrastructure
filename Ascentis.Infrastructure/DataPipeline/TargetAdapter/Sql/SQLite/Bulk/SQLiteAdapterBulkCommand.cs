using System.Collections.Generic;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Bulk
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteAdapterBulkCommand : TargetAdapterSqlBulkBase<SQLiteCommand, SQLiteTransaction, SQLiteConnection>, 
        ITargetAdapterBulk, 
        ITargetAdapterSQLite
    {
        private readonly string _sqlCommandText;

        public SQLiteAdapterBulkCommand(string sqlCommandText,
            IEnumerable<string> sourceColumnNames,
            SQLiteConnection conn,
            int batchSize = SQLiteUtils.DefaultBatchSize) : base(sourceColumnNames, conn, batchSize)
        {
            _sqlCommandText = sqlCommandText;
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
            conn.SetLimitOption(SQLiteLimitOpsEnum.SQLITE_LIMIT_VARIABLE_NUMBER, SQLiteUtils.DefaultMaxSQLiteParams);
        }

        protected override string BuildBulkSql(int rowCount)
        {
            return BulkSqlCommandTextBuilder.BuildBulkSql(ColumnNames, _sqlCommandText, rowCount);
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
            SQLiteUtils.ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, sqlCommand.Parameters, rowCount);
        }
    }
}
