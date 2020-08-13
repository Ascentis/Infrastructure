using System.Collections;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Ascentis.Infrastructure.DataPipeline;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Manual;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base;
using Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Bulk;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class SqlConnectionExtensions
    {
        private static ISourceAdapter<PoolEntry<object[]>> SourceAdapterBuilder(IEnumerable parameters, ColumnMetadataList metadata)
        {
            return new BlockingQueueSourceAdapter(parameters) {ColumnMetadatas = metadata};
        }

        private static TargetAdapterSql TargetAdapterBuilder(string sqlStatement, ColumnMetadataList metadata,
            DbConnection connection, bool paramsAsList)
        {
            var columnNames = metadata.Select(meta => meta.ColumnName).ToList();
            return new SqlClientAdapterBulkCommand(sqlStatement, columnNames, (SqlConnection)connection) {ParamsAsList = paramsAsList};
        }

        public static SqlCommand CreateBulkQueryCommand(
            this SqlConnection connection, 
            string sqlStatement, 
            ColumnMetadataList metadata, 
            IEnumerable parameters,
            bool paramsAsList = false)
        {
            return (SqlCommand) DbConnectionExtensions.CreateBulkQueryCommand(connection, sqlStatement, metadata, parameters, paramsAsList, SourceAdapterBuilder, TargetAdapterBuilder);
        }

        public static SqlCommand CreateBulkQueryCommand(
            this SqlConnection connection, 
            string sqlStatement, 
            IEnumerable parameters,
            bool paramsAsList = true)
        {
            return (SqlCommand)DbConnectionExtensions.CreateBulkQueryCommand(connection, sqlStatement, parameters, paramsAsList, SourceAdapterBuilder, TargetAdapterBuilder);
        }
    }
}
