using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle
{
    public interface ITargetAdapterOracle
    {
        OracleTransaction Transaction { get; set; }
        OracleConnection Connection { get; }
    }
}
