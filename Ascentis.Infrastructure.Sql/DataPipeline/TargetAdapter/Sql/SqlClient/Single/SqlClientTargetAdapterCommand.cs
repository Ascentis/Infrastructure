using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Single
{
    public class SqlClientTargetAdapterCommand : TargetAdapterSqlBase<SqlCommand, SqlTransaction, SqlConnection> , ITargetAdapterSqlClient
    {
        private static readonly ColumnMetadataToSqlDbTypeMapper ParamMapper = new ColumnMetadataToSqlDbTypeMapper();

        public SqlClientTargetAdapterCommand(SqlCommand cmd) : base(cmd) {}
        
        protected override IList<string> ParseParameters()
        {
            return Cmd.ParseParameters();
        }

        protected override void MapParams(Dictionary<string, int> paramToMetaIndex)
        {
            ParamMapper.Map(paramToMetaIndex, Source.ColumnMetadatas, AnsiStringParameters, Cmd.Parameters);
        }
    }
}
