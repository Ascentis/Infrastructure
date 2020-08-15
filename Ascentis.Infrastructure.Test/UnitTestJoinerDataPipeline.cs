using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestJoinerDataPipeline
    {
        [TestMethod]
        public void TestJoinerDataPipelineBasic()
        {
            using var conn1 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");
            using var conn2 = new SqlConnection("Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;");

            conn1.Open();
            conn2.Open();

            using var cmd1 = new SqlCommand("SELECT TOP 1000 * FROM TIME ORDER BY IID", conn1);
            using var cmd2 = new SqlCommand("SELECT TOP 1000 * FROM TIME ORDER BY IID", conn2);

            using var reader1 = cmd1.ExecuteReader();
            using var reader2 = cmd2.ExecuteReader();

            var pipeline = new JoinerDataPipeline<PoolEntry<object[]>>();
            var source1 = new SqlClientSourceAdapter(reader1) {RowsPoolSize = int.MaxValue};
            var source2 = new SqlClientSourceAdapter(reader2) {RowsPoolSize = int.MaxValue};

            var cnt = 0;
            pipeline.Pump(new ISourceAdapter<PoolEntry<object[]>>[] {source1, source2}, rowsRowsList =>
            {
                foreach (var entry in rowsRowsList)
                {
                    if (cnt == 0)
                        cnt = entry.Value.Length;
                    Assert.AreEqual(cnt, entry.Value.Length);
                }

                for (var i = 0; i < cnt; i++)
                {
                    var firstValue = rowsRowsList[0].Value[i];
                    for (var j = 1; j < rowsRowsList.Count; j++)
                        Assert.AreEqual(firstValue, rowsRowsList[j].Value[i]);
                }
                //Assert.Fail("error");
            });
            Assert.AreNotEqual(0, cnt);
        }
    }
}
