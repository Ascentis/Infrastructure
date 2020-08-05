using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text;
using Ascentis.Infrastructure.DataStreamer.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestDataPipeline
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
            var streamer = new SqlDataPipeline<Stream>();
            streamer.Pump(cmd, new DataPipelineTargetAdapterDelimited(), stream);
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
            var streamer = new SqlDataPipeline<Stream>();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength {FieldSizes = new []{6, 4}}, stream);
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("  WKHR   0\r\n  WKHR   0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedStreamWithCustomFormats()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline<Stream>();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength
            {
                FieldSizes = new[] { 6, 6 },
                ColumnFormatStrings = new[] { "", "N2" }
            }, stream);
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("  WKHR  0.00\r\n  WKHR  0.00\r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedStreamLeftAligned()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline<Stream>();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength { FieldSizes = new[] { -6, -4 } }, stream);
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR  0   \r\nWKHR  0   \r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedStreamTruncateOutput()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline<Stream>();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength
            {
                FieldSizes = new[] { 3, 4 },
                OverflowStringFieldWidthBehaviors = new[]
                {
                    DataPipelineTargetAdapterFixedLength.OverflowStringFieldWidthBehavior.Truncate,
                    DataPipelineTargetAdapterFixedLength.OverflowStringFieldWidthBehavior.Truncate
                }
            }, stream);
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKH   0\r\nWKH   0\r\n", str);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedStreamErrorOnOverflowString()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline<Stream>();
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => 
                streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength {FieldSizes = new[] { 3, 4 }}, stream)).InnerException is DataStreamerException);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedStreamErrorOnOverflowOnInt()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline<Stream>();
            // ReSharper disable once AccessToDisposedClosure
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength
            {
                FieldSizes = new[] { 4, 1 },
                ColumnFormatStrings = new[] { "", "N2" }
            }, stream)).InnerException is DataStreamerException);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void TestSqlToFixedStreamThrowsExceptionOnFieldSizes()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline<Stream>();
            Assert.ThrowsException<NullReferenceException>(() => streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(), stream));
            streamer = new SqlDataPipeline<Stream>();
            Assert.ThrowsException<DataStreamerException>(() => streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength() { FieldSizes = new []{0}}, stream));
        }

        [TestMethod]
        public void TestSqlToCsvStreamWithHeaders()
        {
            using var cmd = new SqlCommand("SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline<Stream>();
            streamer.Pump(cmd, new DataPipelineTargetAdapterDelimited { OutputHeaders = true }, stream);
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
            var streamer = new SqlDataPipeline<Stream>();
            streamer.Pump(cmd, new DataPipelineTargetAdapterDelimited(), fileStream);
        }
    }
}
