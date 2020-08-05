using System.Collections.Generic;
using Ascentis.Infrastructure.DataStreamer.SourceAdapter;

namespace Ascentis.Infrastructure.DataStreamer
{
    public interface IDataStreamerSourceAdapter
    {
        void ReleaseRow(object[] row);
        IEnumerable<object[]> RowsEnumerable { get; }
        int FieldCount { get; }
        DataStreamerColumnMetadata[] ColumnMetadatas { get; }
    }
}
