using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Bulk;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Utils;
using Ascentis.Infrastructure.DataReplicator.Generic;

namespace Ascentis.Infrastructure.DataReplicator.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteDataReplicator : DataReplicator<SQLiteCommand, SQLiteConnection, SQLiteAdapterBulkInsert, SQLiteDataPipeline>
    {
        private readonly IDictionary<Type, string> _typeToExprMap;
        
        public SQLiteDataReplicator(
            string sourceConnStr, 
            string targetConnStr, 
            int parallelismLevel = DefaultParallelismLevel) : base(sourceConnStr, targetConnStr, parallelismLevel)
        {
            _typeToExprMap = new Dictionary<Type, string>
            {
                {typeof(bool), "INTEGER"},
                {typeof(byte), "INTEGER"},
                {typeof(byte[]), "BLOB"},
                {typeof(char), "TEXT"},
                {typeof(DateTime), "TEXT"},
                {typeof(DateTimeOffset), "TEXT"},
                {typeof(decimal), "TEXT"},
                {typeof(double), "REAL"},
                {typeof(Guid), "TEXT"},
                {typeof(short), "INTEGER"},
                {typeof(int), "INTEGER"},
                {typeof(long), "INTEGER"},
                {typeof(sbyte), "INTEGER"},
                {typeof(float), "REAL"},
                {typeof(string), "TEXT"},
                {typeof(TimeSpan), "TEXT"},
                {typeof(ushort), "INTEGER"},
                {typeof(uint), "INTEGER"},
                {typeof(ulong), "INTEGER"}
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
