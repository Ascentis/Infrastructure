using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk
{
    public class SqlClientAdapterBulkCommand : 
        TargetAdapterSqlBulkBase<SqlCommand, SqlTransaction, SqlConnection>, 
        ITargetAdapterBulk, 
        ITargetAdapterSqlClient
    {
        private readonly string _sqlCommandText;

        public SqlClientAdapterBulkCommand(string sqlCommandText,
            IEnumerable<string> sourceColumnNames,
            SqlConnection conn,
            int batchSize = SqlClientUtils.DefaultBatchSize) : base(sourceColumnNames, conn, batchSize)
        {
            _sqlCommandText = sqlCommandText;
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
        }

        protected override string BuildBulkSql(int rowCount)
        {
            return BulkSqlCommandTextBuilder.BuildBulkSql(ColumnNames, _sqlCommandText, rowCount);
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

            if (ColumnNameToMetadataIndexMap.Count * BatchSize > SqlClientUtils.MaxMSSQLParams)
                throw new TargetAdapterException($"Number of columns * target adapter buffer size exceeds MSSQL limit of {SqlClientUtils.MaxMSSQLParams} parameters in a query");
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SqlCommand sqlCommand, int rowCount)
        {
            SqlClientUtils.ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, sqlCommand.Parameters, rowCount);
        }
    }
}
