using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Ascentis.Infrastructure.DataStreamer.SourceAdapter.Sql
{
    public static class DataStreamerSqlAdapterExtensions
    {
        public static void Run(this DataStreamer dataStreamer,
            SqlDataReader source,
            IDataStreamerTargetFormatter dataStreamerTargetFormatter,
            Stream target,
            int rowsPoolCapacity = SqlDataReaderDataStreamerSourceAdapter.DefaultRowsCapacity)
        {
            var adapter = new SqlDataReaderDataStreamerSourceAdapter(source, rowsPoolCapacity);
            dataStreamer.Run(adapter, dataStreamerTargetFormatter, target);
        }

        public static void Run(this DataStreamer dataStreamer, 
            SqlCommand source, 
            IDataStreamerTargetFormatter dataStreamerTargetFormatter, 
            Stream target, 
            int rowsPoolCapacity = SqlDataReaderDataStreamerSourceAdapter.DefaultRowsCapacity)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            dataStreamer.Run(reader, dataStreamerTargetFormatter, target, rowsPoolCapacity);
        }

        public static void Run(this DataStreamer dataStreamer,
            string sourceSql,
            SqlConnection sourceConnection,
            IDataStreamerTargetFormatter dataStreamerTargetFormatter,
            Stream target,
            int rowsPoolCapacity = SqlDataReaderDataStreamerSourceAdapter.DefaultRowsCapacity)
        {
            using var sqlCommand = new SqlCommand(sourceSql, sourceConnection);
            dataStreamer.Run(sqlCommand, dataStreamerTargetFormatter, target, rowsPoolCapacity);
        }
    }
}
