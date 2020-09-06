using System;
using System.Collections.Generic;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataReplicator.Generic;
using Ascentis.Infrastructure.Sql.DataPipeline.SourceAdapter.Sql.Oracle;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Bulk;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataReplicator.Oracle
{
    // ReSharper disable once InconsistentNaming
    public class OracleDataReplicator<TSourceAdapter> : 
        DataReplicator<OracleCommand, OracleConnection, OracleAdapterBulkInsert, OracleDataPipeline, TSourceAdapter>
        where TSourceAdapter : SourceAdapterSqlBase
    {
        public const int MaxNVARCHAR2Size = 32767 / 2; // New Oracle limit when running with MAX_STRING_SIZE in EXTENDED mode
        /* See https://docs.oracle.com/en/database/oracle/oracle-database/19/refrn/MAX_STRING_SIZE.html#GUID-D424D23B-0933-425F-BC69-9C0E6724693C for config */
        private readonly IDictionary<Type, string> _typeToExprMap;
        
        public OracleDataReplicator(
            string sourceConnStr, 
            string targetConnStr, 
            int parallelismLevel = DefaultParallelismLevel) : base(sourceConnStr, targetConnStr, parallelismLevel)
        {
            _typeToExprMap = new Dictionary<Type, string>
            {
                {typeof(bool), "NUMBER(1)"},
                {typeof(byte), "NUMBER(3)"},
                {typeof(byte[]), "BLOB"},
                {typeof(char), "CHAR"},
                {typeof(DateTime), "DATE"},
                {typeof(decimal), "NUMBER"},
                {typeof(double), "NUMBER(38)"},
                {typeof(Guid), "VARCHAR(36)"},
                {typeof(short), "NUMBER(5)"},
                {typeof(int), "NUMBER(10)"},
                {typeof(long), "NUMBER(19)"},
                {typeof(float), "FLOAT(49)"},
                {typeof(string), "NVARCHAR2"}
            };
            IgnoreDropTableException = true;
        }
         
        protected override string BuildDropTableStatement(string tableName)
        {
            return$"DROP TABLE {tableName}";
        }

        protected override string BuildCreateTableStatement(string tableName, ColumnMetadataList metadatas)
        {
            const string defDelimiter = ",\r\n";

            var statement = $"CREATE TABLE {tableName} (\r\n";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var colDef in metadatas)
                statement += @$"{colDef.ColumnName} 
                                {(colDef.DataTypeName != string.Empty && typeof(TSourceAdapter) == typeof(OracleSourceAdapter) 
                                    ? colDef.DataTypeName 
                                    : _typeToExprMap[colDef.DataType])} 
                                {(colDef.DataType == typeof(string) ? "(" + Math.Min(colDef.ColumnSize ?? MaxNVARCHAR2Size, MaxNVARCHAR2Size) + ")" : "")}
                                {(colDef.DataType == typeof(decimal) ? "(" + colDef.NumericPrecision + "," + colDef.NumericScale + ")" : "")}
                                {(colDef.IsIdentity ?? false ? "PRIMARY KEY" : "")}{defDelimiter}";
            statement = statement.Remove(statement.Length - defDelimiter.Length, defDelimiter.Length);
            statement += ")";

            return statement;
        }

        protected override bool TableExists(string tableName, DbConnection connection)
        {
            using var tableExistsCmd = new OracleCommand($"SELECT TABLE_NAME FROM USER_TABLES WHERE TABLE_NAME = '{tableName}'", (OracleConnection)connection);
            var cnt = tableExistsCmd.ExecuteScalar();
            return !(cnt is DBNull);
        }

        protected override string BuildTruncateTableStatement(string tableName)
        {
            return $"TRUNCATE TABLE {tableName} DROP STORAGE";
        }
    }
}
