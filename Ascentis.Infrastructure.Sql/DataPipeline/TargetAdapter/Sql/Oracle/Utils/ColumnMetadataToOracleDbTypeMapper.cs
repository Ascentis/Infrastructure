using System;
using System.Data;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Utils
{
    // ReSharper disable once InconsistentNaming
    public class ColumnMetadataToOracleDbTypeMapper : ColumnMetadataToDbTypeMapper
    {
        protected override int SqlTypeFromType(Type type)
        {
            return (int)TypeToOracleDbType.From(type);
        }

        protected override DbParameter AddParam(DbParameterCollection target, string name, int type)
        {
            return ((OracleParameterCollection) target).Add(name, (OracleDbType)type, ParameterDirection.Input);
        }
    }
}
