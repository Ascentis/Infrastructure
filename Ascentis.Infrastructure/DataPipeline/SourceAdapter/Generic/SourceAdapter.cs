using System;
using System.Collections.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Generic
{
    public abstract class SourceAdapter<T> : SourceAdapter, ISourceAdapter<T>
    {
        private IEnumerable<T> _rowsEnumerable;
        
        public event DataPipeline<T>.RowErrorDelegate OnSourceAdapterRowReadError;

        protected void InvokeRowReadErrorEvent(object sourceData, Exception e)
        {
            OnSourceAdapterRowReadError?.Invoke(this, sourceData, e);
        }

        public virtual void ReleaseRow(T row) { }
        public abstract IEnumerable<T> RowsEnumerable { get; }
    }
}
