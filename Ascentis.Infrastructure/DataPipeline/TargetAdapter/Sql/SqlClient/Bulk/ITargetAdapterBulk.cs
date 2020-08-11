namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk
{
    public interface ITargetAdapterBulk : ITargetAdapterSqlClient
    {
        void Flush();
    }
}
