using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.Sql.DataPipeline.SourceAdapter.Sql.SqlClient
{
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    public class SqlClientSourceAdapter : SourceAdapterSqlBase
    { 
        public SqlClientSourceAdapter(SqlDataReader sqlDataReader, int rowsPoolCapacity) : base(sqlDataReader, rowsPoolCapacity) { }
        public SqlClientSourceAdapter(SqlDataReader sqlDataReader) : base(sqlDataReader) { } 
        public SqlClientSourceAdapter(
            string connectionString,
            string sqlCommandText,
            int rowsPoolCapacity) : base(connectionString, sqlCommandText, rowsPoolCapacity) { }
        public SqlClientSourceAdapter(
            string connectionString,
            string sqlCommandText) : this(connectionString, sqlCommandText, DefaultRowsCapacity) { }

        public static DbConnection BuildConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public static DbCommand BuildCommand(string sqlCommandText, DbConnection connection)
        {
            return new SqlCommand(sqlCommandText, (SqlConnection)connection);
        }
    }
}
