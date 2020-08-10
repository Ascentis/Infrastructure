using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.SqlClient.Utils
{
    public class ColumnMetadataToDbTypeMapper
    {
        public bool UseShortParam { get; set; }

        public void Map(IDictionary<string, int> columns, ColumnMetadata[] metadatas, IEnumerable<string> ansiStringParameters, SqlParameterCollection target, string paramSuffix = "")
        {
            var index = 0;
            var ansiParameters = ansiStringParameters?.ToDictionary(ansiParam => ansiParam, ansiParam => 0) ?? new Dictionary<string, int>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var column in columns)
            {
                var meta = column.Value >= 0 ? metadatas[column.Value] : ColumnMetadata.NullMeta;

                var param = target.Add((UseShortParam ? $"P{index++}" : column.Key) + paramSuffix, TypeToSqlDbType.From(meta.DataType));
                if (!string.IsNullOrEmpty(meta.ColumnName) && (param.DbType == DbType.String || param.DbType == DbType.StringFixedLength) && ansiParameters.ContainsKey(meta.ColumnName))
                    param.DbType = param.DbType == DbType.String ? DbType.AnsiString : DbType.AnsiStringFixedLength;

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

        public void Map(IDictionary<string, int> columns, ColumnMetadata[] metadatas, IEnumerable<string> ansiStringParameters, SqlParameterCollection target, int batchCount)
        {
            for (var i = 0; i < batchCount; i++)
                // ReSharper disable once PossibleMultipleEnumeration
                Map(columns, metadatas, ansiStringParameters, target, $"_{i}");
        }
    }
}
