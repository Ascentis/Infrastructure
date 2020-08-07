using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient
{
    public class DataPipelineTargetAdapterSqlCommand : DataPipelineTargetAdapter<PoolEntry<object[]>>
    {
        private static readonly DataPipelineColumnMetadataToDbTypeMapper ParamMapper = new DataPipelineColumnMetadataToDbTypeMapper();
        private readonly SqlCommand _cmd;
        private int[] _paramToMetaMap;

        public DataPipelineTargetAdapterSqlCommand(SqlCommand cmd)
        {
            _cmd = cmd;
        }

        public override void Prepare(IDataPipelineSourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);

            var parameters = _cmd.ParseParameters();
            var paramToMetaIndex = new Dictionary<string, int>();
            _paramToMetaMap = new int[parameters.Count];
            var i = 0;
            foreach (var parameter in parameters)
            {
                var metaIndex = Source.MetadatasColumnToIndexMap.TryGetValue(parameter, out var index) ? index : -1;
                paramToMetaIndex.Add(parameter, metaIndex);
                _paramToMetaMap[i++] = metaIndex;
            }
            ParamMapper.Map(paramToMetaIndex, source.ColumnMetadatas, _cmd.Parameters);

            _cmd.Prepare();
        }

        public override void Process(PoolEntry<object[]> row)
        {
            for (var i = 0; i < _cmd.Parameters.Count; i++)
                _cmd.Parameters[i].Value = row.Value[_paramToMetaMap[i]];
            try
            {
                _cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                if (AbortOnProcessException??false)
                    throw;
                InvokeProcessErrorEvent(row, e);
            }
        }
    }
}
