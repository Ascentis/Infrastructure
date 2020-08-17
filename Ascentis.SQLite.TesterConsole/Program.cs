using System;
using System.Data.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.SQLite.TesterConsole
{
    class Program
    {
        static void Main()
        {
            try
            {
                // mode=memory
                // "Data Source=c:\\inmemorydb.db;cache=shared;synchronous=Off;New=True;"
                //using var conn = new SQLiteConnection("Data Source=inmemorydb.db;New=True;");
                var conn = new SQLiteConnection("FullUri=file::memory:?cache=shared;");
                conn.Open();
                using var createTbl = new SQLiteCommand("CREATE TABLE TEST (F TEXT)", conn);
                createTbl.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                using var conn2 = new SQLiteConnection("FullUri=file::memory:?cache=shared;");
                conn2.Open();
                using var q = new SQLiteCommand("SELECT tbl_name FROM sqlite_master WHERE type='table'", conn2);
                using var reader = q.ExecuteReader();
                var metadata = new ColumnMetadataList(reader);
                foreach (var meta in metadata)
                {
                    Console.Write("{0} ", meta.ColumnName);
                }

                Console.WriteLine();
                var row = new object[reader.FieldCount];
                while (true)
                {
                    if (!reader.Read())
                        break;
                    reader.GetValues(row);
                    foreach (var value in row)
                        Console.Write("{0} ", value);
                    Console.WriteLine();
                }

                //Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
