using System;
using System.Collections.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter
{
    public abstract class DataPipelineSourceAdapter<T> : IDataPipelineSourceAdapter<T>
    {
        private IEnumerable<T> _rowsEnumerable;
        public event DataPipeline<T>.RowErrorDelegate OnSourceAdapterRowReadError;
        public bool AbortOnReadException { get; set; }
        public abstract int FieldCount { get; }
        public virtual DataPipelineColumnMetadata[] ColumnMetadatas { get; set; }

        protected void InvokeRowReadErrorEvent(object sourceData, Exception e)
        {
            OnSourceAdapterRowReadError?.Invoke(sourceData, e);
        }

        public virtual void UnPrepare() { }
        public virtual void ReleaseRow(T row) { }
        public abstract IEnumerable<T> RowsEnumerable { get; }
        public virtual void Prepare() { }
    }
}
