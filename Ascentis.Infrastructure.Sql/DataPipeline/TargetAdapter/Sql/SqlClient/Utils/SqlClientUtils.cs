using System;
using System.Diagnostics.CodeAnalysis;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SqlClient.Utils
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SqlClientUtils
    {
        public const int DefaultBatchSize = 100;
        public const int MaxMSSQLParams = 2100;
        public const int MaxMSSQLInsertRows = 1000;

        public static readonly ColumnMetadataToSqlDbTypeMapper ParamMapper =
            new ColumnMetadataToSqlDbTypeMapper
            {
                UseShortParam = true
            };

        public static string ValueToSqlLiteralText(object obj)
        {
            return obj switch
            {
                string s => $"'{s.Replace("'", "''")}'",
                DateTime dateTime => $"'{dateTime}'",
                DateTimeOffset dateTimeOffset => $"'{dateTimeOffset}'",
                TimeSpan timeSpan => $"'{timeSpan}'",
                Guid guid => $"'{guid}'",
                bool b => b ? "1" : "0",
                _ => (obj is DBNull ? "NULL" : obj.ToString())
            };
        }
    }
}
