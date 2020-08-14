using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.DataReplicator.SqlClient;
using Ascentis.Infrastructure.DataReplicator.SQLite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestDataReplicator
    {
        [TestMethod]
        public void TestReadMetadata()
        {
            using var replicator = new SQLiteDataReplicator(
                "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                "Data Source=inmemorydb;mode=memory;cache=shared;journal_mode=WAL;") {ParallelismLevel = 2};
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.Prepare<SqlCommand, SqlConnection>();
            Assert.AreEqual(6, replicator.ColumnMetadataLists.Length);
            foreach (var metaList in replicator.ColumnMetadataLists)
            {
                Assert.AreNotEqual(0, metaList.Count);
                Assert.AreEqual(typeof(string), metaList[0].DataType);
            }
            replicator.UnPrepare();
        }

        [TestMethod]
        public void TestBasicReplicate()
        {
            using var replicator = new SQLiteDataReplicator(
                "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                "Data Source=inmemorydb;mode=memory;cache=shared;synchronous=Off;")
            { ParallelismLevel = 2 };
            replicator.AddSourceTable("SELECT * FROM SITES");
            replicator.AddSourceTable("TIME", "SELECT TOP 10000 * FROM TIME");
            replicator.AddSourceTable("A_TIMESHEET", "SELECT TOP 10000 * FROM A_TIMESHEET");
            replicator.AddSourceTable("A_SCHEDULE", "SELECT TOP 10000 * FROM A_SCHEDULE");
            replicator.AddSourceTable("PM_DIST", "SELECT TOP 10000 * FROM PM_DIST");
            replicator.AddSourceTable("PM_LOG", "SELECT TOP 10000 * FROM PM_LOG");
            replicator.AddSourceTable("AUDITLOG", "SELECT TOP 10000 * FROM AUDITLOG");
            replicator.AddSourceTable("APPROVPR", "SELECT TOP 10000 * FROM APPROVPR");
            replicator.ForceDropTable = true;
            replicator.Prepare<SqlCommand, SqlConnection>();
            replicator.Replicate<SqlClientSourceAdapter>(1000, 1);
            replicator.UnPrepare();
        }

        [TestMethod]
        public void TestReplicateMultipleRounds()
        {
            const string srcConnStr = "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;";

            using var replicator = new SQLiteDataReplicator(
                    srcConnStr,
                    "Data Source=inmemorydb;mode=memory;cache=shared;synchronous=Off;Pooling=True;")
            { ParallelismLevel = 2 };
            replicator.AddSourceTable("SITES", "SELECT * FROM SITES");
            replicator.AddSourceTable("TIME", "SELECT TOP 1000 * FROM TIME");
            replicator.AddSourceTable("A_TIMESHEET", "SELECT TOP 1000 * FROM A_TIMESHEET");
            replicator.AddSourceTable("A_SCHEDULE", "SELECT TOP 1000 * FROM A_SCHEDULE");
            replicator.AddSourceTable("PM_DIST", "SELECT TOP 1000 * FROM PM_DIST", new []
            {
                "CREATE INDEX PM_DIST_CEMPID ON PM_DIST(CEMPID)"
            });
            var connection = new SqlConnection(srcConnStr);
            var pmLogCmd = new SqlCommand("SELECT TOP 1000 * FROM PM_LOG", connection);
            replicator.AddSourceTable("PM_LOG", pmLogCmd); // Here we Add a command to the list of tables. command could be created with extension method CreateBulkQueryCommand
            replicator.AddSourceTable("AUDITLOG", "SELECT TOP 1000 * FROM AUDITLOG");
            replicator.AddSourceTable("APPROVPR", "SELECT TOP 1000 * FROM APPROVPR");
            replicator.Prepare<SqlCommand, SqlConnection>();
            Assert.AreEqual(8, replicator.SourceCommandCount);
            for (var i = 0; i < 5; i++)
            {
                // Can't use the Source Connection if a reader still open. Need to check readers before creating new commands
                // In this example we replace only the dataset index 1
                replicator.CloseReader(1);
                var cmd = ((SqlConnection) replicator.SourceConnections[1]).CreateBulkQueryCommand("SELECT TOP 10 * FROM TIME WHERE IID IN (@@@Params)", new object[] {1});
                replicator.SourceCommand[1] = cmd; // Every loop we can replace the source command with a new one linked to the new dataset we intend to replicate
                replicator.Replicate<SqlClientSourceAdapter>(1000, 1);
            }

            replicator.UnPrepare();
        }

        [TestMethod]
        public void TestBasicReplicateWithoutTransactions()
        {
            using var replicator = new SQLiteDataReplicator(
                    "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                    "Data Source=inmemorydb;mode=memory;cache=shared;synchronous=Off;")
            { ParallelismLevel = 2 };
            replicator.AddSourceTable("SITES", "SELECT * FROM SITES");
            replicator.UseTransaction = false;
            replicator.Prepare<SqlCommand, SqlConnection>();
            replicator.Replicate<SqlClientSourceAdapter>(1000, 1);
            replicator.UnPrepare();
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestBasicReplicateMSSQLToMSSQL()
        {
            using var replicator = new SqlClientDataReplicator(
                    "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                    "Server=vm-pc-sql02;Database=sbattig_test;Trusted_Connection=True;")
            { ParallelismLevel = 2 };
            replicator.AddSourceTable("SELECT TOP 1000 * FROM TIME");
            replicator.UseTransaction = false;
            replicator.Prepare<SqlCommand, SqlConnection>();
            replicator.Replicate<SqlClientSourceAdapter>(1000, 13);
            replicator.UnPrepare();
        }
    }
}
