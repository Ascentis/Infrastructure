using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient
{
    public class DataPipelineColumnMetadataToDbTypeMapper
    {
        public bool UseShortParam { get; set; }

        public void Map(IEnumerable<DataPipelineColumnMetadata> source, SqlParameterCollection target, string paramSuffix = "")
        {
            var index = 0;
            foreach(var meta in source)
            {
                var param = target.Add((UseShortParam ? $"P{index++}" : meta.ColumnName) + paramSuffix, TypeToSqlDbType.From(meta.DataType));
                switch (param.DbType)
                {
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.Binary:
                    case DbType.String:
                    case DbType.StringFixedLength:
                    case DbType.Xml:
                        param.Size = meta.ColumnSize ?? 0;
                        break;
                    case DbType.Decimal:
                        param.Precision = (byte)(meta.NumericPrecision ?? 0);
                        param.Scale = (byte)(meta.NumericScale ?? 0);
                        break;
                }
            }
        }

        public void Map(IEnumerable<DataPipelineColumnMetadata> source, SqlParameterCollection target, int batchCount)
        {
            for (var i = 0; i < batchCount; i++)
                // ReSharper disable once PossibleMultipleEnumeration
                Map(source, target, $"_{i}");
        }
    }
}
