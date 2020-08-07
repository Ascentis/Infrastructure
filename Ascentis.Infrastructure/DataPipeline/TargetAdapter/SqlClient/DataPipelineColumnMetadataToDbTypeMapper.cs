using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient
{
    public class DataPipelineColumnMetadataToDbTypeMapper
    {
        public bool UseShortParam { get; set; }

        public void Map(IDictionary<string, int> columns, DataPipelineColumnMetadata[] metadatas, SqlParameterCollection target, string paramSuffix = "")
        {
            var index = 0;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var column in columns)
            {
                var meta = column.Value >= 0 ? metadatas[column.Value] : DataPipelineColumnMetadata.NullMeta;

                var param = target.Add((UseShortParam ? $"P{index++}" : column.Key) + paramSuffix, TypeToSqlDbType.From(meta.DataType));
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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

        public void Map(IDictionary<string, int> columns, DataPipelineColumnMetadata[] metadatas, SqlParameterCollection target, int batchCount)
        {
            for (var i = 0; i < batchCount; i++)
                // ReSharper disable once PossibleMultipleEnumeration
                Map(columns, metadatas, target, $"_{i}");
        }
    }
}
