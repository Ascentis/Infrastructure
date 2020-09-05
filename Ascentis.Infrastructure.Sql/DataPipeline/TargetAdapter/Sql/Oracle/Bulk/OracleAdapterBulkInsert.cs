using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Utils;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Bulk
{
    public class OracleAdapterBulkInsert : 
        TargetAdapterBulkInsertBase<OracleCommand, OracleTransaction, OracleConnection, OracleException>, 
        ITargetAdapterFlushable, 
        ITargetAdapterOracle
    {
        private OracleArrayBindingHelper _oracleArrayBindingHelper;

        public OracleAdapterBulkInsert(string tableName,
            IEnumerable<string> columnNames,
            OracleConnection conn,
            int batchSize = OracleUtils.DefaultBatchSize) : base(tableName, columnNames, conn, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref OracleCommand sqlCommand, int rowCount)
        {
            OracleUtils.ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, sqlCommand.Parameters, LiteralParamBinding ? rowCount : 1);
        }

        public override string ValueToSqlLiteralText(object obj)
        {
            return OracleUtils.ValueToSqlLiteralText(obj);
        }

        protected override void ConfigureBulkCommandBuilder(BulkSqlCommandTextBuilder cmdBuilder)
        {
            cmdBuilder.SingleParamSetInsertStatement = !LiteralParamBinding;
            cmdBuilder.ParamIndicator = ':';
        }

        public override void BindParameters()
        {
            if (LiteralParamBinding)
            {
                base.BindParameters();
                return;
            }

            if (Cmd == null)
                BuildSqlCommand(Rows.Count, ref Cmd);

            _oracleArrayBindingHelper.BindParameters(Cmd, Rows);
        }

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            if (BatchSize == MaxPossibleBatchSize)
                BatchSize = OracleUtils.DefaultMaxOracleParams / ColumnNameToMetadataIndexMap.Count;

            if (!LiteralParamBinding && ColumnNameToMetadataIndexMap.Count * BatchSize > OracleUtils.DefaultMaxOracleParams)
                throw new TargetAdapterException($"Number of columns in target adapter buffer size ({ColumnNameToMetadataIndexMap.Count * BatchSize}) exceeds ORACLE limit of {OracleUtils.DefaultMaxOracleParams} parameters in a query");
            
            if (LiteralParamBinding)
                return;
            _oracleArrayBindingHelper = new OracleArrayBindingHelper(BatchSize, ColumnNameToMetadataIndexMap, SourceValueToParamValue);
        }

        public override void UnPrepare()
        {
            base.UnPrepare();
            _oracleArrayBindingHelper = null;
        }
    }
}
