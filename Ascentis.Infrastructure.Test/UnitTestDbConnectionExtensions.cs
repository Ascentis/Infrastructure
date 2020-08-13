using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestDbConnectionExtensions
    {
        [TestMethod]
        public void TestBasicCreateCmd()
        {
            using var conn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            conn.Open();
            using var cmd = conn.CreateBulkQueryCommand("SELECT * FROM TIME T INNER JOIN (SELECT IID FROM (/*<DATA>*/ SELECT 1 /*</DATA>*/) SRC) SRC ON T.IID = SRC.IID", new ColumnMetadataList
            {
                new ColumnMetadata
                {
                    ColumnName = "IID",
                    DataType = typeof(int)
                }
            }, new object[]{18});
            Assert.IsNotNull(cmd);
            using var reader = cmd.ExecuteReader();
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(18, reader.GetInt32(0));
            Assert.IsFalse(reader.Read());
        }

        [TestMethod]
        public void TestBasicCreateCmdWithDoubleArray()
        {
            using var conn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            conn.Open();
            using var cmd = conn.CreateBulkQueryCommand("SELECT * FROM TIME T INNER JOIN (SELECT N, IID FROM (/*<DATA>*/ SELECT 1, 2 /*</DATA>*/) SRC) SRC ON T.IID = SRC.IID", new ColumnMetadataList
            {
                new ColumnMetadata
                {
                    ColumnName = "N",
                    DataType = typeof(int)
                },
                new ColumnMetadata
                {
                    ColumnName = "IID",
                    DataType = typeof(int)
                }
            }, new object[] {new object[] {1, 18}});
            Assert.IsNotNull(cmd);
            using var reader = cmd.ExecuteReader();
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(18, reader.GetInt32(0));
            Assert.IsFalse(reader.Read());
        }

        [TestMethod]
        public void TestBasicCreateCmdWithInList()
        {
            using var conn = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            conn.Open();
            using var cmd = conn.CreateBulkQueryCommand("SELECT * FROM TIME T WHERE IID IN (@@@Params)", new object[] { 1, 18, -1 });
            Assert.IsNotNull(cmd);
            using var reader = cmd.ExecuteReader();
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(18, reader.GetInt32(0));
            Assert.IsFalse(reader.Read());
        }
    }
}
