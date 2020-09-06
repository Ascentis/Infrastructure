using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    // ReSharper disable once InconsistentNaming
    public static class TypeToOracleDbType
    {
        public static DbType From(Type sourceType)
        {
            static OracleDbType GetDbType(Type type) => new OracleParameter("Test", type.IsValueType ? Activator.CreateInstance(type) : null).OracleDbType;
            return (DbType)GetDbType(sourceType != typeof(bool) ? sourceType : typeof(short));
        }

        public static DbType From(object value)
        {
            return From(value.GetType());
        }
    }
}
