using System.Collections.Generic;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface IDataPipelineSourceAdapter<TRow>
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnSourceAdapterRowReadError;
        public bool AbortOnReadException { get; set; }
        void Prepare();
        void UnPrepare();
        void ReleaseRow(TRow row);
        IEnumerable<TRow> RowsEnumerable { get; }
        int FieldCount { get; }
        DataPipelineColumnMetadata[] ColumnMetadatas { get; }
        int ParallelLevel { get; set; }
        Dictionary<string, int> MetadatasColumnToIndexMap { get; }
    }
}
