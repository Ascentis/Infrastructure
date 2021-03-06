﻿using System;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Utils
{
    // ReSharper disable once InconsistentNaming
    public class OracleUtils
    {
        public const int DefaultBatchSize = 100;
        // ReSharper disable once InconsistentNaming
        public const int DefaultMaxOracleParams = 65536;

        public static readonly ColumnMetadataToOracleDbTypeMapper ParamMapper = new ColumnMetadataToOracleDbTypeMapper();

        public static object GetNativeValue(object value)
        {
            return value switch
            {
                bool b => b ? 1 : 0,
                Guid guid => guid.ToString("00000000-0000-0000-0000-000000000000"),
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
