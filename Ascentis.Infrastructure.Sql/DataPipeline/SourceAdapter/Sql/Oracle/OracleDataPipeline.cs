using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.SourceAdapter.Sql.Oracle
{
    public class OracleDataPipeline : SqlDataPipeline<OracleCommand, OracleConnection, OracleSourceAdapter> { }
}
