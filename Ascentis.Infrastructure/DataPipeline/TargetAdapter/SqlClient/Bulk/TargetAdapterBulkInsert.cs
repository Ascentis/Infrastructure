using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Bulk
{
    public class TargetAdapterBulkInsert : TargetAdapterBulk
    {
        private readonly string _tableName;
        private SqlCommand _sqlCommandRowByRow;
        
        public bool FallbackRowByRow { get; set; }

        public TargetAdapterBulkInsert(string tableName, 
            IEnumerable<string> columnNames, 
            SqlConnection sqlConnection, 
            int batchSize = DefaultBatchSize) : base(columnNames, sqlConnection, batchSize)
        {
            _tableName = tableName;
            FallbackRowByRow = true;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        protected override string BuildBulkSql(int rowCount)
        {
            const string rowSeparator = ",\r\n";

            var sqlText = $"INSERT INTO {_tableName}\r\n(";
            foreach (var columnName in ColumnNames)
                sqlText += $"\"{columnName}\",";
            sqlText = sqlText.Remove(sqlText.Length - 1, 1);
            sqlText += ") VALUES\r\n";
            for (var i = 0; i < rowCount; i++)
            {
                sqlText += "(";
                var columnIndex = 0;
                foreach (var dummy in ColumnNames)
                    sqlText += $"@P{columnIndex++}_{i},";

                sqlText = sqlText.Remove(sqlText.Length - 1, 1);
                sqlText += $"){rowSeparator}";
            }
            sqlText = sqlText.Remove(sqlText.Length - rowSeparator.Length, rowSeparator.Length);

            return sqlText;
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

        protected override void Flush()
        {
            try
            {
                InternalFlush();
            }
            catch (SqlException)
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
