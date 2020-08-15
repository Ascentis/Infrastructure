using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient
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

        protected override DbConnection BuildConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        protected override DbCommand BuildCommand(string sqlCommandText, DbConnection connection)
        {
            return new SqlCommand(sqlCommandText, (SqlConnection)connection);
        }
    }
}
