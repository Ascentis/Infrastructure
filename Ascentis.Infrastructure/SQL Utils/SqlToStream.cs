using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlToStream
    {
        private SqlCommand _cmd;

        public SqlToStream(SqlCommand cmd)
        {
            _cmd = cmd;
        }

        public void WriteToStream(Stream stream)
        {
            using var reader = _cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var formatString = "";
            var fieldCount = reader.FieldCount;
            for (var i = 0; i < fieldCount; i++)
                formatString += $"{{{i}}},";
            formatString = formatString.Remove(formatString.Length - 1) + "\r\n";

            var rowsReservoir = new ConcurrentReservoir<object[]>(1000, () => new object[fieldCount]);
            var writingConveyor = new Conveyor<object[]>(row =>
            {
                var s = string.Format(formatString, row);
                rowsReservoir.Release(row);
                var buf = Encoding.UTF8.GetBytes(s);
                stream.Write(buf, 0, buf.Length);
            });
            writingConveyor.Start();
            while (reader.Read())
            {
                var row = rowsReservoir.Acquire();
                reader.GetValues(row);
                writingConveyor.InsertPacket(row);
            }
            writingConveyor.StopAndWait();
        }

        public void WriteToStreamSingleThreaded(Stream stream)
        {
            using var reader = _cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var formatString = "";
            for (var i = 0; i < reader.FieldCount; i++)
                formatString += $"{{{i}}},";
            formatString = formatString.Remove(formatString.Length - 1) + "\r\n";

            // ReSharper disable once AccessToDisposedClosure
            var row = new object[reader.FieldCount];
            while (reader.Read())
            {
                reader.GetValues(row);
                var s = string.Format(formatString, row);
                var buf = Encoding.UTF8.GetBytes(s);
                stream.Write(buf, 0, buf.Length);
            }
        }
    }
}
