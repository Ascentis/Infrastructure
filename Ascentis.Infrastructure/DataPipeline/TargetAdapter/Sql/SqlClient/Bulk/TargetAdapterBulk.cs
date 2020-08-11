using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk
{
    public abstract class TargetAdapterBulk : TargetAdapterSqlBulkBase<SqlCommand, SqlTransaction, SqlConnection>, ITargetAdapterBulk, ITargetAdapterSqlClient
    {
        public const int DefaultBatchSize = 100;
        // ReSharper disable once InconsistentNaming
        public const int MaxMSSQLParams = 2100;
        
        protected static readonly ColumnMetadataToSqlDbTypeMapper ParamMapper =
            new ColumnMetadataToSqlDbTypeMapper
            {
                UseShortParam = true
            };

        protected TargetAdapterBulk(IEnumerable<string> columnNames, SqlConnection sqlConnection, int batchSize) : base(columnNames, sqlConnection, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
        }
        
        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            if (ColumnNameToMetadataIndexMap.Count * BatchSize > MaxMSSQLParams)
                throw new TargetAdapterException($"Number of columns * target adapter buffer size exceeds MSSQL limit of {MaxMSSQLParams} parameters in a query");
        }
        
        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SqlCommand sqlCommand, int rowCount)
        {
            ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, AnsiStringParameters, sqlCommand.Parameters, rowCount);
        }
    }
}
