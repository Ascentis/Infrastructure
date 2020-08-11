namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SqlClient.Utils
{
    public class SqlClientUtils
    {
        public const int DefaultBatchSize = 100;
        // ReSharper disable once InconsistentNaming
        public const int MaxMSSQLParams = 2100;

        public static readonly ColumnMetadataToSqlDbTypeMapper ParamMapper =
            new ColumnMetadataToSqlDbTypeMapper
            {
                UseShortParam = true
            };
    }
}
