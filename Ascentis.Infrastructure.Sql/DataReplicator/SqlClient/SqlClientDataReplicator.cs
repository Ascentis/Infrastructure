using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataReplicator.Generic;
using Ascentis.Infrastructure.Sql.DataPipeline.SourceAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk;

namespace Ascentis.Infrastructure.DataReplicator.SqlClient
{
    // ReSharper disable once InconsistentNaming
    public class SqlClientDataReplicator<TSourceAdapter> : 
        DataReplicator<SqlCommand, SqlConnection, SqlClientAdapterBulkInsert, SqlClientDataPipeline, TSourceAdapter>
        where TSourceAdapter : SourceAdapterSqlBase
    {
        private readonly IDictionary<Type, string> _typeToExprMap;
        
        public SqlClientDataReplicator(
            string sourceConnStr, 
            string targetConnStr, 
            int parallelismLevel = DefaultParallelismLevel) : base(sourceConnStr, targetConnStr, parallelismLevel)
        {
            _typeToExprMap = new Dictionary<Type, string>
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
        }

        protected override string BuildDropTableStatement(string tableName)
        {
            return @$"IF OBJECT_ID('dbo.{tableName}', 'U') IS NOT NULL DROP TABLE dbo.{tableName};";
        }

        protected override string BuildCreateTableStatement(string tableName, ColumnMetadataList metadatas)
        {
            const string defDelimiter = ",\r\n";

            var statement = $"CREATE TABLE {tableName} (\r\n";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var colDef in metadatas)
                statement += @$"{colDef.ColumnName} 
                                {(colDef.DataTypeName != string.Empty ? colDef.DataTypeName : _typeToExprMap[colDef.DataType])} 
                                {((colDef.DataType == typeof(string) || colDef.DataType == typeof(byte[])) && colDef.DataTypeName != "text" 
                                    ? "(" + (colDef.ColumnSize == int.MaxValue 
                                        ? "MAX" 
                                        : colDef.ColumnSize.ToString()) + ")" 
                                    : "")}
                                {(colDef.DataType == typeof(decimal) ? "(" + colDef.NumericPrecision + "," + colDef.NumericScale + ")" : "")}
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
    }
}
