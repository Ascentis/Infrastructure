using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.Exceptions.DataPipelineComparer;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.TestHelpers.AssertExtension
{
    public static class DataPipelineAssertExtension
    {
        public static void AreEqual<TSourceAdapter1, TSourceAdapter2>(
            this Assert assert, string connStr1, string sqlText1, string connStr2, string sqlText2)
            where TSourceAdapter1 : SourceAdapterSqlBase
            where TSourceAdapter2 : SourceAdapterSqlBase
        {
            var comparer = new DataPipelineComparer();
            try
            {
                comparer.CheckEqual<TSourceAdapter1, TSourceAdapter2>(connStr1, sqlText1, connStr2, sqlText2);
            }
            catch (DataPipelineComparerException exception)
            {
                Assert.Fail(exception.Message);
            }
        }

        public static void AreNotEqual<TSourceAdapter1, TSourceAdapter2>(
            this Assert assert, string connStr1, string sqlText1, string connStr2, string sqlText2)
            where TSourceAdapter1 : SourceAdapterSqlBase
            where TSourceAdapter2 : SourceAdapterSqlBase
        {
            Assert.ThrowsException<AssertFailedException>(() => AreEqual<TSourceAdapter1, TSourceAdapter2>(assert, connStr1, sqlText1, connStr2, sqlText2));
        }
    }
}
