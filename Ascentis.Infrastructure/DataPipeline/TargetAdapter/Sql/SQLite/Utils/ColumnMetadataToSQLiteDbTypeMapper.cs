using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Utils
{
    // ReSharper disable once InconsistentNaming
    public class ColumnMetadataToSQLiteDbTypeMapper : ColumnMetadataToDbTypeMapper
    {
        protected override int SqlTypeFromType(Type type)
        {
            return (int)TypeToSQLiteDbType.From(type);
        }

        protected override DbParameter AddParam(DbParameterCollection target, string name, int type)
        {
            return ((SQLiteParameterCollection) target).Add(name, (DbType)type);
        }
    }
}
