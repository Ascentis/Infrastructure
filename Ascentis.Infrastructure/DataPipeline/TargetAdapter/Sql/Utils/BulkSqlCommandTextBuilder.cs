using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static class BulkSqlCommandTextBuilder
    {
        public static string BuildBulkInsertSql(string tableName, IEnumerable<string> columnNames, int rowCount)
        {
            const string rowSeparator = ",\r\n";

            var sqlText = $"INSERT INTO {tableName}\r\n(";
            foreach (var columnName in columnNames)
                sqlText += $"\"{columnName}\",";
            sqlText = sqlText.Remove(sqlText.Length - 1, 1);
            sqlText += ") VALUES\r\n";
            for (var i = 0; i < rowCount; i++)
            {
                sqlText += "(";
                var columnIndex = 0;
                foreach (var dummy in columnNames)
                    sqlText += $"@P{columnIndex++}_{i},";

                sqlText = sqlText.Remove(sqlText.Length - 1, 1);
                sqlText += $"){rowSeparator}";
            }
            sqlText = sqlText.Remove(sqlText.Length - rowSeparator.Length, rowSeparator.Length);

            return sqlText;
        }

        public static string BuildBulkSql(IEnumerable<string> columnNames, string sqlCommandText, int rowCount, bool paramsAsList)
        {
            var sourceSql = !paramsAsList ? "SELECT " : "";
            var columnNumber = 0;
            if (!paramsAsList)
            {
                foreach (var columnName in columnNames)
                    sourceSql += $"@P{columnNumber++}_0 \"{columnName}\",";
                sourceSql = sourceSql.Remove(sourceSql.Length - 1, 1);
            }

            for (var i = paramsAsList ? 0 : 1; i < rowCount; i++)
            {
                sourceSql += !paramsAsList ? "\r\nUNION ALL\r\nSELECT " : "";
                columnNumber = 0;
                foreach (var dummy in columnNames)
                    sourceSql += $"@P{columnNumber++}_{i},";
                if (!paramsAsList || i == rowCount - 1)
                    sourceSql = sourceSql.Remove(sourceSql.Length - 1, 1);
            }

            var newSqlCommandText = Regex.Replace(sqlCommandText,
                @"(\/\*\<DATA\>\*\/.*?\/\*\<\/DATA\>\*\/)|@@@Parameters|@@@Params",
                sourceSql,
                RegexOptions.Compiled | RegexOptions.Singleline);

            return newSqlCommandText;
        }
    }
}
