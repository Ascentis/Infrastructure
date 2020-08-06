using System.Data;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient
{
    public class SqlDataPipeline : DataPipeline<object[]>
    {
        public void Pump(SqlDataReader source,
            IDataPipelineTargetAdapter<object[]> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineSqlSourceAdapter.DefaultRowsCapacity)
        {
            var adapter = new DataPipelineSqlSourceAdapter(source, rowsPoolCapacity);
            Pump(adapter, dataPipelineTargetAdapter);
        }

        public void Pump(SqlCommand source,
            IDataPipelineTargetAdapter<object[]> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineSqlSourceAdapter.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            Pump(reader, dataPipelineTargetAdapter, rowsPoolCapacity);
        }

        public void Pump(string sourceSql,
            SqlConnection sourceConnection,
            IDataPipelineTargetAdapter<object[]> dataPipelineTargetAdapter,
            int rowsPoolCapacity = DataPipelineSqlSourceAdapter.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            Pump(sqlCommand, dataPipelineTargetAdapter, rowsPoolCapacity);
        }
    }
}
