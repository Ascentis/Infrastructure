using System;
using Ascentis.Infrastructure.DataPipeline.Exceptions;
using Ascentis.Infrastructure.DataPipeline.Exceptions.DataPipelineComparer;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.Generic;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipelineComparer
    {
        private const string AdaptersOutOfBalance = "Comparison failed. SourceAdapters don't return same number of objects";

        public int RowCount { get; private set; }

        private static void CheckAggregateException(AggregateException exceptions)
        {
            foreach (var e in exceptions.InnerExceptions)
            {
                switch (e)
                {
                    case DataPipelineAbortedException _:
                        throw new DataPipelineComparerOutOfBalance(AdaptersOutOfBalance);
                    case AggregateException aggregateException:
                        CheckAggregateException(aggregateException);
                        break;
                    case DataPipelineComparerException exception:
                        throw exception;
                }
            }
        }

        public void CheckEqual<TSourceAdapter1, TSourceAdapter2>(
            string connStr1, string sqlText1, string connStr2, string sqlText2)
            where TSourceAdapter1 : SourceAdapterSqlBase
            where TSourceAdapter2 : SourceAdapterSqlBase
        {
            var pipeline = new JoinerDataPipeline<PoolEntry<object[]>>();
            var source1 = GenericObjectBuilder.Build<TSourceAdapter1>(connStr1, sqlText1);
            source1.RowsPoolSize = int.MaxValue;
            var source2 = GenericObjectBuilder.Build<TSourceAdapter2>(connStr2, sqlText2);
            source2.RowsPoolSize = int.MaxValue;

            var columnCount = 0;
            RowCount = 0;
            try
            {
                pipeline.Pump(new ISourceAdapter<PoolEntry<object[]>>[] { source1, source2 }, rowsRowsList =>
                {
                    foreach (var entry in rowsRowsList)
                    {
                        if (columnCount == 0)
                            columnCount = entry.Value.Length;
                        if (columnCount != entry.Value.Length)
                            throw new DataPipelineComparerColumnCountMismatch(columnCount, entry.Value.Length);
                    }

                    for (var i = 0; i < columnCount; i++)
                    {
                        var firstValue = rowsRowsList[0].Value[i];
                        for (var j = 1; j < rowsRowsList.Count; j++)
                            if (!firstValue.Equals(rowsRowsList[j].Value[i]))
                                throw new DataPipelineComparerDataMismatch(firstValue, rowsRowsList[j].Value[i]);
                    }

                    RowCount++;
                });
            }
            catch (AggregateException exceptions)
            {
                CheckAggregateException(exceptions);
                throw;
            }
            catch (DataPipelineAbortedException)
            {
                throw new DataPipelineComparerOutOfBalance(AdaptersOutOfBalance);
            }
        }
    }
}
