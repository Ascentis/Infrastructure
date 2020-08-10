using System;
using System.Data;
using System.Data.SqlClient;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class TypeToSqlDbType
    {
        public static SqlDbType From(Type sourceType)
        {
            static SqlDbType GetSqlType(Type type) => new SqlParameter("Test", type.IsValueType ? Activator.CreateInstance(type) : null).SqlDbType;
            return GetSqlType(sourceType);
        }

        public static SqlDbType From(object value)
        {
            return From(value.GetType());
        }
    }
}
