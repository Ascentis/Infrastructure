using System;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient
{
    public class DataPipelineTargetAdapterSqlCommand : DataPipelineTargetAdapter<PoolEntry<object[]>>
    {
        private readonly SqlCommand _cmd;

        public DataPipelineTargetAdapterSqlCommand(SqlCommand cmd)
        {
            _cmd = cmd;
        }

        public override void Prepare(IDataPipelineSourceAdapter<PoolEntry<object[]>> source)
        {
            base.Prepare(source);
            new DataPipelineColumnMetadataToDbTypeMapper().Map(source.ColumnMetadatas, _cmd.Parameters);
            _cmd.Prepare();
        }

        public override void Process(PoolEntry<object[]> row)
        {
            for (var i = 0; i < Source.FieldCount; i++)
                _cmd.Parameters[i].Value = row.Value[i];
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
