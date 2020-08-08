using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient
{
    public class DataPipelineSql : DataPipeline<PoolEntry<object[]>>
    {
        public void Pump(SqlDataReader source,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSql.DefaultRowsCapacity)
        {
            var adapter = new SourceAdapterSql(source, rowsPoolCapacity);
            Pump(adapter, targetAdapter);
        }

        public void Pump(SqlCommand source,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSql.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump(reader, targetAdapter, rowsPoolCapacity);
        }

        public void Pump(SqlDataReader source,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SourceAdapterSql.DefaultRowsCapacity)
        {
            var adapter = new SourceAdapterSql(source, rowsPoolCapacity);
            Pump(adapter, dataPipelineTargetAdapters);
        }

        public void Pump(SqlCommand source,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SourceAdapterSql.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump(reader, dataPipelineTargetAdapters, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSql.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            Pump(sqlCommand, targetAdapter, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SourceAdapterSql.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            Pump(sqlCommand, dataPipelineTargetAdapters, rowsPoolCapacity);
        }
    }
}
