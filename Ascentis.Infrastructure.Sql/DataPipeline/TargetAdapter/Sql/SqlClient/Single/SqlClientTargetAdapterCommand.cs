using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SqlClient.Utils;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SqlClient.Single
{
    public class SqlClientTargetAdapterCommand : TargetAdapterSqlBase<SqlCommand, SqlTransaction, SqlConnection> , ITargetAdapterSqlClient
    {
        private static readonly ColumnMetadataToSqlDbTypeMapper ParamMapper = new ColumnMetadataToSqlDbTypeMapper();

        public SqlClientTargetAdapterCommand(SqlCommand cmd) : base(cmd) {}
        
        protected override IList<string> ParseParameters()
        {
            return Cmd.ParseParameters();
        }

        protected override void MapParams(IEnumerable<string> columnNames, Dictionary<string, int> paramToMetaIndex)
        {
            var metaToParamSettings = new MetaToParamSettings
            {
                Columns = columnNames,
                Metadatas = Source.ColumnMetadatas,
                AnsiStringParameters = AnsiStringParameters,
                ColumnToIndexMap = paramToMetaIndex,
                Target = Cmd.Parameters,
                UseShortParam = false
            };
            ParamMapper.Map(metaToParamSettings);
        }
    }
}
