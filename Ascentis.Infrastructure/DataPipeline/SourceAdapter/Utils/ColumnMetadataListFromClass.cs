namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils
{
    public class ColumnMetadataList<T> : ColumnMetadataList
    {
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
                    meta.ColumnSize = 255;
                else if (prop.PropertyType == typeof(decimal))
                {
                    meta.NumericPrecision = 17;
                    meta.NumericScale = 7;
                }
                Add(meta);
            }
        }
    }
}
