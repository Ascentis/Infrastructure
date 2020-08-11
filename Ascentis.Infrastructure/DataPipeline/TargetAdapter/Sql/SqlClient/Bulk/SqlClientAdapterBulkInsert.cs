using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk
{
    public class SqlClientAdapterBulkInsert : 
        TargetAdapterBulkInsertBase<SqlCommand, SqlTransaction, SqlConnection, SqlException>, 
        ITargetAdapterBulk, 
        ITargetAdapterSqlClient
    {
        public SqlClientAdapterBulkInsert(string tableName,
            IEnumerable<string> columnNames,
            SqlConnection sqlConnection,
            int batchSize = SqlClientUtils.DefaultBatchSize) : base(tableName, columnNames, sqlConnection, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SqlCommand sqlCommand, int rowCount)
        {
            SqlClientUtils.ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, sqlCommand.Parameters, rowCount);
        }

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            if (ColumnNameToMetadataIndexMap.Count * BatchSize > SqlClientUtils.MaxMSSQLParams)
                throw new TargetAdapterException($"Number of columns * target adapter buffer size exceeds MSSQL limit of {SqlClientUtils.MaxMSSQLParams} parameters in a query");
        }
    }
}
