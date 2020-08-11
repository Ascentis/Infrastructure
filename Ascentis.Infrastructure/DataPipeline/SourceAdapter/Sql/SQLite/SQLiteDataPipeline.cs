using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteDataPipeline : SqlDataPipeline<SQLiteDataReader, SQLiteCommand, SQLiteSourceAdapter> { }
}
