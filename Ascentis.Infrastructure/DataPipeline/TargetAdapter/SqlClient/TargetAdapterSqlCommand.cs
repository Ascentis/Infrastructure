using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Generic;
using Ascentis.Infrastructure.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient
{
    public class TargetAdapterSqlCommand : TargetAdapter<PoolEntry<object[]>>, ITargetAdapterSql
    {
        private static readonly ColumnMetadataToDbTypeMapper ParamMapper = new ColumnMetadataToDbTypeMapper();
        private readonly SqlCommand _cmd;
        private int[] _paramToMetaMap;

        public TargetAdapterSqlCommand(SqlCommand cmd)
        {
            _cmd = cmd;
        }

        public virtual SqlTransaction Transaction
        {
            get => _cmd.Transaction; 
            set => _cmd.Transaction = value;
        }

        public virtual SqlConnection Connection => _cmd.Connection;

        public override void Prepare(ISourceAdapter<PoolEntry<object[]>> source)
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
            if (InvokeBeforeTargetAdapterProcessRowEvent(row) == BeforeProcessRowResult.Abort)
                return;

            for (var i = 0; i < _cmd.Parameters.Count; i++)
                _cmd.Parameters[i].Value = row.Value[_paramToMetaMap[i]];
            try
            {
                _cmd.ExecuteNonQuery();
                InvokeAfterTargetAdapterProcessRowEvent(row);
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
