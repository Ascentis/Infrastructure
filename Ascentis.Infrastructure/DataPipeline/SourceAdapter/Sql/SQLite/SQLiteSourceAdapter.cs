using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SQLite
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SQLiteSourceAdapter : SourceAdapterSqlBase
    {
        public SQLiteSourceAdapter(SQLiteDataReader sqlDataReader, int rowsPoolCapacity) : base(sqlDataReader, rowsPoolCapacity) { }
        public SQLiteSourceAdapter(SQLiteDataReader sqlDataReader) : base(sqlDataReader) { }
        public SQLiteSourceAdapter(
            string connectionString,
            string sqlCommandText,
            int rowsPoolCapacity) : base(connectionString, sqlCommandText, rowsPoolCapacity) { }
        public SQLiteSourceAdapter(
            string connectionString,
            string sqlCommandText) : this(connectionString, sqlCommandText, DefaultRowsCapacity) { }

        public static DbConnection BuildConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        public static DbCommand BuildCommand(string sqlCommandText, DbConnection connection)
        {
            return new SQLiteCommand(sqlCommandText, (SQLiteConnection)connection);
        }
    }
}
