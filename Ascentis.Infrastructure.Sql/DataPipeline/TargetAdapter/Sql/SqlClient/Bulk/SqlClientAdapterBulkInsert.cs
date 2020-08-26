using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk
{
    public class SqlClientAdapterBulkInsert : 
        TargetAdapterBulkInsertBase<SqlCommand, SqlTransaction, SqlConnection, SqlException>, 
        ITargetAdapterFlushable, 
        ITargetAdapterSqlClient
    {
        public SqlClientAdapterBulkInsert(string tableName,
            IEnumerable<string> columnNames,
            SqlConnection conn,
            int batchSize = SqlClientUtils.DefaultBatchSize) : base(tableName, columnNames, conn, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SqlCommand sqlCommand, int rowCount)
        {
            SqlClientUtils.ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, sqlCommand.Parameters, rowCount);
        }

        public override string ValueToSqlLiteralText(object obj)
        {
            return SqlClientUtils.ValueToSqlLiteralText(obj);
        }

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            if (BatchSize == MaxPossibleBatchSize)
                BatchSize = SqlClientUtils.MaxMSSQLParams / ColumnNameToMetadataIndexMap.Count;

            if (!LiteralParamBinding && ColumnNameToMetadataIndexMap.Count * BatchSize > SqlClientUtils.MaxMSSQLParams)
                throw new TargetAdapterException(
                    $"Number of columns in target adapter buffer size ({ColumnNameToMetadataIndexMap.Count * BatchSize}) exceeds MSSQL limit of {SqlClientUtils.MaxMSSQLParams} parameters in a query");
            if (LiteralParamBinding && BatchSize > SqlClientUtils.MaxMSSQLInsertRows)
                throw new TargetAdapterException("BatchSize can't be greater than 1000, not supported by MSSQL");
        }
    }
}
