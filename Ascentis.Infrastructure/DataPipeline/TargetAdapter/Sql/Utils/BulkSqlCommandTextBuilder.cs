using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public class BulkSqlCommandTextBuilder
    {
        public delegate string GetValueAsSqlStringDelegate(object value);

        private readonly GetValueAsSqlStringDelegate _getNativeValueAsSqlString = value => value.ToString();
        public bool LiteralParamBinding { get; set; }
        public string TableName { get; set; }
        public IEnumerable<string> ColumnNames { get; set; }
        public IDictionary<string, int> ColumnNameToMetadataIndexMap { get; set; }

        public BulkSqlCommandTextBuilder(GetValueAsSqlStringDelegate getValueAsSqlString = null)
        {
            if (getValueAsSqlString != null)
                _getNativeValueAsSqlString = getValueAsSqlString;
        }
        
        public string BuildBulkInsertSql(List<PoolEntry<object[]>> rows)
        {
            const string rowSeparator = ",\r\n";

            ArgsChecker.CheckForNull<ArgumentNullException>(rows, nameof(rows));
            ArgsChecker.CheckForNull<InvalidOperationException>(ColumnNames, "ColumnNames can't be null");
            if (TableName == string.Empty)
                throw new InvalidOperationException("TableName can't be blank");
            if (LiteralParamBinding)
                ArgsChecker.CheckForNull<InvalidOperationException>(ColumnNameToMetadataIndexMap, "ColumnNameToMetadataIndexMap can't be null");

            var stringBuilder = new StringBuilder($"INSERT INTO {TableName}\r\n(");

            foreach (var columnName in ColumnNames)
                stringBuilder.Append($"\"{columnName}\",");
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(") VALUES\r\n");
            for (var i = 0; i < rows.Count; i++)
            {
                stringBuilder.Append("(");
                if (!LiteralParamBinding)
                {
                    var columnIndex = 0;
                    foreach (var dummy in ColumnNames)
                        stringBuilder.Append($"@P{columnIndex++}_{i},");
                }
                else
                {
                    foreach (var column in ColumnNameToMetadataIndexMap)
                        stringBuilder.Append(_getNativeValueAsSqlString(column.Value >= 0 ? rows[i].Value[column.Value] : DBNull.Value) + ",");
                }

                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                stringBuilder.Append($"){rowSeparator}");
            }
            stringBuilder.Remove(stringBuilder.Length - rowSeparator.Length, rowSeparator.Length);

            return stringBuilder.ToString();
        }

        public string BuildBulkSql(
            string sqlCommandText, 
            List<PoolEntry<object[]>> rows, 
            bool paramsAsList)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(rows, nameof(rows));
            ArgsChecker.CheckForNull<InvalidOperationException>(ColumnNames, "ColumnNames can't be null");
            if (LiteralParamBinding)
                ArgsChecker.CheckForNull<InvalidOperationException>(ColumnNameToMetadataIndexMap, "ColumnNameToMetadataIndexMap can't be null");

            var stringBuilder = new StringBuilder(!paramsAsList ? "SELECT " : "");

            var columnNumber = 0;
            if (!paramsAsList)
            {
                if (!LiteralParamBinding)
                {
                    foreach (var columnName in ColumnNames)
                        stringBuilder.Append($"@P{columnNumber++}_0 \"{columnName}\",");
                }
                else
                {
                    foreach (var column in ColumnNameToMetadataIndexMap)
                        stringBuilder.Append($"{_getNativeValueAsSqlString(column.Value >= 0 ? rows[0].Value[column.Value] : DBNull.Value)} \"{column.Key}\",");
                }

                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            for (var i = paramsAsList ? 0 : 1; i < rows.Count; i++)
            {
                stringBuilder.Append(!paramsAsList ? "\r\nUNION ALL\r\nSELECT " : "");
                if (!LiteralParamBinding)
                {
                    columnNumber = 0;
                    foreach (var dummy in ColumnNames)
                        stringBuilder.Append($"@P{columnNumber++}_{i},");
                }
                else
                {
                    foreach (var column in ColumnNameToMetadataIndexMap)
                        stringBuilder.Append(_getNativeValueAsSqlString(column.Value >= 0 ? rows[i].Value[column.Value] : DBNull.Value) + ",");
                }

                if (!paramsAsList || i == rows.Count - 1)
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            var newSqlCommandText = Regex.Replace(sqlCommandText,
                @"(\/\*\<DATA\>\*\/.*?\/\*\<\/DATA\>\*\/)|@@@Parameters|@@@Params",
                stringBuilder.ToString(),
                RegexOptions.Compiled | RegexOptions.Singleline);

            return newSqlCommandText;
        }
    }
}
