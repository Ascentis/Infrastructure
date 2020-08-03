using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SqlStreamer
    {
        public const int DefaultRowsPoolCapacity = 1000;
        private readonly SqlCommand _cmd;
        public int RowsPoolCapacity = DefaultRowsPoolCapacity;

        public SqlStreamer(SqlCommand cmd)
        {
            _cmd = cmd;
        }

        public void WriteToStream(Stream stream, ISqlStreamerFormatter sqlStreamerFormatter)
        {
            using var reader = _cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            var fieldCount = reader.FieldCount;
            sqlStreamerFormatter.Prepare(reader, stream);
            try
            {

                var rowsPool = new Pool<object[]>(RowsPoolCapacity, () => new object[fieldCount]);
                var writingConveyor = new Conveyor<object[]>(row =>
                {
                    sqlStreamerFormatter.Process(row, stream);
                    rowsPool.Release(row);
                });
                writingConveyor.Start();
                while (reader.Read())
                {
                    var row = rowsPool.Acquire();
                    reader.GetValues(row);
                    writingConveyor.InsertPacket(row);
                }

                writingConveyor.StopAndWait();
                sqlStreamerFormatter.UnPrepare(stream);
            }
            catch (Exception e)
            {
                sqlStreamerFormatter.AbortedWithException(e);
                throw;
            }
        }
    }
}
