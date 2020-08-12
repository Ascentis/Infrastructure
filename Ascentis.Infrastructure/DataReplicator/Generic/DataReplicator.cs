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
        private readonly List<Tuple<string, string, IEnumerable<string>>> _sourceTables;
        private List<DbCommand> _sourceCmds;
        private List<DbDataReader> _readers;
        private DbConnection[] _sourceConnections;
        private TTargetConn[] _targetConnections;
        private BoundedParallel _parallelRunner;

        public int ParallelismLevel { get; set; }
        public ColumnMetadataList[] ColumnMetadataLists { get; private set; }
        public bool UseTransaction { get; set; }
        public bool ForceDropTable { get; set; }
        public bool UseNativeTypeConvertor { get; set; }

        public DbCommand GetSourceCommand(int index)
        {
            return _sourceCmds[index];
        }

        public int SourceCommandCount => _sourceCmds?.Count ?? 0;

        protected DataReplicator(string sourceConnStr, string targetConnStr, int parallelismLevel = DefaultParallelismLevel)
        {
            _sourceConnStr = sourceConnStr;
            _targetConnStr = targetConnStr;
            _sourceTables = new List<Tuple<string, string, IEnumerable<string>>>();
            ParallelismLevel = parallelismLevel;
        }

        public void AddSourceTable(string tableName, string sqlStatement, IEnumerable<string> customCommands = null)
        {
            _sourceTables.Add(new Tuple<string, string, IEnumerable<string>>(tableName, sqlStatement, customCommands));
        }

        public virtual void Prepare<TSrcCmd, TSrcConn>() 
            where TSrcCmd : DbCommand
            where TSrcConn : DbConnection
        {
            if (_prepared)
                throw new InvalidOperationException("DataReplicator already prepared.");
            try
            {
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
            catch (Exception)
            {
                UnPrepare(true);
                throw;
            }
        }

        protected abstract string BuildDropTableStatement(string tableName);
        protected abstract string BuildCreateTableStatement(string tableName, ColumnMetadataList metadatas);
        protected abstract bool TableExists(string tableName, DbConnection connection);
        protected abstract string BuildTruncateTableStatement(string tableName);

        private void EnsureTableCreated(int tableNumber)
        {
            var tableDef = ColumnMetadataLists[tableNumber];
            if (tableDef.Count <= 0)
                throw new InvalidOperationException("Found table definition with no columns");
            
            var tableName = _sourceTables[tableNumber].Item1;
            var customCommands = _sourceTables[tableNumber].Item3;

            if (ForceDropTable || !TableExists(tableName, _targetConnections[tableNumber]))
            {
                var dropTableStatement = BuildDropTableStatement(tableName);
                using var dropTableCmd =
                    GenericObjectBuilder.Build<TTargetCmd>(dropTableStatement, _targetConnections[tableNumber]);
                dropTableCmd.ExecuteNonQuery();

                var createTableStatement = BuildCreateTableStatement(tableName, tableDef);
                using var createTableCmd = GenericObjectBuilder.Build<TTargetCmd>(createTableStatement, _targetConnections[tableNumber]);
                createTableCmd.ExecuteNonQuery();

                if (customCommands == null) 
                    return;

                foreach (var customCommandStatement in customCommands)
                {
                    using var customCommand = GenericObjectBuilder.Build<TTargetCmd>(customCommandStatement, _targetConnections[tableNumber]);
                    customCommand.ExecuteNonQuery();
                }
            }
            else
            {
                using var truncateTableCmd = GenericObjectBuilder.Build<TTargetCmd>(BuildTruncateTableStatement(tableName), _targetConnections[tableNumber]);
                truncateTableCmd.ExecuteNonQuery();
            }
        }

        protected abstract TargetAdapterSql BuildTargetAdapter(string tableName, IEnumerable<string> columnNames, TTargetConn conn, int batchSize);
        protected abstract DataPipeline<PoolEntry<object[]>> BuildDataPipeline();
        protected virtual void ConfigureTargetConnection(TTargetConn connection, int columnCount, int batchSize) {}
        
        public virtual void Replicate<TSourceAdapter>(int readBufferSize, int insertBatchSize) 
            where TSourceAdapter : SourceAdapterSqlBase
        {
            if (!_prepared)
                throw new InvalidOperationException("DataReplicator not prepared. Can't run.");
            try
            {
                _parallelRunner.For(0, _readers.Count, i =>
                {
                    EnsureTableCreated(i);

                    _readers[i] ??= _sourceCmds[i].ExecuteReader();
                    var sourceAdapter = GenericObjectBuilder.Build<TSourceAdapter>(_readers[i], readBufferSize);
                    sourceAdapter.AbortOnReadException = true;

                    var columnNames = new List<string>();
                    foreach (var meta in ColumnMetadataLists[i])
                        columnNames.Add(meta.ColumnName);

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
            finally
            {
                Disposer.Dispose(_readers);
            }
        }

        public virtual void UnPrepare(bool skipPreparedCheck = false)
        {
            if (!_prepared && !skipPreparedCheck)
                throw new InvalidOperationException("DataReplicator not prepared. Can't UnPrepare.");
            Dispose();
            _parallelRunner = null;
            ColumnMetadataLists = null;
            _prepared = false;
        }
        
        public virtual void Dispose()
        {
            Disposer.Dispose(ref _readers);
            Disposer.Dispose(ref _sourceCmds);
            Disposer.Dispose(ref _sourceConnections);
            Disposer.Dispose(ref _targetConnections);
        }
    }
}
