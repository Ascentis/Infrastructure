using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IStreamerSourceAdapter
    {
        void ReleaseRow(object[] row);
        IEnumerable<object[]> GetRowsEnumerable();
        int FieldCount { get; }
        ColumnMetadata[] ColumnMetadatas { get; }
    }
}
