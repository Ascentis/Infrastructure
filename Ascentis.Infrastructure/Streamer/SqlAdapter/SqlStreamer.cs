using System.Data;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class StreamerSqlExtensions
    {
        public static void Run(this Streamer streamer, SqlCommand source, Stream target, int rowsPoolCapacity = 1000)
        {
            using var reader = source.ExecuteReader(CommandBehavior.SequentialAccess);
            var adapter = new SqlDataReaderStreamerAdapter(reader, rowsPoolCapacity);
            streamer.Run(adapter, target);
        }
    }
}
