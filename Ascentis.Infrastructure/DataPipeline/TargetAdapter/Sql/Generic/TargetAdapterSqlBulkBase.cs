using System;
using System.Collections.Generic;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic
{
    public abstract class TargetAdapterSqlBulkBase<TCmd, TTran, TCon> : TargetAdapterSql 
        where TCmd : DbCommand
        where TTran : DbTransaction
        where TCon : DbConnection
    {
        private static readonly GenericObjectBuilder.ConstructorDelegate<TCmd> CmdBuilder = GenericObjectBuilder.Builder<TCmd>(new [] {typeof(string), typeof(TCon), typeof(TTran)});

        protected IDictionary<string, int> ColumnNameToMetadataIndexMap;
        protected IEnumerable<string> ColumnNames;
        protected int BatchSize;
        protected List<PoolEntry<object[]>> Rows;
        protected TCmd SqlCommand;
        protected TCon SqlConnection;
        protected TTran SqlTransaction;

        protected TargetAdapterSqlBulkBase(IEnumerable<string> columnNames, TCon sqlConnection, int batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
            ColumnNames = columnNames;
            BatchSize = batchSize;
            SqlConnection = sqlConnection;
        }
        
        public virtual TCon Connection => (TCon)SqlCommand.Connection;

        public override int BufferSize => BatchSize;

        public virtual TTran Transaction
        {
            get => SqlTransaction;
            set
            {
                SqlTransaction = value;
                if (SqlCommand != null)
                    SqlCommand.Transaction = value;
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

        protected static object SourceValueToParamValue(int columnIndex, IReadOnlyList<object> row)
        {
            var value = columnIndex >= 0 ? row[columnIndex] : null;
            return value ?? DBNull.Value;
        }

        protected static void DisposeAndNullify(ref TCmd sqlCommand)
        {
            sqlCommand?.Dispose();
            sqlCommand = null;
        }

        protected virtual void DisposeSqlCommands()
        {
            DisposeAndNullify(ref SqlCommand);
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
            sqlCommand = CmdBuilder(sqlCommandText, SqlConnection, SqlTransaction);
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

        protected void InternalFlush()
        {
            if (SqlCommand == null || Rows.Count != BatchSize)
                BuildSqlCommand(Rows.Count, ref SqlCommand);

            var paramIndex = 0;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var row in Rows)
                foreach (var column in ColumnNameToMetadataIndexMap)
                    SqlCommand.Parameters[paramIndex++].Value = SourceValueToParamValue(column.Value, row.Value);

            SqlCommand.ExecuteNonQuery();
        }

        protected void InternalReleaseRows()
        {
            foreach (var row in Rows)
                row.Pool.Release(row);
            Rows.Clear();
        }
    }
}
