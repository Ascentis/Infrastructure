﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Bulk
{
    public class TargetAdapterInsert : TargetAdapter<PoolEntry<object[]>>
    {
        public const int DefaultBatchSize = 100;
        // ReSharper disable once InconsistentNaming
        public const int MaxMSSQLParams = 2100;

        private static readonly ColumnMetadataToDbTypeMapper ParamMapper = 
            new ColumnMetadataToDbTypeMapper
            {
                UseShortParam = true
            };

        private readonly string _tableName;
        private readonly IDictionary<string, int> _columnNameToMetadataIndexMap;
        private readonly IEnumerable<string> _columnNames;
        private readonly SqlConnection _sqlConnection;
        private readonly int _batchSize;
        private readonly List<PoolEntry<object[]>> _rows;
        private SqlCommand _sqlCommand;
        private SqlCommand _sqlCommandRowByRow;

        public bool UseTakeSemantics { get; set; }

        public override int BufferSize => _batchSize;

        public TargetAdapterInsert(string tableName, 
            IEnumerable<string> columnNames, 
            SqlConnection sqlConnection, 
            int batchSize = DefaultBatchSize)
        {
            _tableName = tableName;
            _columnNames = columnNames;
            _sqlConnection = sqlConnection;
            _batchSize = batchSize;
            _columnNameToMetadataIndexMap = new Dictionary<string, int>();
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

        private static void DisposeAndNullify(ref SqlCommand sqlCommand)
        {
            sqlCommand?.Dispose();
            sqlCommand = null;
        }

        private void BuildSqlCommand(int rowCount, ref SqlCommand sqlCommand)
        {
            DisposeAndNullify(ref sqlCommand);
            var sqlCommandText = BuildBulkInsertSql(rowCount);
            sqlCommand = new SqlCommand(sqlCommandText, _sqlConnection);
            ParamMapper.Map(_columnNameToMetadataIndexMap, Source.ColumnMetadatas, sqlCommand.Parameters, rowCount);
            sqlCommand.Prepare();
        }

        private static object SourceValueToParamValue(int columnIndex, object[] row)
        {
            var value = columnIndex >= 0 ? row[columnIndex] : null;
            return value ?? DBNull.Value;
        }

        private void ExecuteFallbackRowByRow()
        {
            if(_sqlCommandRowByRow == null)
                BuildSqlCommand(1, ref _sqlCommandRowByRow);
            foreach (var row in _rows)
            {
                var paramIndex = 0;
                foreach (var column in _columnNameToMetadataIndexMap)
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

        private void Flush()
        {
            try
            {
                if (_sqlCommand == null || _rows.Count != _batchSize)
                    BuildSqlCommand(_rows.Count, ref _sqlCommand);

                var paramIndex = 0;
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var row in _rows)
                    foreach (var column in _columnNameToMetadataIndexMap)
                        _sqlCommand.Parameters[paramIndex++].Value = SourceValueToParamValue(column.Value, row.Value);

                try
                {
                    _sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
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

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            foreach (var columnName in _columnNames)
            {
                var metaIndex = Source.MetadatasColumnToIndexMap.TryGetValue(columnName, out var index) ? index : -1;
                _columnNameToMetadataIndexMap.Add(columnName, metaIndex);
            }

            if (_columnNameToMetadataIndexMap.Count * _batchSize > MaxMSSQLParams)
                throw new TargetAdapterException($"Number of columns * target adapter buffer size exceeds MSSQL limit of {MaxMSSQLParams} parameters in a query");
        }

        public override void Process(PoolEntry<object[]> row)
        {
            if (UseTakeSemantics && !row.Take())
                return;
            row.Retain();
            _rows.Add(row);
            if (_rows.Count >= _batchSize)
                Flush();
        }

        private void DisposeSqlCommands()
        {
            DisposeAndNullify(ref _sqlCommand);
            DisposeAndNullify(ref _sqlCommandRowByRow);
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
                DisposeSqlCommands();
            }
            base.UnPrepare();
        }

        public override void AbortedWithException(Exception e)
        {
            DisposeSqlCommands();
            base.AbortedWithException(e);
        }
    }
}