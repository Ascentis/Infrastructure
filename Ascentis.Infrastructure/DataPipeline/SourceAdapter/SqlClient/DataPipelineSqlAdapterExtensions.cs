using System.Data;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient
{
    public static class DataPipelineSqlAdapterExtensions
    {
        public static void Pump<T>(this DataPipeline<T, object[]> dataPipeline,
            SqlDataReader source,
            IDataPipelineTargetAdapter<T, object[]> dataPipelineTargetAdapter,
            T target,
            int rowsPoolCapacity = SqlDataReaderDataPipelineSourceAdapter.DefaultRowsCapacity)
        {
            var adapter = new SqlDataReaderDataPipelineSourceAdapter(source, rowsPoolCapacity);
            dataPipeline.Pump(adapter, dataPipelineTargetAdapter, target);
        }

        public static void Pump<T>(this DataPipeline<T, object[]> dataPipeline, 
            SqlCommand source, 
            IDataPipelineTargetAdapter<T, object[]> dataPipelineTargetAdapter, 
            T target, 
            int rowsPoolCapacity = SqlDataReaderDataPipelineSourceAdapter.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            dataPipeline.Pump(reader, dataPipelineTargetAdapter, target, rowsPoolCapacity);
        }

        public static void Pump<T>(this DataPipeline<T, object[]> dataPipeline,
            string sourceSql,
            SqlConnection sourceConnection,
            IDataPipelineTargetAdapter<T, object[]> dataPipelineTargetAdapter,
            T target,
            int rowsPoolCapacity = SqlDataReaderDataPipelineSourceAdapter.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            dataPipeline.Pump(sqlCommand, dataPipelineTargetAdapter, target, rowsPoolCapacity);
        }
    }
}
