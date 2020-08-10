using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient
{
    public interface ITargetAdapterSql
    {
        SqlTransaction Transaction { get; set; }
        SqlConnection Connection { get; }
    }
}
