using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.BlockingQueue;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SQLite;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Generic;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Single;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Bulk;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Single;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Text;
using Ascentis.Infrastructure.Test.Properties;
using Ascentis.Infrastructure.TestHelpers.AssertExtension;
using Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLiteCommand = System.Data.SQLite.SQLiteCommand;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
    public class UnitTestDataPipeline
    {
        private const string DataSourceSqLiteConnectionString = "FullUri=file::memory:?cache=shared;";
        private SqlConnection _conn;

        [TestInitialize]
        public void TestInitialize()
        {
            _conn = new SqlConnection(Settings.Default.SqlConnectionString);
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
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new DelimitedTextTargetAdapter(stream));
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
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream) {FieldSizes = new []{6, 4}});
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
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream));
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
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream)
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
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream) { FieldSizes = new[] { -6, -4 } });
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
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream)
            {
                FieldSizes = new[] { 3, 4 },
                OverflowStringFieldWidthBehaviors = new[]
                {
                    FixedLengthTextTargetAdapter.OverflowStringFieldWidthBehavior.Truncate,
                    FixedLengthTextTargetAdapter.OverflowStringFieldWidthBehavior.Truncate
                }
            });
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKH   0\r\nWKH   0\r\n", str);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedPipelineErrorOnOverflowString()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new SqlClientDataPipeline();
            pipeline.AbortOnTargetAdapterException = true;
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => 
                pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream) {FieldSizes = new[] { 3, 4 }})).InnerException is DataPipelineException);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedpipelineErrorOnOverflowOnInt()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new SqlClientDataPipeline();
            pipeline.AbortOnTargetAdapterException = true;
            // ReSharper disable once AccessToDisposedClosure
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream)
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
            var pipeline = new SqlClientDataPipeline();
            Assert.ThrowsException<DataPipelineException>(() => pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(stream) { FieldSizes = new []{0}}));
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedStreamThrowsExceptionOnBufferSizes()
        {
            using var cmd = new SqlCommand("SELECT TOP 10000 TRIM(CPCODE_EXP), NPAYCODE FROM TIME", _conn);
            using var targetConn0 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn0.Open();
            var pipeline = new SqlClientDataPipeline();
            Assert.IsTrue(Assert.ThrowsException<DataPipelineException>(() => pipeline.Pump(cmd,
                new SqlClientAdapterBulkInsert("TIME_BASE", new[] {"CPCODE_EXP", "NPAYCODE"}, targetConn0, 1000), 10)).Message.Contains("Deadlock"));
            Assert.IsTrue(Assert.ThrowsException<TargetAdapterException>(() => pipeline.Pump(cmd,
                new SqlClientAdapterBulkInsert("TIME_BASE", new[] {"CPCODE_EXP", "NPAYCODE"}, targetConn0, 1500), 10000)).Message.Contains("exceeds"));
        }

        [TestMethod]
        public void TestSqlToCsvStreamWithHeaders()
        {
            using var cmd = new SqlCommand("SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new DelimitedTextTargetAdapter(stream) { OutputHeaders = true });
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("CPCODE_EXP,NPAYCODE\r\nWKHR,0\r\nWKHR,0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToCsvStreamToFile()
        {
            using var cmd = new SqlCommand( "SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, TPDATE, TRIM(CGROUP6), TRIM(CGROUP7), NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new DelimitedTextTargetAdapter(fileStream));
        }

        [TestMethod]
        public void TestFixedLengthToCsvBasic()
        {
            var reader = new StringReader("WKHR               0\r\nWKHR               0\r\n");
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new FixedLengthTextDataPipeline();
            pipeline.Pump(reader, new ColumnMetadataList 
            {
                new ColumnMetadata
                {
                    DataType = typeof(string),
                    ColumnSize = 4
                },
                new ColumnMetadata
                {
                    DataType = typeof(int),
                    ColumnSize = 16
                }
            }, new DelimitedTextTargetAdapter(stream));
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
            var pipeline = new FixedLengthTextDataPipeline();
            pipeline.OnSourceAdapterRowReadError += (adapter, sourceObject, e) =>
            {
                exceptionCalled = (string)sourceObject == "WKHR-               A";
                Assert.AreEqual(0, adapter.Id);
            };
            pipeline.Pump(reader, new ColumnMetadataList
            {
                new ColumnMetadata
                {
                    DataType = typeof(string),
                    ColumnSize = 4,
                    StartPosition = 0
                },
                new ColumnMetadata
                {
                    DataType = typeof(int),
                    ColumnSize = 16,
                    StartPosition = 5
                }
            }, new DelimitedTextTargetAdapter(stream));
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
            var pipeline = new FixedLengthTextDataPipeline();
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue(Assert.ThrowsException<DataPipelineException>(() => pipeline.Pump(reader, new ColumnMetadataList
                {
                    new ColumnMetadata
                    {
                        DataType = typeof(string),
                        ColumnSize = 4,
                        StartPosition = 0
                    },
                    new ColumnMetadata
                    {
                        DataType = typeof(int),
                        ColumnSize = 16,
                        StartPosition = 3 // Not a valid position
                    }
                    // ReSharper disable once AccessToDisposedClosure
                }, new DelimitedTextTargetAdapter(stream))).Message.Contains("Field position can't"));
            Assert.IsTrue(Assert.ThrowsException<NullReferenceException>(() => pipeline.Pump(reader, new ColumnMetadataList 
            {
                new ColumnMetadata
                {
                    DataType = typeof(string),
                    ColumnSize = null, // Not valid
                    StartPosition = 0
                },
                new ColumnMetadata
                {
                    DataType = typeof(int),
                    ColumnSize = 16,
                    StartPosition = 3
                }
                // ReSharper disable once AccessToDisposedClosure
            }, new DelimitedTextTargetAdapter(stream))).Message.Contains("ColumnSize[0]"));
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestSqlToFixedStreamToFile1MMRowsAndToCsv()
        {
            using var cmd = new SqlCommand("SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            //using var stream = new BufferedStream(fileStream, 1024 * 1024);
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(cmd, new FixedLengthTextTargetAdapter(fileStream) {FieldSizes = new [] {40, 40, 20, 20}});
            fileStream.Close();
            using var fileStreamSource = new FileStream("T:\\dump.txt", FileMode.Open, FileAccess.Read);
            using var fileSourceReader = new StreamReader(fileStreamSource);
            using var targetFileStream = new FileStream("T:\\dump2.txt", FileMode.Create, FileAccess.ReadWrite);
            var textPipeline = new FixedLengthTextDataPipeline();
            textPipeline.Pump(new FixedLengthTextSourceAdapter(fileSourceReader)
            {
                ColumnMetadatas = new ColumnMetadataList
                {
                    new ColumnMetadata
                    {
                        DataType = typeof(string),
                        ColumnSize = 40
                    },
                    new ColumnMetadata 
                    {
                        DataType = typeof(int),
                        ColumnSize = 40
                    },
                    new ColumnMetadata
                    {
                        DataType = typeof(DateTime),
                        ColumnSize = 20
                    },
                    new ColumnMetadata
                    {
                        DataType = typeof(double),
                        ColumnSize = 20
                    }
                }
            }, new DelimitedTextTargetAdapter(targetFileStream));
        }

        [TestMethod]
        public void TestCsvToFixedBasic()
        {
            var reader = new StringReader("WKHR;;0\r\nWKHR;;0\r\n");
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new DelimitedTextDataPipeline();
            pipeline.Pump(reader, new ColumnMetadataList 
            {
                new ColumnMetadata
                {
                    DataType = typeof(string)
                },
                new ColumnMetadata
                {
                    DataType = typeof(int)
                }
            }, new FixedLengthTextTargetAdapter(stream), ";;");
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("            WKHR             0\r\n            WKHR             0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToDualCvsAndDelimitedStream()
        {
            using var cmd = new SqlCommand("SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);

            var buf2 = new byte[1000];
            using var stream2 = new MemoryStream(buf2);

            var targets = new TextTargetAdapter[]
                {
                    new DelimitedTextTargetAdapter(stream), 
                    new FixedLengthTextTargetAdapter(stream2) {FieldSizes = new []{6, 4}}
                };
            var pipeline = new SqlClientDataPipeline();

            pipeline.Pump(cmd, targets);
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHR,0\r\n", str);

            stream2.Flush();
            str = Encoding.UTF8.GetString(buf2, 0, (int)stream2.Position);
            Assert.AreEqual("  WKHR   0\r\n  WKHR   0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToDualCsvAndFixedStreamToFile()
        {
            using var cmd = new SqlCommand("SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, TPDATE, TRIM(CGROUP6), TRIM(CGROUP7), NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            using var fileStream2 = new FileStream("T:\\dump2.txt", FileMode.Create, FileAccess.ReadWrite);
            var pipeline = new SqlClientDataPipeline();
            var targets = new TextTargetAdapter[]
            {
                new DelimitedTextTargetAdapter(fileStream),
                new FixedLengthTextTargetAdapter(fileStream2) {FieldSizes = new []{40, 40, 20, 20, 30, 30, 30}}
            };
            pipeline.Pump(cmd, targets);
        }

        [TestMethod]
        public void TestSqlToSqlBasic()
        {
            using var cmd = new SqlCommand("SELECT TOP 100 CPCODE_EXP, NPAYCODE FROM TIME", _conn);
            using var targetConn = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn.Open();
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();
            using var targetCmd = new SqlCommand("INSERT INTO TIME_BASE (CPCODE_EXP, NPAYCODE) VALUES (@CPCODE_EXP, @NPAYCODE)", targetConn);
            var pipeline = new SqlClientDataPipeline {AbortOnTargetAdapterException = true};
            var targetAdapter = new SqlClientTargetAdapterCommand(targetCmd) {AnsiStringParameters = new[] {"CPCODE_EXP"}};
            pipeline.Pump(cmd, targetAdapter);
        }

        [TestMethod]
        public void TestSqlToSqlCommitEvery10Rows()
        {
            using var cmd = new SqlCommand("SELECT TOP 100 CPCODE_EXP, NPAYCODE FROM TIME", _conn);
            
            using var targetConn = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn.Open();
            
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();

            var transaction = targetConn.BeginTransaction();
            
            SqlClientTargetAdapterCommand sqlClientTargetAdapter = null;
            using var targetCmd = new SqlCommand("INSERT INTO TIME_BASE (CPCODE_EXP, NPAYCODE) VALUES (@CPCODE_EXP, @NPAYCODE)", targetConn, transaction);
            
            try
            {
                sqlClientTargetAdapter = new SqlClientTargetAdapterCommand(targetCmd);
                var pipeline = new SqlClientDataPipeline {AbortOnTargetAdapterException = true};
                var counter = 0;
                pipeline.AfterTargetAdapterProcessRow += (adapter, row) =>
                {
                    if (++counter % 10 != 0)
                        return;
                    counter = 0;
                    var adapterSqlCommand = (ITargetAdapterSqlClient) adapter;
                    adapterSqlCommand.Transaction.Commit();
                    adapterSqlCommand.Transaction = adapterSqlCommand.Connection.BeginTransaction();
                };

                pipeline.Pump(cmd, sqlClientTargetAdapter);
                sqlClientTargetAdapter.Transaction?.Commit();
            }
            catch (Exception)
            {
                sqlClientTargetAdapter?.Transaction?.Rollback();
                throw;
            }
        }

        [TestMethod]
        public void TestSqlToSqlBulkInsert()
        {
            using var cmd = new SqlCommand("SELECT TOP 40015 CEMPID, NPAYCODE, DWORKDATE, TIN, TOUT FROM TIME", _conn);
            
            using var targetConn0 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn0.Open();
            using var targetConn1 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn1.Open();
            using var targetConn2 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn2.Open();
            using var targetConn3 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn3.Open();

            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var pipeline = new SqlClientDataPipeline {AbortOnTargetAdapterException = true};

            var outPipes = new []
            { 
                new SqlClientAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn0, 300) {UseTakeSemantics = true},
                new SqlClientAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn1, 300) {UseTakeSemantics = true},
                new SqlClientAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn2, 300) {UseTakeSemantics = true},
                new SqlClientAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn3, 300) {UseTakeSemantics = true}
            };
            // ReSharper disable once RedundantArgumentDefaultValue
            pipeline.Pump(cmd, outPipes, 2400);
        }

        [TestMethod]
        public void TestSqlToSqlBulkInsertUsingLiterals()
        {
            using var cmd = new SqlCommand("SELECT TOP 10000 CEMPID, NPAYCODE, DWORKDATE, TIN, TOUT FROM TIME", _conn);

            using var targetConn0 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn0.Open();

            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var pipeline = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };

            var outPipes = new[]
            {
                new SqlClientAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn0, 300)
                {
                    UseTakeSemantics = true,
                    LiteralParamBinding = true
                }
            };
            // ReSharper disable once RedundantArgumentDefaultValue
            pipeline.Pump(cmd, outPipes, 2400);
        }

        [TestMethod]
        public void TestSqlToSqlBulkInsertWithBulkSizeOneAndPoolSizeOne()
        {
            using var cmd = new SqlCommand("SELECT TOP 10 CEMPID, NPAYCODE, DWORKDATE, TIN, TOUT FROM TIME", _conn);

            using var targetConn0 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn0.Open();
            
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var pipeline = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };

            var outPipes = new[]
            {
                new SqlClientAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn0, 1) {UseTakeSemantics = true}
            };
            // ReSharper disable once RedundantArgumentDefaultValue
            pipeline.Pump(cmd, outPipes, 1);
        }

        private static readonly LocalDataStoreSlot CounterSlot = Thread.AllocateDataSlot();

        [TestMethod]
        public void TestSqlToSqlBulkInsertWithTransaction()
        {
            using var cmd = new SqlCommand("SELECT TOP 40015 CEMPID, NPAYCODE, DWORKDATE, TIN, TOUT FROM TIME", _conn);

            using var targetConn0 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn0.Open();
            using var targetConn1 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn1.Open();

            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var transaction0 = targetConn0.BeginTransaction();
            var transaction1 = targetConn1.BeginTransaction();
            var outPipes = new[]
            {
                new SqlClientAdapterBulkInsert("TIME_BASE",
                        new[] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn0, 300)
                    {UseTakeSemantics = true, Transaction = transaction0},
                new SqlClientAdapterBulkInsert("TIME_BASE",
                        new[] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn1, 300)
                    {UseTakeSemantics = true, Transaction = transaction1}
            };
            try
            {
                var pipeline = new SqlClientDataPipeline {AbortOnTargetAdapterException = true};
                pipeline.AfterTargetAdapterProcessRow += (adapter, row) =>
                {
                    var data = Thread.GetData(CounterSlot);
                    if (data == null)
                    {
                        Thread.SetData(CounterSlot, 1);
                        return;
                    }
                    var newData = (int) data + 1;
                    Thread.SetData(CounterSlot, newData);
                    if (newData % 100 != 0)
                        return;
                    var adapterBulk = (ITargetAdapterFlushable) adapter;
                    adapterBulk.Flush();
                    var adapterBulkSqlClient = (ITargetAdapterSqlClient) adapterBulk;
                    adapterBulkSqlClient.Transaction.Commit();
                    adapterBulkSqlClient.Transaction = adapterBulkSqlClient.Connection.BeginTransaction();
                };

                pipeline.Pump(cmd, outPipes, 2400);
                foreach (var adapter in outPipes)
                    adapter.Transaction?.Commit();
            }
            catch (Exception)
            {
                foreach (var adapter in outPipes)
                    adapter.Transaction?.Rollback();
                throw;
            }
        }

        [TestMethod]
        public void TestSqlToBulkSqlBasic()
        {
            using var cmd = new SqlCommand(@"SELECT TOP 10000 CEMPID, CPAYTYPE FROM TIME ORDER BY CEMPID", _conn);
            using var targetConn = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn.Open();
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();

            var pipeline1 = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };
            pipeline1.Pump(cmd, new SqlClientAdapterBulkInsert("TIME_BASE", new []{"CEMPID", "CPAYTYPE"}, targetConn, 500), 2000);

            using var cmd2 = new SqlCommand(@"SELECT TOP 10000 CEMPID, LCALCULATE CPAYTYPE FROM TIME ORDER BY CEMPID", _conn);
            var pipeline2 = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };
            pipeline2.Pump(cmd2, new SqlClientAdapterBulkCommand(@"
                UPDATE TIME_BASE
                SET CPAYTYPE = SRC.CPAYTYPE
                FROM TIME_BASE T
                    INNER JOIN (
                    SELECT CEMPID, CPAYTYPE
                    FROM (
                        /*<DATA>*/
                        SELECT 99993 CEMPID, '1' CPAYTYPE
                        UNION ALL
                        SELECT 99999, '2'
                        /*</DATA>*/) SRC
                    ) SRC ON
                T.CEMPID = SRC.CEMPID", new []{"CEMPID", "CPAYTYPE"}, targetConn, 500), 2000);

            pipeline2.Pump(cmd2, new SqlClientAdapterBulkCommand(@"
                DELETE FROM T
                FROM TIME_BASE T
                INNER JOIN 
                (/*<DATA>*/ 
                SELECT '123' CEMPID
                UNION ALL
                SELECT '124'
                /*</DATA>*/) SRC ON 
                T.CEMPID = SRC.CEMPID", new[] { "CEMPID" }, targetConn, 500), 2000);
        }

        [TestMethod]
        public void TestSqlToBulkSqlBasicBindUsingLiterals()
        {
            using var cmd = new SqlCommand(@"SELECT TOP 10000 CEMPID, CPAYTYPE FROM TIME ORDER BY CEMPID", _conn);
            using var targetConn = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn.Open();
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();

            var pipeline1 = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };
            pipeline1.Pump(cmd, new SqlClientAdapterBulkInsert("TIME_BASE", new[] { "CEMPID", "CPAYTYPE" }, targetConn, 500), 2000);

            using var cmd2 = new SqlCommand(@"SELECT TOP 100 CEMPID, LCALCULATE CPAYTYPE FROM TIME ORDER BY CEMPID", _conn);
            var pipeline2 = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };
            pipeline2.Pump(cmd2, new SqlClientAdapterBulkCommand(@"
                UPDATE TIME_BASE
                SET CPAYTYPE = SRC.CPAYTYPE
                FROM TIME_BASE T
                    INNER JOIN (
                    SELECT CEMPID, CPAYTYPE
                    FROM (
                        /*<DATA>*/
                        SELECT 99993 CEMPID, '1' CPAYTYPE
                        UNION ALL
                        SELECT 99999, '2'
                        /*</DATA>*/) SRC
                    ) SRC ON
                T.CEMPID = SRC.CEMPID", new[] { "CEMPID", "CPAYTYPE" }, targetConn, 100) {LiteralParamBinding = true}, 2000);

            pipeline2.Pump(cmd2, new SqlClientAdapterBulkCommand(@"
                DELETE FROM T
                FROM TIME_BASE T
                INNER JOIN 
                (/*<DATA>*/ 
                SELECT '123' CEMPID
                UNION ALL
                SELECT '124'
                /*</DATA>*/) SRC ON 
                T.CEMPID = SRC.CEMPID", new[] { "CEMPID" }, targetConn, 100)  {LiteralParamBinding = true}, 2000);
        }

        [TestMethod]
        public void TestSqlToBulkSqlCommandUpdate()
        {
            using var cmd = new SqlCommand(@"SELECT TOP 20000 CEMPID, CPAYTYPE FROM TIME ORDER BY CEMPID", _conn);
            using var targetConn = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn.Open();

            using var targetConn2 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn2.Open();

            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();

            var pipeline1 = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };
            pipeline1.Pump(cmd, new SqlClientAdapterBulkInsert("TIME_BASE", new[] { "CEMPID", "CPAYTYPE" }, targetConn, 1000), 2000);

            using var cmd2 = new SqlCommand(@"SELECT TOP 20000 CEMPID, LCALCULATE CPAYTYPE FROM TIME ORDER BY CEMPID", _conn);
            var pipeline2 = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };
            var compositeSql = @"
                UPDATE TIME_BASE
                SET CPAYTYPE = SRC.CPAYTYPE
                FROM TIME_BASE T
                    INNER JOIN (
                    SELECT CEMPID, CPAYTYPE
                FROM (
                    /*<DATA>*/
                    SELECT SOME DATA
                    /*</DATA>*/) SRC
                    ) SRC ON
                T.CEMPID = SRC.CEMPID";
            var adapter1 = new SqlClientAdapterBulkCommand(compositeSql, new[] {"CEMPID", "CPAYTYPE"}, targetConn, 200) {AbortOnProcessException = true, UseTakeSemantics = true};
            var adapter2 = new SqlClientAdapterBulkCommand(compositeSql, new[] {"CEMPID", "CPAYTYPE"}, targetConn2, 200) {AbortOnProcessException = true, UseTakeSemantics = true};
            pipeline2.Pump(cmd2, new [] {adapter1, adapter2}, 2000);
        }

        [TestMethod]
        public void TestSqlToBulkSqlCommandInsert()
        {
            using var cmd = new SqlCommand("SELECT TOP 10000 * FROM TIME ORDER BY IID", _conn);

            using var targetConn = new SqlConnection(Settings.Default.SqlConnectionString2ndDatabase);
            targetConn.Open();

            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME", targetConn);
            truncateCmd.ExecuteNonQuery();

            using var targetConn2 = new SqlConnection(Settings.Default.SqlConnectionString2ndDatabase);
            targetConn2.Open();
            var tran2 = targetConn2.BeginTransaction();

            using var targetConn3 = new SqlConnection(Settings.Default.SqlConnectionString2ndDatabase);
            targetConn3.Open();
            var tran3 = targetConn3.BeginTransaction();

            var tran = targetConn.BeginTransaction();

            var pipeline2 = new SqlClientDataPipeline { AbortOnTargetAdapterException = true };
            
            const string insertSql = "INSERT INTO TIME SELECT * FROM(@@@Params) SRC";

            var adapter1 = new SqlClientAdapterBulkCommand(insertSql, pipeline2.SourceColumnNames(), targetConn, 1) { AbortOnProcessException = true, UseTakeSemantics = true };
            var adapter2 = new SqlClientAdapterBulkCommand(insertSql, pipeline2.SourceColumnNames(), targetConn2, 1) { AbortOnProcessException = true, UseTakeSemantics = true };
            var adapter3 = new SqlClientAdapterBulkCommand(insertSql, pipeline2.SourceColumnNames(), targetConn3, 1) { AbortOnProcessException = true, UseTakeSemantics = true };

            adapter1.Transaction = tran;
            adapter2.Transaction = tran2;
            adapter3.Transaction = tran3;

            pipeline2.Pump(cmd, new[] { adapter1, adapter2, adapter3 }, 1000);

            tran.Commit();
            tran2.Commit();
            tran3.Commit();

            Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(
                Settings.Default.SqlConnectionString, "SELECT TOP 10000 * FROM TIME ORDER BY IID",
                Settings.Default.SqlConnectionString2ndDatabase, "SELECT * FROM TIME ORDER BY IID");
        }

        [TestMethod]
        public void TestManualToCsvBasic()
        {
            var buf = new byte[1000];
            var stream = new MemoryStream(buf);
            var source = new BlockingQueueSourceAdapter
            {
                ColumnMetadatas = new ColumnMetadataList
                {
                    new ColumnMetadata {DataType = typeof(string), ColumnSize = 4},
                    new ColumnMetadata {DataType = typeof(int), ColumnSize = 16}
                }
            };
            var pipeline = new BlockingQueueDataPipeline();
            Assert.IsTrue(ThreadPool.QueueUserWorkItem(obj => pipeline.Pump(source, new DelimitedTextTargetAdapter(stream))));
            var entry = new object[] {"WKHR", 0};
            pipeline.Insert(new List<object[]>{entry, entry});
            pipeline.Finish(true);
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHR,0\r\n", str);
        }

        public class SampleBusinessObject
        {
            public int IntValue { get; set; }
            public string StrValue { get; set; }
            public decimal DecValue { get; set; }
        }

        [TestMethod]
        public void TestBlockingQueueToCsvBasic()
        {
            var buf = new byte[1000];
            var stream = new MemoryStream(buf);
            var source = new BlockingQueueSourceAdapter {ColumnMetadatas = new ColumnMetadataList<SampleBusinessObject>()};
            var pipeline = new BlockingQueueDataPipeline();
            Assert.IsTrue(ThreadPool.QueueUserWorkItem(obj => pipeline.Pump(source, new DelimitedTextTargetAdapter(stream))));
            var objs = new List<object>
            {
                new SampleBusinessObject {IntValue = 1, StrValue = "Hello", DecValue = 12.34M},
                new SampleBusinessObject {IntValue = 2, StrValue = "Good bye", DecValue = 11.334M},
                new SampleBusinessObject {IntValue = 5, StrValue = "truncate dec", DecValue = 11.12345678M}
            };
            var lastBizObj = new SampleBusinessObject {IntValue = 3, StrValue = "Final good bye", DecValue = 1332.8875M};
            pipeline.Insert(objs);
            pipeline.Insert(lastBizObj);
            pipeline.Finish(true);
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("1,Hello,12.34\r\n2,Good bye,11.334\r\n5,truncate dec,11.1234568\r\n3,Final good bye,1332.8875\r\n", str);
        }

        [TestMethod]
        public void TestBlockingQueueToCsvBasicUsingGenericSerializer()
        {
            var buf = new byte[1000];
            var stream = new MemoryStream(buf);
            var source = new BlockingQueueSourceAdapter { ColumnMetadatas = new ColumnMetadataList<SampleBusinessObject>() };
            var pipeline = new BlockingQueueDataPipeline();
            Assert.IsTrue(ThreadPool.QueueUserWorkItem(obj => pipeline.Pump(source, new DelimitedTextTargetAdapter(stream))));
            var objs = new List<object>
            {
                new SampleBusinessObject {IntValue = 1, StrValue = "Hello", DecValue = 12.34M},
                new SampleBusinessObject {IntValue = 2, StrValue = "Good bye", DecValue = 11.334M},
                new SampleBusinessObject {IntValue = 5, StrValue = "truncate dec", DecValue = 11.12345678M}
            };
            var lastBizObj = new SampleBusinessObject { IntValue = 3, StrValue = "Final good bye", DecValue = 1332.8875M };
            var serializer = new Serializer<SampleBusinessObject>();
            pipeline.Insert(objs);
            pipeline.Insert(lastBizObj, serializer);
            IEnumerable<SampleBusinessObject> objsSpecific = new List<SampleBusinessObject>
            {
                new SampleBusinessObject {IntValue = 10, StrValue = "last", DecValue = 1.4M}
            };
            pipeline.Insert(objsSpecific, serializer);
            var objAsArray = new object[] {2, "one", 1};
            pipeline.Insert(objAsArray);

            IEnumerable<object[]> objAsArrayArray = new List<object[]> { objAsArray };
            pipeline.Insert(objAsArrayArray);

            IEnumerable<object> objsGeneric = new List<SampleBusinessObject>
            {
                new SampleBusinessObject {IntValue = 100, StrValue = "last last", DecValue = 1.5M}
            };
            pipeline.Insert(objsGeneric);
            pipeline.Finish(true);
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("1,Hello,12.34\r\n2,Good bye,11.334\r\n5,truncate dec,11.1234568\r\n3,Final good bye,1332.8875\r\n10,last,1.4\r\n2,one,1\r\n2,one,1\r\n100,last last,1.5\r\n", str);
        }

        private ManualResetEventSlim _asyncMethodFinished;

        public class TinyBizObject
        {
            public string StrProp { get; set; }
            public int IntProp { get; set; }
        }

        private async void TestManualToCsvBasicAsyncInternal(BlockingQueueDataPipeline pipeline)
        {
            using var targetConn0 = new SqlConnection(Settings.Default.SqlConnectionString);
            targetConn0.Open();
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var buf = new byte[1000];
            var stream = new MemoryStream(buf);
            var source = new BlockingQueueSourceAdapter
            {
                ColumnMetadatas = new ColumnMetadataList
                {
                    new ColumnMetadata
                    {
                        ColumnName =  "CEMPID",
                        DataType = typeof(string), 
                        ColumnSize = 4
                    },
                    new ColumnMetadata
                    {
                        ColumnName = "NPAYCODE",
                        DataType = typeof(int), 
                        ColumnSize = 16
                    }
                }, 
                WaitForDataTimeout = 1000
            };
            var targetAdapters = new ITargetAdapter<PoolEntry<object[]>>[]
            {
                new SqlClientAdapterBulkInsert("TIME_BASE",
                        new[] {"CEMPID", "NPAYCODE"}, targetConn0, 300),
                new DelimitedTextTargetAdapter(stream)
            };
            var flushCalled = false;
            var waitForDataTimeout = false;
            pipeline.AfterFlushEvent += adapter => flushCalled = true;
            source.OnWaitForDataTimeout += adapter =>
            {
                waitForDataTimeout = true;
                pipeline.InsertFlushEvent(); // We need to Flush() in the target conveyor thread
            };
            ThreadPool.QueueUserWorkItem(obj => pipeline.Pump(source, targetAdapters));
            var entries = new List<object[]>
            {
                new object []{ "WKHR", 0 }, 
                new object []{ "WKHT", 0 }
            };
            await pipeline.InsertAsync(entries);
            var tinyObj = new TinyBizObject {IntProp = 1, StrProp = "WWWW"};
            await pipeline.InsertAsync(tinyObj);
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHT,0\r\nWWWW,1\r\n", str);
            if (flushCalled && waitForDataTimeout)
                _asyncMethodFinished.Set();
        }

        [TestMethod]
        public void TestManualToCsvAndDbBasicAsync()
        {
            _asyncMethodFinished = new ManualResetEventSlim(false);
            var pipeline = new BlockingQueueDataPipeline {AbortOnTargetAdapterException = true};
            TestManualToCsvBasicAsyncInternal(pipeline);
            Assert.IsTrue(_asyncMethodFinished.Wait(3000));
            pipeline.Finish(true);
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestMSSQLToSQLiteUsingSingleInsert()
        {
            using var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();
            using var cmd = new SQLiteCommand("DROP TABLE IF EXISTS TIME_BASE", conn);
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE TABLE TIME_BASE (CPCODE_EXP TEXT, NPAYCODE TEXT)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO TIME_BASE (CPCODE_EXP, NPAYCODE) VALUES (@CPCODE_EXP, @NPAYCODE)";
            using var sourceCmd = new SqlCommand("SELECT TOP 1000000 CPCODE_EXP, NPAYCODE FROM TIME", _conn);
            var targetAdapter = new SQLiteTargetAdapterCommand(cmd);
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(sourceCmd, targetAdapter);
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestMSSQLToSQLiteBulkAndBack()
        {
            using var conn = new SQLiteConnection(DataSourceSqLiteConnectionString);
            conn.Open();
            using var cmd = new SQLiteCommand("DROP TABLE IF EXISTS TIME_BASE", conn);
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE TABLE TIME_BASE (CPCODE_EXP TEXT, NPAYCODE TEXT)";
            cmd.ExecuteNonQuery();

            using var sourceCmd = new SqlCommand("SELECT TOP 10000 CPCODE_EXP, NPAYCODE FROM TIME ORDER BY CPCODE_EXP", _conn);
            var targetAdapter = new SQLiteAdapterBulkInsert("TIME_BASE", new[] {"CPCODE_EXP", "NPAYCODE"}, conn, 1000) {AbortOnProcessException = true};
            var pipeline = new SqlClientDataPipeline();
            pipeline.Pump(sourceCmd, targetAdapter, 5000);
            
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", _conn);
            truncateCmd.ExecuteNonQuery();
            
            using var conn2 = new SqlConnection(Settings.Default.SqlConnectionString);
            conn2.Open();
            using var sqlLiteSrc = new SQLiteCommand("SELECT CPCODE_EXP, NPAYCODE FROM TIME_BASE", conn);
            var backPipeline = new SQLiteDataPipeline();
            var targets = new []
            {
                new SqlClientAdapterBulkInsert("TIME_BASE", new[] {"CPCODE_EXP", "NPAYCODE"}, _conn, 300)
                    {AbortOnProcessException = true, UseTakeSemantics = true},
                new SqlClientAdapterBulkInsert("TIME_BASE", new[] {"CPCODE_EXP", "NPAYCODE"}, conn2, 300)
                    {AbortOnProcessException = true, UseTakeSemantics = true}
            };
            backPipeline.Pump(sqlLiteSrc, targets, 2000);
            Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(
                Settings.Default.SqlConnectionString, "SELECT TOP 10000 CPCODE_EXP, NPAYCODE FROM TIME ORDER BY CPCODE_EXP",
                Settings.Default.SqlConnectionString, "SELECT TOP 10000 CPCODE_EXP, NPAYCODE FROM TIME_BASE ORDER BY CPCODE_EXP");
        }
    }
}
