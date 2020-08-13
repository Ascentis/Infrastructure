using System;
using System.Data;
using System.Data.SQLite;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    // ReSharper disable once InconsistentNaming
    public static class TypeToSQLiteDbType
    {
        public static DbType From(Type sourceType)
        {
            static DbType GetDbType(Type type) => new SQLiteParameter("Test", type.IsValueType ? Activator.CreateInstance(type) : null).DbType;
            return GetDbType(sourceType);
        }

        public static DbType From(object value)
        {
            return From(value.GetType());
        }
    }
}
