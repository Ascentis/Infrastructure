using System;
using System.Collections.Generic;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic
{
    public abstract class TargetAdapterBulkInsertBase<TCmd, TTran, TConn, TException> : 
        TargetAdapterSqlBulkBase<TCmd, TTran, TConn> 
        where TCmd : DbCommand
        where TTran : DbTransaction
        where TConn : DbConnection
        where TException : Exception
    {
        private readonly string _tableName;
        private TCmd _sqlCommandRowByRow;
        
        public bool FallbackRowByRow { get; set; }

        public override TTran Transaction
        {
            set
            {
                base.Transaction = value;
                if (_sqlCommandRowByRow != null)
                    _sqlCommandRowByRow.Transaction = value;
            }
        }

        protected TargetAdapterBulkInsertBase(string tableName, 
            IEnumerable<string> columnNames, 
            TConn conn, 
            int batchSize) : base(columnNames, conn, batchSize)
        {
            _tableName = tableName;
        }

        protected override string BuildBulkSql(List<PoolEntry<object[]>> rows)
        {
            var builder = new BulkSqlCommandTextBuilder(ValueToSqlLiteralText)
            {
                TableName = _tableName,
                LiteralParamBinding = LiteralParamBinding,
                ColumnNameToMetadataIndexMap = ColumnNameToMetadataIndexMap,
                ColumnNames = ColumnNames
            };
            ConfigureBulkCommandBuilder(builder);
            return builder.BuildBulkInsertSql(rows);
        }

        private void ExecuteFallbackRowByRow()
        {
            if(_sqlCommandRowByRow == null)
                BuildSqlCommand(1, ref _sqlCommandRowByRow);
            foreach (var row in Rows)
            {
                var paramIndex = 0;
                foreach (var column in ColumnNameToMetadataIndexMap)
                    _sqlCommandRowByRow.Parameters[paramIndex++].Value = SourceValueToParamValue(column.Value, row.Value);

                try
                {
                    _sqlCommandRowByRow.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    InvokeProcessErrorEvent(row, e);
                }
            }
        }

        public override void Flush()
        {
            try
            {
                InternalFlush();
            }
            catch (TException)
            {
                if ((AbortOnProcessException ?? false) || !FallbackRowByRow)
                    throw;
                ExecuteFallbackRowByRow();
            }
            finally
            {
                InternalReleaseRows();
            }
        }

        protected override void DisposeSqlCommands()
        {
            base.DisposeSqlCommands();
            DisposeAndNullify(ref _sqlCommandRowByRow);
        }
    }
}
