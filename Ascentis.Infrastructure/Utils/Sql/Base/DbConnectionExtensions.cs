using System;
using System.Collections;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public delegate ISourceAdapter<PoolEntry<object[]>> SourceAdapterBuilderDelegate(IEnumerable parameters, ColumnMetadataList metadata);
    public delegate TargetAdapterSql TargetAdapterBuilderDelegate(string sqlStatement, ColumnMetadataList metadata, DbConnection connection, bool paramsAsList);

    public static class DbConnectionExtensions
    { 
        public static DbCommand CreateBulkQueryCommand(
            DbConnection connection,
            string sqlStatement,
            ColumnMetadataList metadata,
            IEnumerable parameters,
            bool paramsAsList,
            SourceAdapterBuilderDelegate sourceAdapterBuilder,
            TargetAdapterBuilderDelegate adapterBuilder)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata)); 
            if (sourceAdapterBuilder == null)
                throw new ArgumentNullException(nameof(sourceAdapterBuilder));
            if (adapterBuilder == null)
                throw new ArgumentNullException(nameof(adapterBuilder));

            var sourceAdapter = sourceAdapterBuilder(parameters, metadata);
            var adapter = adapterBuilder(sqlStatement, metadata, connection, paramsAsList);
            adapter.Prepare(sourceAdapter);
            try
            {
                foreach (var row in sourceAdapter.RowsEnumerable)
                    adapter.Process(row);
                adapter.BindParameters();
                return adapter.TakeCommand();
            }
            finally
            {
                adapter.UnPrepare();
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static DbCommand CreateBulkQueryCommand(
            DbConnection connection,
            string sqlStatement,
            IEnumerable parameters,
            bool paramsAsList,
            SourceAdapterBuilderDelegate sourceAdapterBuilder,
            TargetAdapterBuilderDelegate adapterBuilder)
        {
            ColumnMetadataList metadata = null;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var param in parameters)
            {
                metadata = new ColumnMetadataList {new ColumnMetadata
                {
                    ColumnName = "P",
                    DataType = param.GetType()
                }};
                break;
            }
            if (metadata == null)
                throw new InvalidOperationException("parameters enumerable is empty");

            return CreateBulkQueryCommand(connection, sqlStatement, metadata, parameters, paramsAsList, sourceAdapterBuilder, adapterBuilder);
        }
    }
}
