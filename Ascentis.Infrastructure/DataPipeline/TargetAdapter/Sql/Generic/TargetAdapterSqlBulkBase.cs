using System;
using System.Collections.Generic;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic
{
    public abstract class TargetAdapterSqlBulkBase<TCmd, TTran, TConn> : TargetAdapterSql 
        where TCmd : DbCommand
        where TTran : DbTransaction
        where TConn : DbConnection
    {
        private static readonly GenericObjectBuilder.ConstructorDelegate<TCmd> CmdBuilder = GenericObjectBuilder.Builder<TCmd>(typeof(string), typeof(TConn), typeof(TTran));

        protected IDictionary<string, int> ColumnNameToMetadataIndexMap;
        protected IEnumerable<string> ColumnNames;
        protected int BatchSize;
        protected List<PoolEntry<object[]>> Rows;
        protected TCmd Cmd;
        protected TConn Conn;
        protected TTran Tran;

        protected TargetAdapterSqlBulkBase(IEnumerable<string> columnNames, TConn conn, int batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
            ColumnNames = columnNames;
            BatchSize = batchSize;
            Conn = conn;
        }
        
        public virtual TConn Connection => (TConn)Cmd.Connection;

        public override int BufferSize => BatchSize;

        public bool ParamsAsList { get; set; }
        
        public override DbCommand TakeCommand()
        {
            var cmd = Cmd;
            Cmd = null;
            return cmd;
        }

        public virtual TTran Transaction
        {
            get => Tran;
            set
            {
                Tran = value;
                if (Cmd != null)
                    Cmd.Transaction = value;
            }
        }

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            foreach (var columnName in ColumnNames)
            {
                var metaIndex = Source.MetadatasColumnToIndexMap.TryGetValue(columnName, out var index) ? index : -1;
                ColumnNameToMetadataIndexMap.Add(columnName, metaIndex);
            }
        }
        
        protected static void DisposeAndNullify(ref TCmd sqlCommand)
        {
            sqlCommand?.Dispose();
            sqlCommand = null;
        }

        protected virtual void DisposeSqlCommands()
        {
            DisposeAndNullify(ref Cmd);
        }

        public override void AbortedWithException(Exception e)
        {
            DisposeSqlCommands();
            base.AbortedWithException(e);
        }

        protected abstract string BuildBulkSql(int rowCount);
        protected abstract void MapParams(IDictionary<string, int> paramToMetaIndex, ref TCmd sqlCommand, int rowCount);

        protected void BuildSqlCommand(int rowCount, ref TCmd sqlCommand)
        {
            DisposeAndNullify(ref sqlCommand);
            var sqlCommandText = BuildBulkSql(rowCount);
            sqlCommand = CmdBuilder(sqlCommandText, Conn, Tran);
            MapParams(ColumnNameToMetadataIndexMap, ref sqlCommand, rowCount);
            sqlCommand.Prepare();
        }

        public abstract void Flush();

        public override void Process(PoolEntry<object[]> row)
        {
            if (InvokeBeforeTargetAdapterProcessRowEvent(row) == BeforeProcessRowResult.Abort)
                return;

            if (UseTakeSemantics && !row.Take())
                return;

            row.Retain();
            Rows.Add(row);
            
            InvokeAfterTargetAdapterProcessRowEvent(row);
            
            if (Rows.Count >= BatchSize)
                Flush();
        }

        public override void UnPrepare()
        {
            try
            {
                if (Rows.Count > 0)
                    Flush();
            }
            finally
            {
                DisposeSqlCommands();
            }
            base.UnPrepare();
        }

        private void EnsureCommandBuilt()
        {
            if (Cmd == null || Rows.Count != BatchSize)
                BuildSqlCommand(Rows.Count, ref Cmd);
        }

        public override void BindParameters()
        {
            EnsureCommandBuilt();

            var paramIndex = 0;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var row in Rows)
                foreach (var column in ColumnNameToMetadataIndexMap)
                    Cmd.Parameters[paramIndex++].Value = SourceValueToParamValue(column.Value, row.Value);
        }

        protected void InternalFlush()
        {
            BindParameters();
            Cmd.ExecuteNonQuery();
        }

        protected void InternalReleaseRows()
        {
            foreach (var row in Rows)
                row.Pool.Release(row);
            Rows.Clear();
        }
    }
}
