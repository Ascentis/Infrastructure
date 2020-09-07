using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Utils;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Single
{
    public class OracleTargetAdapterCommand : TargetAdapterSqlBase<OracleCommand, OracleTransaction, OracleConnection> , ITargetAdapterOracle
    {
        private static readonly ColumnMetadataToOracleDbTypeMapper ParamMapper = new ColumnMetadataToOracleDbTypeMapper();

        public OracleTargetAdapterCommand(OracleCommand cmd) : base(cmd) {}
        
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

        public override object GetNativeValue(object value)
        {
            return OracleUtils.GetNativeValue(value);
        }
    }
}
