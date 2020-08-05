using System.Data;
using System.Data.SqlClient;

namespace Ascentis.Infrastructure.DataStreamer.SourceAdapter.SqlClient
{
    public static class DataStreamerSqlAdapterExtensions
    {
        public static void Run<T>(this DataStreamer<T, object[]> dataStreamer,
            SqlDataReader source,
            IDataStreamerTargetFormatter<T, object[]> dataStreamerTargetFormatter,
            T target,
            int rowsPoolCapacity = SqlDataReaderDataStreamerSourceAdapter.DefaultRowsCapacity)
        {
            var adapter = new SqlDataReaderDataStreamerSourceAdapter(source, rowsPoolCapacity);
            dataStreamer.Run(adapter, dataStreamerTargetFormatter, target);
        }

        public static void Run<T>(this DataStreamer<T, object[]> dataStreamer, 
            SqlCommand source, 
            IDataStreamerTargetFormatter<T, object[]> dataStreamerTargetFormatter, 
            T target, 
            int rowsPoolCapacity = SqlDataReaderDataStreamerSourceAdapter.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            dataStreamer.Run(reader, dataStreamerTargetFormatter, target, rowsPoolCapacity);
        }

        public static void Run<T>(this DataStreamer<T, object[]> dataStreamer,
            string sourceSql,
            SqlConnection sourceConnection,
            IDataStreamerTargetFormatter<T, object[]> dataStreamerTargetFormatter,
            T target,
            int rowsPoolCapacity = SqlDataReaderDataStreamerSourceAdapter.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            dataStreamer.Run(sqlCommand, dataStreamerTargetFormatter, target, rowsPoolCapacity);
        }
    }
}
