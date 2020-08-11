using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Utils
{
    public class ColumnMetadataToSqlDbTypeMapper : ColumnMetadataToDbTypeMapper
    {
        protected override int SqlTypeFromType(Type type)
        {
            return (int) TypeToSqlDbType.From(type);
        }

        protected override DbParameter AddParam(DbParameterCollection target, string name, int type)
        {
            return ((SqlParameterCollection) target).Add(name, (SqlDbType)type);
        }
    }
}
