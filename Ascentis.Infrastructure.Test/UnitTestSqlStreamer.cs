using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestSqlStreamer
    {
        private SqlConnection _conn;

        [TestInitialize]
        public void TestInitialize()
        {
            _conn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            _conn.Open();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _conn.Close();
            _conn.Dispose();
        }

        [TestMethod]
        public void TestSqlToCvsStreamBasic()
        {
            using var cmd = new SqlCommand( "SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlStreamer(cmd);
            streamer.WriteToStream(stream, new SqlStreamerFormatterCsv());
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHR,0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedStreamBasic()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlStreamer(cmd);
            streamer.WriteToStream(stream, new SqlStreamerFormatterFixedLen {FieldSizes = new []{6, 4}});
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("  WKHR   0\r\n  WKHR   0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedStreamTruncateOutput()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlStreamer(cmd);
            streamer.WriteToStream(stream, new SqlStreamerFormatterFixedLen
            {
                FieldSizes = new[] { 3, 4 },
                OverflowStringFieldWidthBehaviors = new []
                {
                    SqlStreamerFormatterFixedLen.OverflowStringFieldWidthBehavior.Truncate, 
                    SqlStreamerFormatterFixedLen.OverflowStringFieldWidthBehavior.Truncate
                }
            });
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKH   0\r\nWKH   0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedStreamErrorOnOverflowString()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlStreamer(cmd);
            // ReSharper disable once AccessToDisposedClosure
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => streamer.WriteToStream(stream, new SqlStreamerFormatterFixedLen {FieldSizes = new[] { 3, 4 }})).InnerException is SqlStreamerFormatterException);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedStreamThrowsExceptionOnFieldSizes()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlStreamer(cmd);
            Assert.ThrowsException<NullReferenceException>(() => streamer.WriteToStream(stream, new SqlStreamerFormatterFixedLen()));
            Assert.ThrowsException<SqlStreamerFormatterException>(() => streamer.WriteToStream(stream, new SqlStreamerFormatterFixedLen() { FieldSizes = new []{0}}));
        }

        [TestMethod]
        public void TestSqlToCsvStreamWithHeaders()
        {
            using var cmd = new SqlCommand("SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlStreamer(cmd);
            streamer.WriteToStream(stream, new SqlStreamerFormatterCsv() { OutputHeaders = true });
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("CPCODE_EXP,NPAYCODE\r\nWKHR,0\r\nWKHR,0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToCsvStreamToFile()
        {
            using var cmd = new SqlCommand( "SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, TPDATE, TRIM(CGROUP6), TRIM(CGROUP7), NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            //using var stream = new BufferedStream(fileStream, 1024 * 1024);
            var streamer = new SqlStreamer(cmd);
            streamer.WriteToStream(fileStream, new SqlStreamerFormatterCsv());
        }

        //[TestMethod]
        public void TestSqlToCsvStreamToFileSingleThreaded()
        {
            using var cmd = new SqlCommand("SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, TPDATE, TRIM(CGROUP6), TRIM(CGROUP7), NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            //using var stream = new BufferedStream(fileStream, 1024 * 1024);
            var streamer = new SqlStreamer(cmd);
            streamer.WriteToStreamSingleThreaded(fileStream);
        }
    }
}
