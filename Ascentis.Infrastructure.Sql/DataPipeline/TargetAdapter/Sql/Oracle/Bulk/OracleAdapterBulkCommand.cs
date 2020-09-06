using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Utils;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Bulk
{
    public class OracleAdapterBulkCommand : 
        TargetAdapterSqlBulkBase<OracleCommand, OracleTransaction, OracleConnection>, 
        ITargetAdapterFlushable, 
        ITargetAdapterOracle
    {
        public bool UseArrayBinding { get; set; }

        private OracleArrayBindingHelper _oracleArrayBindingHelper;
        
        public OracleAdapterBulkCommand(string sqlCommandText,
            IEnumerable<string> sourceColumnNames,
            OracleConnection conn,
            int batchSize = OracleUtils.DefaultBatchSize) : base(sqlCommandText, sourceColumnNames, conn, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
        }

        protected override void ConfigureBulkCommandBuilder(BulkSqlCommandTextBuilder cmdBuilder)
        {
            cmdBuilder.ParamIndicator = ':';
            cmdBuilder.InternalSelectSuffix = "FROM DUAL";
        }

        protected override string BuildBulkSql(List<PoolEntry<object[]>> rows)
        {
            if (!UseArrayBinding)
                return base.BuildBulkSql(rows);

            var parameters = string.Join(",:", ColumnNames);
            parameters = ":" + parameters;
            var newSqlCommandText = Regex.Replace(SqlCommandText,
                BulkSqlCommandTextBuilder.ParamsReplacementRegEx,
                parameters,
                RegexOptions.Compiled | RegexOptions.Singleline);

            return newSqlCommandText;
        }

        public override string ValueToSqlLiteralText(object obj)
        {
            return OracleUtils.ValueToSqlLiteralText(obj);
        }

        public override void Flush()
        {
            try
            {
                InternalFlush();
            }
            finally
            {
                InternalReleaseRows();
            }
        }

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            if (BatchSize == MaxPossibleBatchSize)
                BatchSize = OracleUtils.DefaultMaxOracleParams / ColumnNameToMetadataIndexMap.Count;

            if (ColumnNameToMetadataIndexMap.Count * BatchSize > OracleUtils.DefaultMaxOracleParams)
                throw new TargetAdapterException(
                    $"Number of columns in target adapter buffer size ({ColumnNameToMetadataIndexMap.Count * BatchSize}) exceeds ORACLE limit of {OracleUtils.DefaultMaxOracleParams} parameters in a query");
            if (!UseArrayBinding)
                return;
            _oracleArrayBindingHelper = new OracleArrayBindingHelper(BatchSize, ColumnNameToMetadataIndexMap, SourceValueToParamValue) {UseNativeTypeConvertor = UseNativeTypeConvertor};
        }

        public override void UnPrepare()
        {
            base.UnPrepare();
            _oracleArrayBindingHelper = null;
        }

        public override void BindParameters()
        {
            if (!UseArrayBinding)
            {
                base.BindParameters();
                return;
            }

            if (Cmd == null)
                BuildSqlCommand(Rows.Count, ref Cmd);

            _oracleArrayBindingHelper.BindParameters(Cmd, Rows);
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref OracleCommand sqlCommand, int rowCount)
        {
            OracleUtils.ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, 
                sqlCommand.Parameters, !UseArrayBinding ? rowCount : 1, !UseArrayBinding, !UseArrayBinding);
        }

        public override object GetNativeValue(object value)
        {
            return OracleUtils.GetNativeValue(value);
        }
    }
}
