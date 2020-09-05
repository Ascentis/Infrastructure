using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace Ascentis.Infrastructure.Sql.DataPipeline.TargetAdapter.Sql.Oracle.Bulk
{
    public class OracleArrayBindingHelper
    {
        public delegate object SourceValueToParamValueDelegate(int columnIndex, IReadOnlyList<object> row);
        private readonly List<object[]> _cachedParamsArrayList;
        private readonly IDictionary<string, int> _columnNameToMetadataIndexMap;
        private readonly SourceValueToParamValueDelegate _sourceValueToParamValueDelegate;

        public OracleArrayBindingHelper(int batchSize, IDictionary<string, int> columnNameToMetadataIndexMap, SourceValueToParamValueDelegate sourceValueToParamValueDelegate)
        {
            _columnNameToMetadataIndexMap = columnNameToMetadataIndexMap;
            _sourceValueToParamValueDelegate = sourceValueToParamValueDelegate;
            _cachedParamsArrayList = new List<object[]>();
            for (var i = 0; i < _columnNameToMetadataIndexMap.Count; i++)
                _cachedParamsArrayList.Add(new object[batchSize]);
        }

        public void BindParameters(OracleCommand cmd, List<PoolEntry<object[]>> rows)
        {
            cmd.ArrayBindCount = rows.Count;
            var paramIndex = 0;
            foreach (var column in _columnNameToMetadataIndexMap)
            {
                object[] arr;
                if (column.Value >= 0)
                {
                    arr = _cachedParamsArrayList[column.Value];
                    var rowIndex = 0;
                    foreach (var row in rows)
                        arr[rowIndex++] = _sourceValueToParamValueDelegate(column.Value, row.Value);
                }
                else
                    arr = null;

                cmd.Parameters[paramIndex++].Value = arr;
            }
        }
    }
}
