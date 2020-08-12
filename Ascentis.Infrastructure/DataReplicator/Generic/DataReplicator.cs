using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;
using Ascentis.Infrastructure.DataPipeline;

namespace Ascentis.Infrastructure.DataReplicator.Generic
{
    [SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
    public abstract class DataReplicator<TTargetCmd, TTargetConn> : IDisposable
        where TTargetCmd : DbCommand
        where TTargetConn : DbConnection
    {
        public const int DefaultParallelismLevel = 2;

        private bool _prepared;
        private readonly string _sourceConnStr;
        private readonly string _targetConnStr;
        private readonly List<Tuple<string, string>> _sourceTables;
        private List<DbCommand> _sourceCmds;
        private List<DbDataReader> _readers;
        private DbConnection[] _sourceConnections;
        private TTargetConn[] _targetConnections;
        private BoundedParallel _parallelRunner;

        public int ParallelismLevel { get; set; }
        public ColumnMetadataList[] ColumnMetadataLists { get; private set; }

        public bool UseTransaction { get; set; }

        protected DataReplicator(string sourceConnStr, string targetConnStr, int parallelismLevel = DefaultParallelismLevel)
        {
            _sourceConnStr = sourceConnStr;
            _targetConnStr = targetConnStr;
            _sourceTables = new List<Tuple<string, string>>();
            ParallelismLevel = parallelismLevel;
        }

        public void AddSourceTable(string tableName, string sqlStatement)
        {
            _sourceTables.Add(new Tuple<string, string>(tableName, sqlStatement));
        }

        public void Prepare<TSrcCmd, TSrcConn>() 
            where TSrcCmd : DbCommand
            where TSrcConn : DbConnection
        {
            if (_prepared)
                throw new InvalidOperationException("DataReplicator already prepared.");
            _sourceCmds = new List<DbCommand>();
            foreach (var sqlStatement in _sourceTables)
                _sourceCmds.Add(GenericObjectBuilder.Build<TSrcCmd>(sqlStatement.Item2));
            
            _sourceConnections = new DbConnection[_sourceCmds.Count];
            _targetConnections = new TTargetConn[_sourceCmds.Count];
            ColumnMetadataLists = new ColumnMetadataList[_sourceCmds.Count];
            _readers = new List<DbDataReader>();
            
            _parallelRunner = new BoundedParallel(1, ParallelismLevel);
            _parallelRunner.For(0, _sourceCmds.Count, i =>
            {
                _sourceConnections[i] = GenericObjectBuilder.Build<TSrcConn>(_sourceConnStr);
                _sourceConnections[i].Open();

                _sourceCmds[i].Connection = _sourceConnections[i];
                var reader = _sourceCmds[i].ExecuteReader();
                _readers.Add(reader);
                
                ColumnMetadataLists[i] = new ColumnMetadataList(reader);

                _targetConnections[i] = GenericObjectBuilder.Build<TTargetConn>(_targetConnStr);
                _targetConnections[i].Open();
            });
            _prepared = true;
        }

        protected abstract string BuildDropTableStatement(string tableName);

        protected abstract string BuildCreateTableStatement(string tableName, ColumnMetadataList metadatas);

        private void CreateTable(int tableNumber)
        {
            var tableDef = ColumnMetadataLists[tableNumber];
            if (tableDef.Count <= 0)
                throw new InvalidOperationException("Found table definition with no columns");

            var tableName = _sourceTables[tableNumber].Item1;

            var dropTableStatement = BuildDropTableStatement(tableName);
            using var dropTableCmd = GenericObjectBuilder.Build<TTargetCmd>(dropTableStatement, _targetConnections[tableNumber]);
            dropTableCmd.ExecuteNonQuery();

            var createTableStatement = BuildCreateTableStatement(tableName, tableDef);
            using var createTableCmd = GenericObjectBuilder.Build<TTargetCmd>(createTableStatement, _targetConnections[tableNumber]);
            createTableCmd.ExecuteNonQuery();
        }

        protected abstract TargetAdapterSql BuildTargetAdapter(
            string tableName,
            IEnumerable<string> columnNames,
            TTargetConn conn,
            int batchSize);

        protected abstract DataPipeline<PoolEntry<object[]>> BuildDataPipeline();

        protected abstract void ConfigureTargetConnection(TTargetConn connection, int columnCount, int batchSize);
        
        public void Replicate<TSourceAdapter>(int readBufferSize, int insertBatchSize) 
            where TSourceAdapter : SourceAdapterSqlBase
        {
            if (!_prepared)
                throw new InvalidOperationException("DataReplicator not prepared. Can't run.");
            _parallelRunner.For(0, _readers.Count, i =>
            {
                CreateTable(i);

                var columnNames = new List<string>();
                foreach (var meta in ColumnMetadataLists[i])
                    columnNames.Add(meta.ColumnName);

                var sourceAdapter = GenericObjectBuilder.Build<TSourceAdapter>(_readers[i], readBufferSize);
                sourceAdapter.AbortOnReadException = true;
                
                var targetAdapter = BuildTargetAdapter(_sourceTables[i].Item1, columnNames, _targetConnections[i], insertBatchSize);
                targetAdapter.AbortOnProcessException = true;
                
                var pipeline = BuildDataPipeline();
                
                ConfigureTargetConnection(_targetConnections[i], columnNames.Count, insertBatchSize);
                
                var tran = UseTransaction ? _targetConnections[i].BeginTransaction() : null;
                try
                {
                    pipeline.Pump(sourceAdapter, targetAdapter);
                    tran?.Commit();
                }
                catch (Exception)
                {
                    tran?.Rollback();
                    throw;
                }
            });
        }

        public void UnPrepare()
        {
            if (!_prepared)
                throw new InvalidOperationException("DataReplicator not prepared. Can't UnPrepare.");
            Dispose();
            _parallelRunner = null;
            ColumnMetadataLists = null;
            _prepared = false;
        }

        public virtual void Dispose()
        {
            if (_sourceCmds != null)
                foreach(var cmd in _sourceCmds)
                    cmd.Dispose();
            _sourceCmds = null;
            if (_sourceConnections != null)
                foreach(var conn in _sourceConnections)
                    conn.Dispose();
            _sourceConnections = null;
        }
    }
}
