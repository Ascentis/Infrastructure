using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.TestHelpers.AssertExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestDataPipelineAssertExtension
    {
        public const string ConnStr = "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;";
        public const string SqlText = "SELECT TOP 1000 * FROM TIME ORDER BY IID";
        public const string SqlText2 = "SELECT TOP 1001 * FROM TIME ORDER BY IID";
        public const string SqlText100K = "SELECT TOP 100000 * FROM TIME ORDER BY IID";
        public const string SqlText100K2 = "SELECT TOP 100100 * FROM TIME ORDER BY IID";

        [TestMethod]
        public void TestBasicDataPipelineAssertExtension()
        {
            Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(ConnStr, SqlText, ConnStr, SqlText);
            Assert.ThrowsException<AssertFailedException>(() => Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(ConnStr, SqlText2, ConnStr, SqlText));
            Assert.That.AreNotEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(ConnStr, SqlText2, ConnStr, SqlText);
        }

        [TestMethod]
        public void TestHighVolumeDataPipelineAssertExtension()
        {
            Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(ConnStr, SqlText100K, ConnStr, SqlText100K);
            Assert.ThrowsException<AssertFailedException>(() => Assert.That.AreEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(ConnStr, SqlText100K2, ConnStr, SqlText100K));
            Assert.That.AreNotEqual<SqlClientSourceAdapter, SqlClientSourceAdapter>(ConnStr, SqlText100K2, ConnStr, SqlText100K);
        }
    }
}
