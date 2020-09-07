using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public abstract class ColumnMetadataToDbTypeMapper
    {
        protected abstract DbParameter AddParam(DbParameterCollection target, string name, int type);

        protected abstract int SqlTypeFromType(Type type);

        public void Map(MetaToParamSettings settings)
        {
            ArgsChecker.CheckForNull<ArgumentNullException>(new []
            {
                ArgsChecker.Arg(settings.Metadatas, nameof(settings.Metadatas)),
                ArgsChecker.Arg(settings.Columns, nameof(settings.Columns)),
                ArgsChecker.Arg(settings.ColumnToIndexMap, nameof(settings.ColumnToIndexMap)),
                ArgsChecker.Arg(settings.Target, nameof(settings.Target))
            });
            var index = 0;
            var ansiParameters = settings.AnsiStringParameters?.ToDictionary(
                ansiParam => ansiParam, 
                ansiParam => 0) ?? new Dictionary<string, int>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var columnName in settings.Columns)
            {
                if (!settings.ColumnToIndexMap.TryGetValue(columnName, out var columnIndex))
                    columnIndex = -1;
                var meta = columnIndex >= 0 ? settings.Metadatas[columnIndex] : ColumnMetadata.NullMeta;

                var param = AddParam(settings.Target, (settings.UseShortParam ? $"P{index++}" : columnName) + (settings.ParamSuffix ?? ""), SqlTypeFromType(meta.DataType));
                if (!string.IsNullOrEmpty(meta.ColumnName)
                    && (param.DbType == DbType.String || param.DbType == DbType.StringFixedLength)
                    && ansiParameters.ContainsKey(meta.ColumnName))
                {
                    param.DbType = param.DbType == DbType.String ? DbType.AnsiString : DbType.AnsiStringFixedLength;
                }

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

        public void Map(MetaToParamSettings settings, int batchCount)
        {
            for (var i = 0; i < batchCount; i++)
            {
                if (settings.UseDefaultSuffix)
                    settings.ParamSuffix = $"_{i}";
                Map(settings);
            }
        }
    }
}
