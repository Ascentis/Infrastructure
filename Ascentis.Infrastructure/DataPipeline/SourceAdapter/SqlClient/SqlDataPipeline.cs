using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient
{
    public class SqlDataPipeline : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(SqlDataReader source,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineSourceAdapterSql.DefaultRowsCapacity)
        {
            var adapter = new DataPipelineSourceAdapterSql(source, rowsPoolCapacity);
            Pump(adapter, dataPipelineTargetAdapter);
        }

        public void Pump(SqlCommand source,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineSourceAdapterSql.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump(reader, dataPipelineTargetAdapter, rowsPoolCapacity);
        }

        public void Pump(SqlDataReader source,
            IEnumerable<IDataPipelineTargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = DataPipelineSourceAdapterSql.DefaultRowsCapacity)
        {
            var adapter = new DataPipelineSourceAdapterSql(source, rowsPoolCapacity);
            Pump(adapter, dataPipelineTargetAdapters);
        }

        public void Pump(SqlCommand source,
            IEnumerable<IDataPipelineTargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = DataPipelineSourceAdapterSql.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump(reader, dataPipelineTargetAdapters, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            IDataPipelineTargetAdapter<PoolEntry<object[]>> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineSourceAdapterSql.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            Pump(sqlCommand, dataPipelineTargetAdapter, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            IEnumerable<IDataPipelineTargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = DataPipelineSourceAdapterSql.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            Pump(sqlCommand, dataPipelineTargetAdapters, rowsPoolCapacity);
        }
    }
}
