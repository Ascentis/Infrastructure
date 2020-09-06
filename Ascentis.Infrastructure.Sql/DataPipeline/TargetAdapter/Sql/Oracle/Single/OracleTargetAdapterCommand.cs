using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
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

        protected override void MapParams(Dictionary<string, int> paramToMetaIndex)
        {
            ParamMapper.Map(paramToMetaIndex, Source.ColumnMetadatas, AnsiStringParameters, Cmd.Parameters, false);
        }

        public override object GetNativeValue(object value)
        {
            return OracleUtils.GetNativeValue(value);
        }
    }
}
