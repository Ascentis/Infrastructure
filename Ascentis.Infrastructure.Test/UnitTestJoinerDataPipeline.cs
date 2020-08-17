using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestJoinerDataPipeline
    {
        public const string ConnStr = "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;";
        public const string SqlText = "SELECT TOP 1000 * FROM TIME ORDER BY IID";

        [TestMethod]
        public void TestJoinerDataPipelineBasic()
        {
            var pipeline = new JoinerDataPipeline<PoolEntry<object[]>>();
            var source1 = new SqlClientSourceAdapter(ConnStr, SqlText) {RowsPoolSize = int.MaxValue};
            var source2 = new SqlClientSourceAdapter(ConnStr, SqlText) {RowsPoolSize = int.MaxValue};

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
