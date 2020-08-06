using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text;
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
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterDelimited(stream));
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
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream) {FieldSizes = new []{6, 4}});
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("  WKHR   0\r\n  WKHR   0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedWithDefaultSizesStreamBasic()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream));
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR               0\r\nWKHR               0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToFixedStreamWithCustomFormats()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream)
            {
                FieldSizes = new[] { 6, 6 },
                ColumnFormatStrings = new[] { "", "N2" }
            });
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
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream) { FieldSizes = new[] { -6, -4 } });
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
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream)
            {
                FieldSizes = new[] { 3, 4 },
                OverflowStringFieldWidthBehaviors = new[]
                {
                    DataPipelineTargetAdapterFixedLength.OverflowStringFieldWidthBehavior.Truncate,
                    DataPipelineTargetAdapterFixedLength.OverflowStringFieldWidthBehavior.Truncate
                }
            });
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
            var streamer = new SqlDataPipeline();
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => 
                streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream) {FieldSizes = new[] { 3, 4 }})).InnerException is DataPipelineException);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedStreamErrorOnOverflowOnInt()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline();
            // ReSharper disable once AccessToDisposedClosure
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream)
            {
                FieldSizes = new[] { 4, 1 },
                ColumnFormatStrings = new[] { "", "N2" }
            })).InnerException is DataPipelineException);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void TestSqlToFixedStreamThrowsExceptionOnFieldSizes()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline();
            Assert.ThrowsException<DataPipelineException>(() => streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(stream) { FieldSizes = new []{0}}));
        }

        [TestMethod]
        public void TestSqlToCsvStreamWithHeaders()
        {
            using var cmd = new SqlCommand("SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterDelimited(stream) { OutputHeaders = true });
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
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterDelimited(fileStream));
        }

        [TestMethod]
        public void TestFixedLengthToCsvBasic()
        {
            var reader = new StringReader("WKHR               0\r\nWKHR               0\r\n");
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new FixedLengthDataPipeline();
            streamer.Pump(reader, new []
            {
                new DataPipelineColumnMetadata
                {
                    DataType = typeof(string),
                    ColumnSize = 4
                },
                new DataPipelineColumnMetadata
                {
                    DataType = typeof(int),
                    ColumnSize = 16
                }
            }, new DataPipelineTargetAdapterDelimited(stream));
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHR,0\r\n", str);
        }

        [TestMethod]
        public void TestFixedLengthToCsvErrorReading()
        {
            var exceptionCalled = false;
            var reader = new StringReader("WKHR-               A\r\nWKHR-               0\r\n");
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new FixedLengthDataPipeline();
            streamer.OnSourceAdapterRowReadError += (sourceObject, e) =>
            {
                exceptionCalled = (string)sourceObject == "WKHR-               A";
            };
            streamer.Pump(reader, new[]
            {
                new DataPipelineColumnMetadata
                {
                    DataType = typeof(string),
                    ColumnSize = 4,
                    StartPosition = 0
                },
                new DataPipelineColumnMetadata
                {
                    DataType = typeof(int),
                    ColumnSize = 16,
                    StartPosition = 5
                }
            }, new DataPipelineTargetAdapterDelimited(stream));
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\n", str);
            Assert.IsTrue(exceptionCalled);
        }

        [TestMethod]
        public void TestFixedLengthToCsvErrorPreparing()
        {
            var reader = new StringReader("WKHR-               A\r\nWKHR-               0\r\n");
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var streamer = new FixedLengthDataPipeline();
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue(Assert.ThrowsException<DataPipelineException>(() => streamer.Pump(reader, new[]
                {
                    new DataPipelineColumnMetadata
                    {
                        DataType = typeof(string),
                        ColumnSize = 4,
                        StartPosition = 0
                    },
                    new DataPipelineColumnMetadata
                    {
                        DataType = typeof(int),
                        ColumnSize = 16,
                        StartPosition = 3 // Not a valid position
                    }
                    // ReSharper disable once AccessToDisposedClosure
                }, new DataPipelineTargetAdapterDelimited(stream))).Message.Contains("Field position can't"));
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestSqlToFixedStreamToFile1MMRowsAndToCsv()
        {

            using var cmd = new SqlCommand("SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            //using var stream = new BufferedStream(fileStream, 1024 * 1024);
            var streamer = new SqlDataPipeline();
            streamer.Pump(cmd, new DataPipelineTargetAdapterFixedLength(fileStream) {FieldSizes = new []{40, 40, 20, 20}});
            fileStream.Close();
            using var fileStreamSource = new FileStream("T:\\dump.txt", FileMode.Open, FileAccess.Read);
            using var fileSourceReader = new StreamReader(fileStreamSource);
            using var targetFileStream = new FileStream("T:\\dump2.txt", FileMode.Create, FileAccess.ReadWrite);
            var textStreamer = new FixedLengthDataPipeline();
            textStreamer.Pump(new DataPipelineFixedLengthSourceAdapter(fileSourceReader)
            {
                ColumnMetadatas = new []
                {
                    new DataPipelineColumnMetadata
                    {
                        DataType = typeof(string),
                        ColumnSize = 40
                    },
                    new DataPipelineColumnMetadata 
                    {
                        DataType = typeof(int),
                        ColumnSize = 40
                    },
                    new DataPipelineColumnMetadata
                    {
                        DataType = typeof(DateTime),
                        ColumnSize = 20
                    },
                    new DataPipelineColumnMetadata
                    {
                        DataType = typeof(double),
                        ColumnSize = 20
                    }
                }
            }, new DataPipelineTargetAdapterDelimited(targetFileStream));
        }
    }
}
