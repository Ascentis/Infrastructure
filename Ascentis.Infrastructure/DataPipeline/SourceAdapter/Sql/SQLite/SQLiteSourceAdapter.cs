using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteSourceAdapter : SourceAdapterSqlBase<SQLiteDataReader>
    {
        public SQLiteSourceAdapter(SQLiteDataReader sqlDataReader, int rowsPoolCapacity) : base(sqlDataReader, rowsPoolCapacity) { }

        public SQLiteSourceAdapter(SQLiteDataReader sqlDataReader) : base(sqlDataReader) { }
    }
}
