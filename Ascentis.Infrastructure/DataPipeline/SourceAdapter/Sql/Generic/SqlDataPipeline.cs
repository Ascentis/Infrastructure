using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic
{
    public abstract class SqlDataPipeline<TReader, TCmd, TAdapter> : DataPipeline<PoolEntry<object[]>> 
        where TReader : DbDataReader 
        where TCmd : DbCommand
        where TAdapter : SourceAdapterSqlBase<TReader>
    {
        public void Pump(TReader source,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSqlBase<TReader>.DefaultRowsCapacity)
        {
            var adapter = GenericObjectBuilder.Build<TAdapter>(source, rowsPoolCapacity);
            Pump(adapter, targetAdapter);
        }

        public void Pump(TCmd source,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSqlBase<TReader>.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump((TReader)reader, targetAdapter, rowsPoolCapacity);
        }

        public void Pump(TReader source,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SqlClientSourceAdapter.DefaultRowsCapacity)
        {
            var adapter = GenericObjectBuilder.Build<TAdapter>(source, rowsPoolCapacity);
            Pump(adapter, dataPipelineTargetAdapters);
        }

        public void Pump(TCmd source,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SqlClientSourceAdapter.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump((TReader)reader, dataPipelineTargetAdapters, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SqlClientSourceAdapter.DefaultRowsCapacity)
        {
            using var cmd = GenericObjectBuilder.Build<TCmd>(sourceSql, sourceConnection);
            Pump(cmd, targetAdapter, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SqlClientSourceAdapter.DefaultRowsCapacity)
        {
            using var cmd = GenericObjectBuilder.Build<TCmd>(sourceSql, sourceConnection);
            Pump(cmd, dataPipelineTargetAdapters, rowsPoolCapacity);
        }
    }
}
