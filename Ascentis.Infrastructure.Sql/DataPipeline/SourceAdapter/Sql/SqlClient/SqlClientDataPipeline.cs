﻿using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.Sql.DataPipeline.SourceAdapter.Sql.SqlClient
{
    public class SqlClientDataPipeline : SqlDataPipeline<SqlCommand, SqlConnection, SqlClientSourceAdapter> { }
}
