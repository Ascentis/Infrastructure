using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic
{
    public abstract class SqlDataPipeline<TCmd, TAdapter> : DataPipeline<PoolEntry<object[]>> 
        where TCmd : DbCommand
        where TAdapter : SourceAdapterSqlBase
    {
        public void Pump(DbDataReader source,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSqlBase.DefaultRowsCapacity)
        {
            var adapter = GenericObjectBuilder.Build<TAdapter>(source, rowsPoolCapacity);
            Pump(adapter, targetAdapter);
        }

        public void Pump(TCmd source,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSqlBase.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump(reader, targetAdapter, rowsPoolCapacity);
        }

        public void Pump(DbDataReader source,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SourceAdapterSqlBase.DefaultRowsCapacity)
        {
            var adapter = GenericObjectBuilder.Build<TAdapter>(source, rowsPoolCapacity);
            Pump(adapter, dataPipelineTargetAdapters);
        }

        public void Pump(TCmd source,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SourceAdapterSqlBase.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump(reader, dataPipelineTargetAdapters, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            ITargetAdapter<PoolEntry<object[]>> targetAdapter,
            int rowsPoolCapacity = SourceAdapterSqlBase.DefaultRowsCapacity)
        {
            using var cmd = GenericObjectBuilder.Build<TCmd>(sourceSql, sourceConnection);
            Pump(cmd, targetAdapter, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            IEnumerable<ITargetAdapter<PoolEntry<object[]>>> dataPipelineTargetAdapters,
            int rowsPoolCapacity = SourceAdapterSqlBase.DefaultRowsCapacity)
        {
            using var cmd = GenericObjectBuilder.Build<TCmd>(sourceSql, sourceConnection);
            Pump(cmd, dataPipelineTargetAdapters, rowsPoolCapacity);
        }
    }
}
