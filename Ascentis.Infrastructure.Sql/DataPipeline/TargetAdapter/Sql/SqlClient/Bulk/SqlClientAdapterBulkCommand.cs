﻿using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;
using Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SqlClient.Utils;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk
{
    public class SqlClientAdapterBulkCommand : 
        TargetAdapterSqlBulkBase<SqlCommand, SqlTransaction, SqlConnection>, 
        ITargetAdapterFlushable, 
        ITargetAdapterSqlClient
    {
        public SqlClientAdapterBulkCommand(string sqlCommandText,
            IEnumerable<string> sourceColumnNames,
            SqlConnection conn,
            int batchSize = SqlClientUtils.DefaultBatchSize) : base(sqlCommandText, sourceColumnNames, conn, batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
        }

        public override string ValueToSqlLiteralText(object obj)
        {
            return SqlClientUtils.ValueToSqlLiteralText(obj);
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
                BatchSize = SqlClientUtils.MaxMSSQLParams / ColumnNameToMetadataIndexMap.Count;

            if (ColumnNameToMetadataIndexMap.Count * BatchSize > SqlClientUtils.MaxMSSQLParams)
                throw new TargetAdapterException(
                    $"Number of columns in target adapter buffer size ({ColumnNameToMetadataIndexMap.Count * BatchSize}) exceeds MSSQL limit of {SqlClientUtils.MaxMSSQLParams} parameters in a query");
        }

        protected override void MapParams(IDictionary<string, int> paramToMetaIndex, ref SqlCommand sqlCommand, int rowCount)
        {
            var metaToParamSettings = new MetaToParamSettings
            {
                Columns = ColumnNames,
                Metadatas = Source.ColumnMetadatas,
                AnsiStringParameters = AnsiStringParameters,
                ColumnToIndexMap = paramToMetaIndex,
                Target = sqlCommand.Parameters,
                UseShortParam = true
            };
            SqlClientUtils.ParamMapper.Map(metaToParamSettings, rowCount);
        }
    }
}
