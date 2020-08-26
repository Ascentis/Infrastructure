using System.Data.SQLite;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite
{
    // ReSharper disable once InconsistentNaming
    public interface ITargetAdapterSQLite
    {
        SQLiteTransaction Transaction { get; set; }
        SQLiteConnection Connection { get; }
    }
}
