using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;

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
            for (var i = 0; i < reader.FieldCount; i++)
                formatString += $"{{{i}}},";
            formatString = formatString.Remove(formatString.Length - 1) + "\r\n";

            // ReSharper disable once AccessToDisposedClosure
            var rowsFactory = new ConcurrentReservoir<object[]>(1000, () => new object[reader.FieldCount]);
            var dataAvailable = new ManualResetEventSlim(false);
            var rowsQueue = new ConcurrentQueue<object[]>();
            var writerThread = new Thread(() =>
            {
                while(true)
                {
                    object[] row;
                    do
                    {
                        if (rowsQueue.TryDequeue(out row)) 
                            break;
                        dataAvailable.Wait();
                        dataAvailable.Reset();
                    } while (true);

                    if (row == null)
                        break;

                    var s = string.Format(formatString, row);
                    rowsFactory.Release(row);
                    var buf = Encoding.UTF8.GetBytes(s);
                    stream.Write(buf, 0, buf.Length);
                }
            });
            writerThread.Start();
            while (reader.Read())
            {
                var row = rowsFactory.Acquire();
                reader.GetValues(row);
                rowsQueue.Enqueue(row);
                dataAvailable.Set();
            }

            rowsQueue.Enqueue(null);
            dataAvailable.Set();
            writerThread.Join();
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
