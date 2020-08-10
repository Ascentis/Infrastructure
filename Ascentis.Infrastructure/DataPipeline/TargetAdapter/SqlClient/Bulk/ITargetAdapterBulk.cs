namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Bulk
{
    public interface ITargetAdapterBulk : ITargetAdapterSql
    {
        void Flush();
    }
}
