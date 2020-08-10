using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Manual;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Text;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Bulk;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Single;
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
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterDelimited(stream));
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
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterFixedLength(stream) {FieldSizes = new []{6, 4}});
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
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterFixedLength(stream));
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
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterFixedLength(stream)
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
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterFixedLength(stream) { FieldSizes = new[] { -6, -4 } });
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
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterFixedLength(stream)
            {
                FieldSizes = new[] { 3, 4 },
                OverflowStringFieldWidthBehaviors = new[]
                {
                    TargetAdapterFixedLength.OverflowStringFieldWidthBehavior.Truncate,
                    TargetAdapterFixedLength.OverflowStringFieldWidthBehavior.Truncate
                }
            });
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKH   0\r\nWKH   0\r\n", str);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedpipelinerorOnOverflowString()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new DataPipelineSql();
            pipeline.AbortOnTargetAdapterException = true;
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => 
                pipeline.Pump(cmd, new TargetAdapterFixedLength(stream) {FieldSizes = new[] { 3, 4 }})).InnerException is DataPipelineException);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedpipelinerorOnOverflowOnInt()
        {
            using var cmd = new SqlCommand("SELECT TRIM(CPCODE_EXP), NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new DataPipelineSql();
            pipeline.AbortOnTargetAdapterException = true;
            // ReSharper disable once AccessToDisposedClosure
            Assert.IsTrue(Assert.ThrowsException<ConveyorException>(() => pipeline.Pump(cmd, new TargetAdapterFixedLength(stream)
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
            var pipeline = new DataPipelineSql();
            Assert.ThrowsException<DataPipelineException>(() => pipeline.Pump(cmd, new TargetAdapterFixedLength(stream) { FieldSizes = new []{0}}));
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void TestSqlToFixedStreamThrowsExceptionOnBufferSizes()
        {
            using var cmd = new SqlCommand("SELECT TOP 10000 TRIM(CPCODE_EXP), NPAYCODE FROM TIME", _conn);
            using var targetConn0 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn0.Open();
            var pipeline = new DataPipelineSql();
            Assert.IsTrue(Assert.ThrowsException<DataPipelineException>(() => pipeline.Pump(cmd,
                new TargetAdapterBulkInsert("TIME_BASE", new[] {"CPCODE_EXP", "NPAYCODE"}, targetConn0, 1000), 10)).Message.Contains("Deadlock"));
            Assert.IsTrue(Assert.ThrowsException<TargetAdapterException>(() => pipeline.Pump(cmd,
                new TargetAdapterBulkInsert("TIME_BASE", new[] {"CPCODE_EXP", "NPAYCODE"}, targetConn0, 1500), 10000)).Message.Contains("exceeds"));
        }

        [TestMethod]
        public void TestSqlToCsvStreamWithHeaders()
        {
            using var cmd = new SqlCommand("SELECT CPCODE_EXP, NPAYCODE FROM TIME WHERE IID BETWEEN 18 AND 36", _conn);
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterDelimited(stream) { OutputHeaders = true });
            stream.Flush();
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("CPCODE_EXP,NPAYCODE\r\nWKHR,0\r\nWKHR,0\r\n", str);
        }

        [TestMethod]
        public void TestSqlToCsvStreamToFile()
        {
            using var cmd = new SqlCommand( "SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, TPDATE, TRIM(CGROUP6), TRIM(CGROUP7), NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterDelimited(fileStream));
        }

        [TestMethod]
        public void TestFixedLengthToCsvBasic()
        {
            var reader = new StringReader("WKHR               0\r\nWKHR               0\r\n");
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new DataPipelineFixedLength();
            pipeline.Pump(reader, new []
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
            }, new TargetAdapterDelimited(stream));
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
            var pipeline = new DataPipelineFixedLength();
            pipeline.OnSourceAdapterRowReadError += (adapter, sourceObject, e) =>
            {
                exceptionCalled = (string)sourceObject == "WKHR-               A";
                Assert.AreEqual("src", adapter.Id);
            };
            pipeline.Pump(reader, new[]
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
            }, new TargetAdapterDelimited(stream));
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
            var pipeline = new DataPipelineFixedLength();
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue(Assert.ThrowsException<DataPipelineException>(() => pipeline.Pump(reader, new[]
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
                }, new TargetAdapterDelimited(stream))).Message.Contains("Field position can't"));
            Assert.IsTrue(Assert.ThrowsException<NullReferenceException>(() => pipeline.Pump(reader, new[]
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
            }, new TargetAdapterDelimited(stream))).Message.Contains("ColumnSize[0]"));
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestSqlToFixedStreamToFile1MMRowsAndToCsv()
        {

            using var cmd = new SqlCommand("SELECT TOP 1000000 CPCODE_EXP, NPAYCODE, DWORKDATE, NRATE FROM TIME", _conn);
            using var fileStream = new FileStream("T:\\dump.txt", FileMode.Create, FileAccess.ReadWrite);
            //using var stream = new BufferedStream(fileStream, 1024 * 1024);
            var pipeline = new DataPipelineSql();
            pipeline.Pump(cmd, new TargetAdapterFixedLength(fileStream) {FieldSizes = new []{40, 40, 20, 20}});
            fileStream.Close();
            using var fileStreamSource = new FileStream("T:\\dump.txt", FileMode.Open, FileAccess.Read);
            using var fileSourceReader = new StreamReader(fileStreamSource);
            using var targetFileStream = new FileStream("T:\\dump2.txt", FileMode.Create, FileAccess.ReadWrite);
            var textPipeline = new DataPipelineFixedLength();
            textPipeline.Pump(new SourceAdapterFixedLength(fileSourceReader)
            {
                ColumnMetadatas = new []
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
            }, new TargetAdapterDelimited(targetFileStream));
        }

        [TestMethod]
        public void TestCsvToFixedBasic()
        {
            var reader = new StringReader("WKHR;;0\r\nWKHR;;0\r\n");
            var buf = new byte[1000];
            using var stream = new MemoryStream(buf);
            var pipeline = new DataPipelineDelimited();
            pipeline.Pump(reader, new[]
            {
                new ColumnMetadata
                {
                    DataType = typeof(string)
                },
                new ColumnMetadata
                {
                    DataType = typeof(int)
                }
            }, new TargetAdapterFixedLength(stream), ";;");
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

            var targets = new TargetAdapterText[]
                {
                    new TargetAdapterDelimited(stream), 
                    new TargetAdapterFixedLength(stream2) {FieldSizes = new []{6, 4}}
                };
            var pipeline = new DataPipelineSql();

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
            var pipeline = new DataPipelineSql();
            var targets = new TargetAdapterText[]
            {
                new TargetAdapterDelimited(fileStream),
                new TargetAdapterFixedLength(fileStream2) {FieldSizes = new []{40, 40, 20, 20, 30, 30, 30}}
            };
            pipeline.Pump(cmd, targets);
        }

        [TestMethod]
        public void TestSqlToSqlBasic()
        {
            using var cmd = new SqlCommand("SELECT TOP 100 CPCODE_EXP, NPAYCODE FROM TIME", _conn);
            using var targetConn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn.Open();
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();
            using var targetCmd = new SqlCommand("INSERT INTO TIME_BASE (CPCODE_EXP, NPAYCODE) VALUES (@CPCODE_EXP, @NPAYCODE)", targetConn);
            var pipeline = new DataPipelineSql {AbortOnTargetAdapterException = true};
            var targetAdapter = new TargetAdapterSqlCommand(targetCmd) {AnsiStringParameters = new[] {"CPCODE_EXP"}};
            pipeline.Pump(cmd, targetAdapter);
        }

        [TestMethod]
        public void TestSqlToSqlCommitEvery10Rows()
        {
            using var cmd = new SqlCommand("SELECT TOP 100 CPCODE_EXP, NPAYCODE FROM TIME", _conn);
            
            using var targetConn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn.Open();
            
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();

            var transaction = targetConn.BeginTransaction();
            
            TargetAdapterSqlCommand targetAdapter = null;
            using var targetCmd = new SqlCommand("INSERT INTO TIME_BASE (CPCODE_EXP, NPAYCODE) VALUES (@CPCODE_EXP, @NPAYCODE)", targetConn, transaction);
            
            try
            {
                targetAdapter = new TargetAdapterSqlCommand(targetCmd);
                var pipeline = new DataPipelineSql {AbortOnTargetAdapterException = true};
                var counter = 0;
                pipeline.OnReleaseRowToSourceAdapter += (adapter, row) =>
                {
                    if (++counter % 10 != 0)
                        return;
                    counter = 0;
                    var adapterSqlCommand = (ITargetAdapterSql) adapter;
                    adapterSqlCommand.Transaction.Commit();
                    adapterSqlCommand.Transaction = adapterSqlCommand.Connection.BeginTransaction();
                };

                pipeline.Pump(cmd, targetAdapter);
                targetAdapter.Transaction?.Commit();
            }
            catch (Exception)
            {
                targetAdapter?.Transaction?.Rollback();
                throw;
            }
        }

        [TestMethod]
        public void TestSqlToSqlBulkInsert()
        {
            using var cmd = new SqlCommand("SELECT TOP 40015 CEMPID, NPAYCODE, DWORKDATE, TIN, TOUT FROM TIME", _conn);
            
            using var targetConn0 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn0.Open();
            using var targetConn1 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn1.Open();
            using var targetConn2 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn2.Open();
            using var targetConn3 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn3.Open();

            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var pipeline = new DataPipelineSql {AbortOnTargetAdapterException = true};

            var outPipes = new []
            { 
                new TargetAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn0, 300) {UseTakeSemantics = true},
                new TargetAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn1, 300) {UseTakeSemantics = true},
                new TargetAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn2, 300) {UseTakeSemantics = true},
                new TargetAdapterBulkInsert("TIME_BASE", new [] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn3, 300) {UseTakeSemantics = true}
            };
            // ReSharper disable once RedundantArgumentDefaultValue
            pipeline.Pump(cmd, outPipes, 2400);
        }

        private static readonly LocalDataStoreSlot CounterSlot = Thread.AllocateDataSlot();

        [TestMethod]
        public void TestSqlToSqlBulkInsertWithTransaction()
        {
            using var cmd = new SqlCommand("SELECT TOP 40015 CEMPID, NPAYCODE, DWORKDATE, TIN, TOUT FROM TIME", _conn);

            using var targetConn0 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn0.Open();
            using var targetConn1 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn1.Open();

            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var transaction0 = targetConn0.BeginTransaction();
            var transaction1 = targetConn1.BeginTransaction();
            var outPipes = new[]
            {
                new TargetAdapterBulkInsert("TIME_BASE",
                        new[] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn0, 300)
                    {UseTakeSemantics = true, Transaction = transaction0},
                new TargetAdapterBulkInsert("TIME_BASE",
                        new[] {"CEMPID", "NPAYCODE", "DWORKDATE", "CPAYTYPE", "TIN", "TOUT"}, targetConn1, 300)
                    {UseTakeSemantics = true, Transaction = transaction1}
            };
            try
            {
                var pipeline = new DataPipelineSql {AbortOnTargetAdapterException = true};
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
                    var adapterBulk = (ITargetAdapterBulk) adapter;
                    adapterBulk.Flush();
                    adapterBulk.Transaction.Commit();
                    adapterBulk.Transaction = adapterBulk.Connection.BeginTransaction();
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
            using var targetConn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn.Open();
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn);
            truncateCmd.ExecuteNonQuery();

            var pipeline1 = new DataPipelineSql { AbortOnTargetAdapterException = true };
            pipeline1.Pump(cmd, new TargetAdapterBulkInsert("TIME_BASE", new []{"CEMPID", "CPAYTYPE"}, targetConn, 500), 2000);

            using var cmd2 = new SqlCommand(@"SELECT TOP 10000 CEMPID, LCALCULATE CPAYTYPE FROM TIME ORDER BY CEMPID", _conn);
            var pipeline2 = new DataPipelineSql { AbortOnTargetAdapterException = true };
            pipeline2.Pump(cmd2, new TargetAdapterBulkSqlCommand(@"
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

            pipeline2.Pump(cmd2, new TargetAdapterBulkSqlCommand(@"
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
        public void TestManualToCsvBasic()
        {
            var buf = new byte[1000];
            var stream = new MemoryStream(buf);
            var source = new SourceAdapterBlockingQueue
            {
                ColumnMetadatas = new[]
                {
                    new ColumnMetadata {DataType = typeof(string), ColumnSize = 4},
                    new ColumnMetadata {DataType = typeof(int), ColumnSize = 16}
                }
            };
            var pipeline = new DataPipelineBlockingQueue();
            Assert.IsTrue(ThreadPool.QueueUserWorkItem(obj => pipeline.Pump(source, new TargetAdapterDelimited(stream))));
            var entry = new object[] {"WKHR", 0};
            pipeline.Insert(new List<object[]>{entry, entry});
            pipeline.Finish(true);
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHR,0\r\n", str);
        }

        private ManualResetEventSlim _asyncMethodFinished;

        private async void TestManualToCsvBasicAsyncInternal(DataPipelineBlockingQueue pipeline)
        {
            using var targetConn0 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            targetConn0.Open();
            using var truncateCmd = new SqlCommand("TRUNCATE TABLE TIME_BASE", targetConn0);
            truncateCmd.ExecuteNonQuery();

            var buf = new byte[1000];
            var stream = new MemoryStream(buf);
            var source = new SourceAdapterBlockingQueue
            {
                ColumnMetadatas = new[]
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
                new TargetAdapterBulkInsert("TIME_BASE",
                        new[] {"CEMPID", "NPAYCODE"}, targetConn0, 300),
                new TargetAdapterDelimited(stream)
            };
            var flushCalled = false;
            var flushSignal = new object [0];
            pipeline.BeforeTargetAdapterProcessRow += (targetAdapter, row) =>
            {
                if (row.Value != flushSignal) 
                    return TargetAdapter.BeforeProcessRowResult.Continue;
                if (!(targetAdapter is ITargetAdapterBulk bulk)) 
                    return TargetAdapter.BeforeProcessRowResult.Abort;
                flushCalled = true;
                bulk.Flush();
                return TargetAdapter.BeforeProcessRowResult.Abort;
            };
            var waitForDataTimeout = false;
            source.OnWaitForDataTimeout += adapter =>
            {
                waitForDataTimeout = true;
                pipeline.Insert(flushSignal); // We need to Flush() in the target conveyor thread
            };
            ThreadPool.QueueUserWorkItem(obj => pipeline.Pump(source, targetAdapters));
            var entries = new List<object[]>
            {
                new object []{ "WKHR", 0 }, 
                new object []{ "WKHT", 0 }
            };
            await pipeline.InsertAsync(entries);
            var str = Encoding.UTF8.GetString(buf, 0, (int)stream.Position);
            Assert.AreEqual("WKHR,0\r\nWKHT,0\r\n", str);
            if (flushCalled && waitForDataTimeout)
                _asyncMethodFinished.Set();
        }

        [TestMethod]
        public void TestManualToCsvAndDbBasicAsync()
        {
            _asyncMethodFinished = new ManualResetEventSlim(false);
            var pipeline = new DataPipelineBlockingQueue {AbortOnTargetAdapterException = true};
            TestManualToCsvBasicAsyncInternal(pipeline);
            Assert.IsTrue(_asyncMethodFinished.Wait(2000));
            pipeline.Finish(true);
        }
    }
}
