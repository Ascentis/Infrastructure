using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestSqlToStream
    {
        private SqlConnection conn;

        [TestInitialize]
        public void TestInitialize()
        {
            conn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            conn.Open();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            conn.Close();
            conn.Dispose();
        }

        [TestMethod]
        public void TestSqlToStreamBasic()
        {
            using var cmd = new SqlCommand( "SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlToStream(cmd);
            streamer.WriteToStream(stream);
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHR,0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToStreamToFile()
        {
            using var cmd = new SqlCommand( "SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, TPDATE, TRIM(CGROUP6), TRIM(CGROUP7), NRATE FROM TIME", conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            //using var stream = new BufferedStream(fileStream, 1024 * 1024);
            var streamer = new SqlToStream(cmd);
            streamer.WriteToStream(fileStream);
        }

        [TestMethod]
        public void TestSqlToStreamToFileSingleThreaded()
        {
            using var cmd = new SqlCommand("SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, TPDATE, TRIM(CGROUP6), TRIM(CGROUP7), NRATE FROM TIME", conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            //using var stream = new BufferedStream(fileStream, 1024 * 1024);
            var streamer = new SqlToStream(cmd);
            streamer.WriteToStreamSingleThreaded(fileStream);
        }
    }
}
