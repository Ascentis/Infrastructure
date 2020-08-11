using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Bulk
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteAdapterBulkInsert : TargetAdapterBulkInsertBase<SQLiteCommand, SQLiteTransaction, SQLiteConnection, DbException>, 
        ITargetAdapterBulk, 
        ITargetAdapterSQLite
    {
        public SQLiteAdapterBulkInsert(string tableName,
            IEnumerable<string> columnNames,
            SQLiteConnection sqlConnection,
            int batchSize = SQLiteUtils.DefaultBatchSize) : base(tableName, columnNames, sqlConnection, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
            sqlConnection.SetLimitOption(SQLiteLimitOpsEnum.SQLITE_LIMIT_FUNCTION_ARG, SQLiteUtils.DefaultMaxSQLiteParams);
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SQLiteCommand sqlCommand, int rowCount)
        {
            SQLiteUtils.ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, sqlCommand.Parameters, rowCount);
        }
    }
}
