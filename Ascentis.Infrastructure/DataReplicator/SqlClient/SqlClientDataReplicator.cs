using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk;
using Ascentis.Infrastructure.DataReplicator.Generic;

namespace Ascentis.Infrastructure.DataReplicator.SqlClient
{
    // ReSharper disable once InconsistentNaming
    public class SqlClientDataReplicator : DataReplicator<SqlCommand, SqlConnection>
    {
        private IDictionary<Type, string> TypeToExprMap { get; }
        
        public SqlClientDataReplicator(
            string sourceConnStr, 
            string targetConnStr, 
            int parallelismLevel = DefaultParallelismLevel) : base(sourceConnStr, targetConnStr, parallelismLevel)
        {
            TypeToExprMap = new Dictionary<Type, string>
            {
                {typeof(bool), "bit"},
                {typeof(byte), "tinyint"},
                {typeof(byte[]), "binary"},
                {typeof(char), "char"},
                {typeof(DateTime), "datetime"},
                {typeof(DateTimeOffset), "datetimeoffset"},
                {typeof(decimal), "decimal"},
                {typeof(double), "float"},
                {typeof(Guid), "uniqueidentifier"},
                {typeof(short), "smallint"},
                {typeof(int), "int"},
                {typeof(long), "bigint"},
                {typeof(float), "float"},
                {typeof(string), "nvarchar"},
                {typeof(TimeSpan), "time"}
            };
            //UseTransaction = true;
        }

        protected override string BuildDropTableStatement(string tableName)
        {
            return @$"IF OBJECT_ID('dbo.{tableName}', 'U') IS NOT NULL 
                        DROP TABLE dbo.{tableName};";
        }

        protected override string BuildCreateTableStatement(string tableName, ColumnMetadataList metadatas)
        {
            const string defDelimiter = ",\r\n";

            var statement = $"CREATE TABLE {tableName} (\r\n";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var colDef in metadatas)
                statement += @$"{colDef.ColumnName} 
                                {TypeToExprMap[colDef.DataType]} 
                                {(colDef.DataType == typeof(string) || colDef.DataType == typeof(byte[]) ? "(" + (colDef.ColumnSize == int.MaxValue ? "MAX" : colDef.ColumnSize.ToString()) + ")" : "")} 
                                {(colDef.IsIdentity ?? false ? "PRIMARY KEY" : "")}{defDelimiter}";
            statement = statement.Remove(statement.Length - defDelimiter.Length, defDelimiter.Length);
            statement += ")";

            return statement;
        }

        protected override bool TableExists(string tableName, DbConnection connection)
        {
            using var tableExistsCmd = new SqlCommand($"SELECT OBJECT_ID('dbo.{tableName}', 'U')", (SqlConnection)connection);
            var cnt = tableExistsCmd.ExecuteScalar();
            return !(cnt is DBNull);
        }

        protected override string BuildTruncateTableStatement(string tableName)
        {
            return $"TRUNCATE TABLE {tableName}";
        }

        protected override TargetAdapterSql BuildTargetAdapter(string tableName, IEnumerable<string> columnNames, SqlConnection conn, int batchSize)
        {
            var adapter = new SqlClientAdapterBulkInsert(tableName, columnNames, conn, batchSize)
            {
                UseNativeTypeConvertor = UseNativeTypeConvertor
            };
            return adapter;
        }

        protected override DataPipeline<PoolEntry<object[]>> BuildDataPipeline()
        {
            return new SqlClientDataPipeline();
        }
    }
}
