using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataReplicator.Generic;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Bulk;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Utils;

namespace Ascentis.Infrastructure.Sql.DataReplicator.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteDataReplicator<TSourceAdapter> : 
        DataReplicator<SQLiteCommand, SQLiteConnection, SQLiteAdapterBulkInsert, SQLiteDataPipeline, TSourceAdapter>
        where TSourceAdapter : SourceAdapterSqlBase
    {
        private readonly IDictionary<Type, string> _typeToExprMap;

        static SQLiteDataReplicator()
        {
            SQLiteTypeMappings.InitAutoMapper();
        }

        public SQLiteDataReplicator(
            string sourceConnStr, 
            string targetConnStr, 
            int parallelismLevel = DefaultParallelismLevel) : base(sourceConnStr, targetConnStr, parallelismLevel)
        {
            _typeToExprMap = new Dictionary<Type, string>
            {
                {typeof(bool), "int_bit"},
                {typeof(byte), "tinyint"},
                {typeof(byte[]), "blob_binary"},
                {typeof(char), "nchar"},
                {typeof(DateTime), "datetime"},
                {typeof(DateTimeOffset), "datetimeoffset"},
                {typeof(decimal), "text_decimal"},
                {typeof(double), "double"},
                {typeof(Guid), "text_uniqueidentifier"},
                {typeof(short), "smallint"},
                {typeof(int), "int"},
                {typeof(long), "bigint"},
                {typeof(float), "float"},
                {typeof(string), "nvarchar"},
                {typeof(TimeSpan), "time"}
            };
            UseTransaction = true;
        }

        protected override string BuildDropTableStatement(string tableName)
        {
            return $"DROP TABLE IF EXISTS {tableName}";
        }

        protected override string BuildCreateTableStatement(string tableName, ColumnMetadataList metadatas)
        {
            const string defDelimiter = ",\r\n";

            var statement = $"CREATE TABLE {tableName} (\r\n";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var colDef in metadatas)
                statement += $"{colDef.ColumnName} {_typeToExprMap[colDef.DataType]} {(colDef.IsIdentity ?? false ? "PRIMARY KEY" : "")}{defDelimiter}";
            statement = statement.Remove(statement.Length - defDelimiter.Length, defDelimiter.Length);
            statement += ")";

            return statement;
        }

        protected override bool TableExists(string tableName, DbConnection connection)
        {
            using var tableExistsCmd = new SQLiteCommand($"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'", (SQLiteConnection)connection);
            var cnt = (long)tableExistsCmd.ExecuteScalar();
            return cnt > 0;
        }

        protected override string BuildTruncateTableStatement(string tableName)
        {
            return $"DELETE FROM {tableName}";
        }

        protected override void ConfigureTargetConnection(SQLiteConnection connection, int columnCount, int batchSize)
        {
            var paramsRequired = columnCount * batchSize;
            if (paramsRequired > SQLiteUtils.DefaultMaxSQLiteParams)
                connection.SetLimitOption(SQLiteLimitOpsEnum.SQLITE_LIMIT_VARIABLE_NUMBER, paramsRequired);
        }
    }
}
