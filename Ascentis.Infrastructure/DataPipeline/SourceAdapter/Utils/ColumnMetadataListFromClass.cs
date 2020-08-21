namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils
{
    public class ColumnMetadataList<T> : ColumnMetadataList
    {
        public const int DefaultStringColumnSize = 255;
        public const int DefaultDecimalPrecision = 20;
        public const int DefaultDecimalScale = 7;

        public ColumnMetadataList()
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                var meta = new ColumnMetadata
                {
                    DataType = prop.PropertyType,
                    ColumnName = prop.Name
                };
                if (prop.PropertyType == typeof(string))
                    meta.ColumnSize = DefaultStringColumnSize;
                else if (prop.PropertyType == typeof(decimal))
                {
                    meta.NumericPrecision = DefaultDecimalPrecision;
                    meta.NumericScale = DefaultDecimalScale;
                }
                Add(meta);
            }
        }
    }
}
