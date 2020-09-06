using System;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SQLite.Utils
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteUtils
    {
        public const int DefaultBatchSize = 100;
        // ReSharper disable once InconsistentNaming
        public const int DefaultMaxSQLiteParams = 2100;

        public static readonly ColumnMetadataToSQLiteDbTypeMapper ParamMapper = new ColumnMetadataToSQLiteDbTypeMapper();

        public static object GetNativeValue(object value)
        {
            return value switch
            {
                DateTime time => time.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF"),
                bool b => b ? 1 : 0,
                DateTimeOffset offset => offset.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFFzzz"),
                decimal n => n.ToString("0.0###########################"),
                Guid guid => guid.ToString("00000000-0000-0000-0000-000000000000"),
                TimeSpan timeSpan => timeSpan.ToString("d.hh:mm:ss.fffffff"),
                _ => value
            };
        }

        public static string ValueToSqlLiteralText(object obj)
        {
            return obj switch
            {
                string s => $"'{s.Replace("'", "''")}'",
                DateTime dateTime => $"'{dateTime:yyyy-MM-dd HH:mm:ss.FFFFFFF}'",
                DateTimeOffset dateTimeOffset => $"'{dateTimeOffset:yyyy-MM-dd HH:mm:ss.FFFFFFFzzz}'",
                TimeSpan timeSpan => $"'{timeSpan:d.hh:mm:ss.fffffff}'",
                bool b => b ? "1" : "0",
                Guid guid => $"'{guid:00000000-0000-0000-0000-000000000000}'",
                _ => (obj is DBNull ? "NULL" : obj.ToString())
            };
        }
    }
}
