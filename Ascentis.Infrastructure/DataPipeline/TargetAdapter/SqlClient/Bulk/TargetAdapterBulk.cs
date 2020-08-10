using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Generic;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Bulk
{
    public abstract class TargetAdapterBulk : TargetAdapter<PoolEntry<object[]>>, ITargetAdapterBulk
    {
        public const int DefaultBatchSize = 100;
        // ReSharper disable once InconsistentNaming
        public const int MaxMSSQLParams = 2100;
        
        protected IDictionary<string, int> ColumnNameToMetadataIndexMap;
        protected IEnumerable<string> ColumnNames;
        protected int BatchSize;
        protected List<PoolEntry<object[]>> Rows;
        protected SqlCommand SqlCommand;
        protected SqlConnection SqlConnection;
        protected SqlTransaction SqlTransaction;
        protected static readonly ColumnMetadataToDbTypeMapper ParamMapper =
            new ColumnMetadataToDbTypeMapper
            {
                UseShortParam = true
            };

        protected TargetAdapterBulk(IEnumerable<string> columnNames, SqlConnection sqlConnection, int batchSize)
        {
            ColumnNameToMetadataIndexMap = new Dictionary<string, int>();
            Rows = new List<PoolEntry<object[]>>();
            ColumnNames = columnNames;
            BatchSize = batchSize;
            SqlConnection = sqlConnection;
        }

        public virtual SqlConnection Connection => SqlCommand.Connection;

        public bool UseTakeSemantics { get; set; }

        public override int BufferSize => BatchSize;

        public virtual SqlTransaction Transaction
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

            if (ColumnNameToMetadataIndexMap.Count * BatchSize > MaxMSSQLParams)
                throw new TargetAdapterException($"Number of columns * target adapter buffer size exceeds MSSQL limit of {MaxMSSQLParams} parameters in a query");
        }

        protected static object SourceValueToParamValue(int columnIndex, IReadOnlyList<object> row)
        {
            var value = columnIndex >= 0 ? row[columnIndex] : null;
            return value ?? DBNull.Value;
        }

        protected static void DisposeAndNullify(ref SqlCommand sqlCommand)
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

        protected void BuildSqlCommand(int rowCount, ref SqlCommand sqlCommand)
        {
            DisposeAndNullify(ref sqlCommand);
            var sqlCommandText = BuildBulkSql(rowCount);
            sqlCommand = new SqlCommand(sqlCommandText, SqlConnection, SqlTransaction);
            ParamMapper.Map(ColumnNameToMetadataIndexMap, Source.ColumnMetadatas, sqlCommand.Parameters, rowCount);
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
