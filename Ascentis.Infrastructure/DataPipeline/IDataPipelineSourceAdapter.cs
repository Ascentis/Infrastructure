using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface IDataPipelineSourceAdapter<TRow>
    {
        void ReleaseRow(TRow row);
        IEnumerable<TRow> RowsEnumerable { get; }
        int FieldCount { get; }
        DataPipelineColumnMetadata[] ColumnMetadatas { get; }
    }
}
