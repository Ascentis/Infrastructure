using System.Collections.Generic;
using Ascentis.Infrastructure.DataStreamer.SourceAdapter;

namespace Ascentis.Infrastructure.DataStreamer
{
    public interface IDataStreamerSourceAdapter<TRow>
    {
        void ReleaseRow(TRow row);
        IEnumerable<TRow> RowsEnumerable { get; }
        int FieldCount { get; }
        DataStreamerColumnMetadata[] ColumnMetadatas { get; }
    }
}
