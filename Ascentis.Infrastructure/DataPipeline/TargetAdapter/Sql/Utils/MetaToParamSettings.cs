using System.Collections.Generic;
using System.Data.Common;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Sql.Utils
{
    public class MetaToParamSettings
    {
        public IEnumerable<string> Columns { get; set; }
        public IDictionary<string, int> ColumnToIndexMap { get; set; }
        public ColumnMetadataList Metadatas { get; set; }
        public IEnumerable<string> AnsiStringParameters { get; set; }
        public DbParameterCollection Target { get; set; }
        public bool UseShortParam { get; set; }
        public bool UseDefaultSuffix { get; set; } = true;
        public string? ParamSuffix { get; set; }
    }
}