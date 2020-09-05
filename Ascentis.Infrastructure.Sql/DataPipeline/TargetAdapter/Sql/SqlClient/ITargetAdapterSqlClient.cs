using System.Data.SqlClient;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SqlClient
{
    public interface ITargetAdapterSqlClient
    {
        SqlTransaction Transaction { get; set; }
        SqlConnection Connection { get; }
    }
}
