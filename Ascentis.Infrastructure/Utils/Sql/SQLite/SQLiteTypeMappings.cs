using System;
using System.Data;
using System.Data.SQLite;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteTypeMappings
    {
        private static bool _initialized;

        public static void InitAutoMapper()
        {
            if (_initialized)
                return;
            SQLiteConnection.Changed += (sender, args) =>
            {
                if (args.EventType != SQLiteConnectionEventType.Opened)
                    return;
                if (sender is SQLiteConnection connection)
                    AddTypeMappings(connection);
            };
            _initialized = true;
        }

        private static readonly Tuple<string, DbType>[] TypeMappings =
        {
            new Tuple<string, DbType>("int_bit", DbType.Boolean),
            new Tuple<string, DbType>("text_decimal", DbType.Decimal),
            new Tuple<string, DbType>("tinyint", DbType.Byte),
            new Tuple<string, DbType>("blob_binary", DbType.Binary),
            new Tuple<string, DbType>("datetime", DbType.DateTime),
            new Tuple<string, DbType>("datetimeoffset", DbType.DateTimeOffset),
            new Tuple<string, DbType>("float", DbType.Single),
            new Tuple<string, DbType>("double", DbType.Double),
            new Tuple<string, DbType>("text_uniqueidentifier", DbType.Guid),
            new Tuple<string, DbType>("smallint", DbType.Int16),
            new Tuple<string, DbType>("int", DbType.Int32),
            new Tuple<string, DbType>("bigint", DbType.Int64),
            new Tuple<string, DbType>("nvarchar", DbType.String),
            new Tuple<string, DbType>("nchar", DbType.StringFixedLength),
            new Tuple<string, DbType>("time", DbType.Time)
        };

        private static void AddTypeMappings(SQLiteConnection connection)
        {
            foreach (var mapping in TypeMappings)
                if (connection.AddTypeMapping(mapping.Item1, mapping.Item2, true) > 0)
                    break;
        }
    }
}
