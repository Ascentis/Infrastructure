using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Bulk
{
    public class DataPipelineTargetAdapterSqlInsert : DataPipelineTargetAdapter<PoolEntry<object[]>>
    {
        public const int DefaultBatchSize = 100;

        private static readonly DataPipelineColumnMetadataToDbTypeMapper ParamMapper = 
            new DataPipelineColumnMetadataToDbTypeMapper
            {
                UseShortParam = true
            };

        private readonly string _tableName;
        private readonly IEnumerable<string> _columnNames;
        private readonly SqlConnection _sqlConnection;
        private readonly int _batchSize;
        private readonly List<PoolEntry<object[]>> _rows;
        private SqlCommand _sqlCommand;
        private SqlCommand _sqlCommandRowByRow;

        public DataPipelineTargetAdapterSqlInsert(string tableName, 
            IEnumerable<string> columnNames, 
            SqlConnection sqlConnection, 
            int batchSize = DefaultBatchSize)
        {
            _tableName = tableName;
            _columnNames = columnNames;
            _sqlConnection = sqlConnection;
            _batchSize = batchSize;
            _rows = new List<PoolEntry<object[]>>();
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        private string BuildBulkInsertSql(int rowCount)
        {
            const string rowSeparator = ",\r\n";

            var sqlText = $"INSERT INTO {_tableName}\r\n(";
            foreach (var columnName in _columnNames)
                sqlText += $"{columnName},";
            sqlText = sqlText.Remove(sqlText.Length - 1, 1);
            sqlText += ") VALUES\r\n";
            for (var i = 0; i < rowCount; i++)
            {
                sqlText += "(";
                var columnIndex = 0;
                foreach (var dummy in _columnNames)
                    sqlText += $"@P{columnIndex++}_{i},";
                sqlText = sqlText.Remove(sqlText.Length - 1, 1);
                sqlText += $"){rowSeparator}";
            }
            sqlText = sqlText.Remove(sqlText.Length - rowSeparator.Length, rowSeparator.Length);

            return sqlText;
        }

        private void BuildSqlCommand(int rowCount, ref SqlCommand sqlCommand)
        {
            sqlCommand?.Dispose();
            var sqlCommandText = BuildBulkInsertSql(rowCount);
            sqlCommand = new SqlCommand(sqlCommandText, _sqlConnection);
            ParamMapper.Map(Source.ColumnMetadatas, sqlCommand.Parameters, rowCount);
            sqlCommand.Prepare();
        }
        
        private void ExecuteFallbackRowByRow()
        {
            if(_sqlCommandRowByRow == null)
                BuildSqlCommand(1, ref _sqlCommandRowByRow);
            foreach (var row in _rows)
            {
                var paramIndex = 0;
                foreach (var value in row.Value)
                    _sqlCommandRowByRow.Parameters[paramIndex++].Value = value;
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

        private void Flush()
        {
            try
            {
                if (_sqlCommand == null || _rows.Count != _batchSize)
                    BuildSqlCommand(_rows.Count, ref _sqlCommand);

                var paramIndex = 0;
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var row in _rows)
                foreach (var value in row.Value)
                    _sqlCommand.Parameters[paramIndex++].Value = value;

                try
                {
                    _sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    if (AbortOnProcessException??false)
                        throw;
                    ExecuteFallbackRowByRow();
                }
            }
            finally
            {
                foreach (var row in _rows)
                    row.Pool.Release(row);
                _rows.Clear();
            }
        }

        public override void Process(PoolEntry<object[]> row)
        {
            row.Retain();
            _rows.Add(row);
            if (_rows.Count >= _batchSize)
                Flush();
        }

        public override void UnPrepare()
        {
            try
            {
                if (_rows.Count > 0)
                    Flush();
            }
            finally
            {
                _sqlCommand?.Dispose();
                _sqlCommand = null;
                _sqlCommandRowByRow?.Dispose();
                _sqlCommandRowByRow = null;
            }
            base.UnPrepare();
        }

        public override void AbortedWithException(Exception e)
        {
            UnPrepare();
            base.AbortedWithException(e);
        }
    }
}
