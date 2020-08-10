using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface ISourceAdapter<TRow> : IAdapter
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnSourceAdapterRowReadError;
        public bool AbortOnReadException { get; set; }
        void Prepare();
        void UnPrepare();
        void ReleaseRow(TRow row);
        IEnumerable<TRow> RowsEnumerable { get; }
        int FieldCount { get; }
        ColumnMetadata[] ColumnMetadatas { get; }
        int ParallelLevel { get; set; }
        Dictionary<string, int> MetadatasColumnToIndexMap { get; }
        int RowsPoolSize { get; }
    }
}
