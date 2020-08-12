using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient
{
    public class SqlClientDataPipeline : SqlDataPipeline<SqlCommand, SqlClientSourceAdapter> { }
}
