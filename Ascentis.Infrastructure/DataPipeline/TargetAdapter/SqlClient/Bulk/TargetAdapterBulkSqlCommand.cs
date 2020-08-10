using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Bulk
{
    public class TargetAdapterBulkSqlCommand : TargetAdapterBulk
    {
        private readonly string _sqlCommandText;

        public TargetAdapterBulkSqlCommand(string sqlCommandText,
            IEnumerable<string> sourceColumnNames,
            SqlConnection sqlConnection,
            int batchSize = DefaultBatchSize) : base(sourceColumnNames, sqlConnection, batchSize)
        {
            _sqlCommandText = sqlCommandText;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        protected override string BuildBulkSql(int rowCount)
        {
            var sourceSql = "SELECT ";
            var columnNumber = 0;
            foreach (var columnName in ColumnNames)
                sourceSql += $"@P{columnNumber++}_0 \"{columnName}\",";
            sourceSql = sourceSql.Remove(sourceSql.Length - 1, 1);

            for (var i = 1; i < rowCount; i++)
            {
                sourceSql += $"\r\nUNION ALL\r\nSELECT ";
                columnNumber = 0;
                foreach (var dummy in ColumnNames)
                    sourceSql += $"@P{columnNumber++}_{i},";
                sourceSql = sourceSql.Remove(sourceSql.Length - 1, 1);
            }

            var newSqlCommandText = Regex.Replace(_sqlCommandText, 
                @"(.*)(\/\*\<DATA\>\*\/.*\/\*\<\/DATA\>\*\/)(.*)", 
                $"$1{sourceSql}$3",
                RegexOptions.Compiled | RegexOptions.Singleline);

            return newSqlCommandText;
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
    }
}
