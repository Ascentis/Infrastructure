using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient
{
    public class SqlClientSourceAdapter : SourceAdapterSqlBase
    {
        public SqlClientSourceAdapter(SqlDataReader sqlDataReader, int rowsPoolCapacity) : base(sqlDataReader, rowsPoolCapacity) { }

        public SqlClientSourceAdapter(SqlDataReader sqlDataReader) : base(sqlDataReader) { }
    }
}
