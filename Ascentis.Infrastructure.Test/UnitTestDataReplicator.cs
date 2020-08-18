using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SQLite;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk;
using Ascentis.Infrastructure.DataReplicator.SqlClient;
using Ascentis.Infrastructure.DataReplicator.SQLite;
using Ascentis.Infrastructure.Test.Properties;
using Ascentis.Infrastructure.TestHelpers.AssertExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class UnitTestDataReplicator
    {
        // ReSharper disable once InconsistentNaming
        private const string SQLiteConnectionString = "FullUri=file:Client1?cache=shared&mode=memory;";
        private const string SelectFromSitesOrderByIid = "SELECT * FROM SITES ORDER BY IID";
        private const string SelectTop10000FromTimeOrderByIid = "SELECT TOP 10000 * FROM TIME ORDER BY IID";
        private const string SelectTop10000FromATimesheetOrderBySeq = "SELECT TOP 10000 * FROM A_TIMESHEET ORDER BY SEQ";
        private const string SelectTop10000FromAScheduleOrderBySeq = "SELECT TOP 10000 * FROM A_SCHEDULE ORDER BY SEQ";
        private const string SelectTop10000FromPmDistOrderByIid = "SELECT TOP 10000 * FROM PM_DIST ORDER BY IID";
        private const string SelectTop10000FromPmLogOrderByIid = "SELECT TOP 10000 * FROM PM_LOG ORDER BY IID";
        private const string SelectTop10000FromAuditlogOrderByIid = "SELECT TOP 10000 * FROM AUDITLOG ORDER BY IID";
        private const string SelectTop10000FromApprovprOrderByIid = "SELECT TOP 10000 * FROM APPROVPR ORDER BY IID";
        private const string SelectVersionAsSqlServerVersion = "SELECT @@VERSION AS 'SQL Server Version'";
        private const string SelectTop1000FromTimeOrderByIid = "SELECT TOP 1000 * FROM TIME ORDER BY IID";
        private const string SelectTop1000FromATimesheetOrderBySeq = "SELECT TOP 1000 * FROM A_TIMESHEET ORDER BY SEQ";
        private const string SelectTop1000FromAScheduleOrderBySeq = "SELECT TOP 1000 * FROM A_SCHEDULE ORDER BY SEQ";
        private const string SelectTop1000FromPmDistOrderByIid = "SELECT TOP 1000 * FROM PM_DIST ORDER BY IID";
        private const string SelectTop1000FromPmLogOrderByIid = "SELECT TOP 1000 * FROM PM_LOG ORDER BY IID";
        private const string SelectTop1000FromAuditlogOrderByIid = "SELECT TOP 1000 * FROM AUDITLOG ORDER BY IID";
        private const string SelectTop1000FromApprovprOrderByIid = "SELECT TOP 1000 * FROM APPROVPR ORDER BY IID";

        private const string TableNameSITES = "SITES";
        private const string TableNameTIME = "TIME";
        private const string TableNameATimesheet = "A_TIMESHEET";
        private const string TableNameASchedule = "A_SCHEDULE";
        private const string TableNamePM_DIST = "PM_DIST";
        private const string TableNamePM_LOG = "PM_LOG";
        private const string TableNameAUDITLOG = "AUDITLOG";
        private const string TableNameAPPROVPR = "APPROVPR";

        private const string SelectAllFromTimeOrderByIid = "SELECT * FROM TIME ORDER BY IID";
        private const string SelectAllFromAScheduleOrderBySeq = "SELECT * FROM A_SCHEDULE ORDER BY SEQ";
        private const string SelectAllFromPmDistOrderByIid = "SELECT * FROM PM_DIST ORDER BY IID";
        private const string SelectAllFromPmLogOrderByIid = "SELECT * FROM PM_LOG ORDER BY IID";
        private const string SelectAllFromAuditlogOrderByIid = "SELECT * FROM AUDITLOG ORDER BY IID";
        private const string SelectAllFromApprovprOrderByIid = "SELECT * FROM APPROVPR ORDER BY IID";
        private const string SelectAllFromATimesheetOrderBySeq = "SELECT * FROM A_TIMESHEET ORDER BY SEQ";

        [TestMethod]
        public void TestReadMetadata()
        {
            using var replicator = new SQLiteDataReplicator<SqlClientSourceAdapter>(
                Settings.Default.SqlConnectionString,
                SQLiteConnectionString) {ParallelismLevel = 2};
            replicator.AddSourceTable("", SelectVersionAsSqlServerVersion);
            replicator.AddSourceTable("", SelectVersionAsSqlServerVersion);
            replicator.AddSourceTable("", SelectVersionAsSqlServerVersion);
            replicator.AddSourceTable("", SelectVersionAsSqlServerVersion);
            replicator.AddSourceTable("", SelectVersionAsSqlServerVersion);
            replicator.AddSourceTable("", SelectVersionAsSqlServerVersion);
            replicator.Prepare();
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
            using var baseConn = new SQLiteConnection(SQLiteConnectionString);
            baseConn.Open();
            using var replicator = new SQLiteDataReplicator<SqlClientSourceAdapter>(
                Settings.Default.SqlConnectionString,
                SQLiteConnectionString)
            { ParallelismLevel = 2 };
            replicator.AddSourceTable(SelectFromSitesOrderByIid);
            replicator.AddSourceTable(SelectTop10000FromTimeOrderByIid);
            replicator.AddSourceTable(SelectTop10000FromATimesheetOrderBySeq);
            replicator.AddSourceTable(SelectTop10000FromAScheduleOrderBySeq);
            replicator.AddSourceTable(SelectTop10000FromPmDistOrderByIid);
            replicator.AddSourceTable(SelectTop10000FromPmLogOrderByIid);
            replicator.AddSourceTable(SelectTop10000FromAuditlogOrderByIid);
            replicator.AddSourceTable(SelectTop10000FromApprovprOrderByIid);
            replicator.ReplicateMode = SQLiteDataReplicator<SqlClientSourceAdapter>.ReplicateModes.DropTableAndPump;
            replicator.Prepare();
            replicator.Replicate(1000, 1);
            replicator.UnPrepare();
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectFromSitesOrderByIid,
                SQLiteConnectionString, SelectFromSitesOrderByIid);
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop10000FromTimeOrderByIid,
                SQLiteConnectionString, SelectAllFromTimeOrderByIid);
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop10000FromAScheduleOrderBySeq,
                SQLiteConnectionString, SelectAllFromAScheduleOrderBySeq);
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop10000FromPmDistOrderByIid,
                SQLiteConnectionString, SelectAllFromPmDistOrderByIid);
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop10000FromPmLogOrderByIid,
                SQLiteConnectionString, SelectAllFromPmLogOrderByIid);
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop10000FromAuditlogOrderByIid,
                SQLiteConnectionString, SelectAllFromAuditlogOrderByIid);
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop10000FromApprovprOrderByIid,
                SQLiteConnectionString, SelectAllFromApprovprOrderByIid);
        }

        [TestMethod]
        public void TestReplicateMultipleRounds()
        {
            var srcConnStr = Settings.Default.SqlConnectionString;

            using var replicator = new SQLiteDataReplicator<SqlClientSourceAdapter>(srcConnStr, SQLiteConnectionString)
            { ParallelismLevel = 2 };
            replicator.AddSourceTable(TableNameSITES, SelectFromSitesOrderByIid);
            replicator.AddSourceTable(TableNameTIME, SelectTop1000FromTimeOrderByIid);
            replicator.AddSourceTable(TableNameATimesheet, SelectTop1000FromATimesheetOrderBySeq);
            replicator.AddSourceTable(TableNameASchedule, SelectTop1000FromAScheduleOrderBySeq);
            replicator.AddSourceTable(TableNamePM_DIST, SelectTop1000FromPmDistOrderByIid, new []
            {
                "CREATE INDEX PM_DIST_CEMPID ON PM_DIST(CEMPID)"
            });
            var connection = new SqlConnection(srcConnStr);
            var pmLogCmd = new SqlCommand(SelectTop1000FromPmLogOrderByIid, connection);
            replicator.AddSourceTable(TableNamePM_LOG, pmLogCmd); // Here we Add a command to the list of tables. command could be created with extension method CreateBulkQueryCommand
            replicator.AddSourceTable(TableNameAUDITLOG, SelectTop1000FromAuditlogOrderByIid);
            replicator.AddSourceTable(TableNameAPPROVPR, SelectTop1000FromApprovprOrderByIid);
            replicator.Prepare();
            Assert.AreEqual(8, replicator.SourceCommandCount);
            for (var i = 0; i < 5; i++)
            {
                // Can't use the Source Connection if a reader still open. Need to check readers before creating new commands
                // In this example we replace only the dataset index 1
                replicator.CloseReader(1);
                var cmd = ((SqlConnection) replicator.SourceConnections[1]).CreateBulkQueryCommand("SELECT TOP 10 * FROM TIME WHERE IID IN (@@@Params) ORDER BY IID", new object[] { i + 18});
                replicator.SourceCommand[1] = cmd; // Every loop we can replace the source command with a new one linked to the new dataset we intend to replicate
                replicator.Replicate(1000, 1);
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, SelectFromSitesOrderByIid,
                    SQLiteConnectionString, SelectFromSitesOrderByIid);
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, $"SELECT TOP 10 * FROM TIME WHERE IID IN ({i + 18}) ORDER BY IID",
                    SQLiteConnectionString, $"SELECT * FROM TIME WHERE IID IN ({i + 18}) ORDER BY IID");
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, SelectTop1000FromATimesheetOrderBySeq,
                    SQLiteConnectionString, SelectAllFromATimesheetOrderBySeq);
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, SelectTop1000FromAScheduleOrderBySeq,
                    SQLiteConnectionString, SelectAllFromAScheduleOrderBySeq);
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, SelectTop1000FromPmDistOrderByIid,
                    SQLiteConnectionString, SelectAllFromPmDistOrderByIid);
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, SelectTop1000FromPmLogOrderByIid,
                    SQLiteConnectionString, SelectAllFromPmLogOrderByIid);
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, SelectTop1000FromAuditlogOrderByIid,
                    SQLiteConnectionString, SelectAllFromAuditlogOrderByIid);
                Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                    Settings.Default.SqlConnectionString, SelectTop1000FromApprovprOrderByIid,
                    SQLiteConnectionString, SelectAllFromApprovprOrderByIid);
            }

            replicator.UnPrepare();
        }

        [TestMethod]
        public void TestBasicReplicateWithoutTransactions()
        {
            using var baseConn = new SQLiteConnection(SQLiteConnectionString);
            baseConn.Open();
            using var replicator = new SQLiteDataReplicator<SqlClientSourceAdapter>(
                    Settings.Default.SqlConnectionString2ndDatabase,
                    SQLiteConnectionString)
            { ParallelismLevel = 2 };
            replicator.AddSourceTable(TableNameSITES, SelectFromSitesOrderByIid);
            replicator.UseTransaction = false;
            replicator.Prepare();
            replicator.ReplicateMode = SQLiteDataReplicator<SqlClientSourceAdapter>.ReplicateModes.DropTableAndPump;
            replicator.Replicate(1000, 1);
            replicator.UnPrepare();
            Assert.That.AreEqual<SqlClientSourceAdapter, SQLiteSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectFromSitesOrderByIid,
                SQLiteConnectionString, SelectFromSitesOrderByIid);
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestBasicReplicateMSSQLToMSSQL()
        {
            using var replicator = new SqlClientDataReplicator<SqlClientSourceAdapter>(
                    Settings.Default.SqlConnectionString,
                    Settings.Default.SqlConnectionString2ndDatabase)
            { ParallelismLevel = 2 };
            replicator.AddSourceTable(SelectTop1000FromTimeOrderByIid);
            replicator.UseTransaction = true;
            replicator.ReplicateMode = SqlClientDataReplicator<SqlClientSourceAdapter>.ReplicateModes.DropTableAndPump;
            replicator.Prepare();
            replicator.Replicate(3000, SqlClientAdapterBulkInsert.MaxPossibleBatchSize);
            replicator.UnPrepare();
            Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop1000FromTimeOrderByIid,
                Settings.Default.SqlConnectionString2ndDatabase, SelectAllFromTimeOrderByIid);
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void TestBasicReplicateMSSQLToMSSQLUsingLiteralParameterBinding()
        {
            using var replicator = new SqlClientDataReplicator<SqlClientSourceAdapter>(
                    Settings.Default.SqlConnectionString,
                    Settings.Default.SqlConnectionString2ndDatabase)
            { ParallelismLevel = 2 };
            replicator.AddSourceTable(SelectTop1000FromTimeOrderByIid);
            replicator.UseTransaction = true;
            replicator.ReplicateMode = SqlClientDataReplicator<SqlClientSourceAdapter>.ReplicateModes.DropTableAndPump;
            replicator.LiteralParamBinding = true;
            replicator.Prepare();
            replicator.Replicate(3000, 50);
            replicator.UnPrepare();
            Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(
                Settings.Default.SqlConnectionString, SelectTop1000FromTimeOrderByIid,
                Settings.Default.SqlConnectionString2ndDatabase, SelectAllFromTimeOrderByIid);
        }
    }
}
