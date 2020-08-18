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
            return _BuildConnection(connectionString);
        }

        protected override  DbCommand BuildCommand(string sqlCommandText, DbConnection connection)
        {
            return _BuildCommand(sqlCommandText, (SqlConnection)connection);
        }

        #region Methods used dynamically from DataReplicator to build connection and commands
        private static DbConnection _BuildConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        private static DbCommand _BuildCommand(string sqlCommandText, DbConnection connection)
        {
            return new SqlCommand(sqlCommandText, (SqlConnection)connection);
        }
        #endregion
    }
}
