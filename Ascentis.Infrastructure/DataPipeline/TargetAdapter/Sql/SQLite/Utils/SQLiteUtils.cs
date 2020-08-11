namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.SQLite.Utils
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteUtils
    {
        public const int DefaultBatchSize = 100;
        // ReSharper disable once InconsistentNaming
        public const int DefaultMaxSQLiteParams = 2100;

        public static readonly ColumnMetadataToSQLiteDbTypeMapper ParamMapper =
            new ColumnMetadataToSQLiteDbTypeMapper
            {
                UseShortParam = true
            };
    }
}
