using System;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.TestHelpers.AssertExtension
{
    public static class DataPipelineAssertExtension
    {
        private const string AdaptersOutOfBalance = "Comparison failed. SourceAdapters don't return same number of objects";

        private static void CheckAggregateException(AggregateException exceptions)
        {
            foreach (var e in exceptions.InnerExceptions)
            {
                switch (e)
                {
                    case DataPipelineAbortedException _:
                        Assert.Fail(AdaptersOutOfBalance);
                        break;
                    case AggregateException aggregateException:
                        CheckAggregateException(aggregateException);
                        break;
                }
            }
        }

        public static void AreEqual<TSourceAdapter1, TSourceAdapter2>(
            this Assert assert, string connStr1, string sqlText1, string connStr2, string sqlText2)
            where TSourceAdapter1 : SourceAdapterSqlBase
            where TSourceAdapter2 : SourceAdapterSqlBase
        {
            var pipeline = new JoinerDataPipeline<PoolEntry<object[]>>();
            var source1 = GenericObjectBuilder.Build<TSourceAdapter1>(connStr1, sqlText1);
            source1.RowsPoolSize = int.MaxValue;
            var source2 = GenericObjectBuilder.Build<TSourceAdapter2>(connStr2, sqlText2);
            source2.RowsPoolSize = int.MaxValue;

            var columnCount = 0;
            try
            {
                pipeline.Pump(new ISourceAdapter<PoolEntry<object[]>>[] {source1, source2}, rowsRowsList =>
                {
                    foreach (var entry in rowsRowsList)
                    {
                        if (columnCount == 0)
                            columnCount = entry.Value.Length;
                        Assert.AreEqual(columnCount, entry.Value.Length);
                    }

                    for (var i = 0; i < columnCount; i++)
                    {
                        var firstValue = rowsRowsList[0].Value[i];
                        for (var j = 1; j < rowsRowsList.Count; j++)
                            Assert.AreEqual(firstValue, rowsRowsList[j].Value[i]);
                    }
                });
            }
            catch (AggregateException exceptions)
            {
                CheckAggregateException(exceptions);
                throw;
            }
            catch (DataPipelineAbortedException)
            {
                Assert.Fail(AdaptersOutOfBalance);
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
