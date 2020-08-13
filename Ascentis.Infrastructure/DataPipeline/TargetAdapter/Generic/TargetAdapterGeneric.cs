using System;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Generic
{
    public abstract class TargetAdapter<TRow> : Base.TargetAdapter, ITargetAdapter<TRow>
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnTargetAdapterRowProcessError;
        public event DataPipeline<TRow>.BeforeProcessRowDelegate BeforeTargetAdapterProcessRow;
        public event DataPipeline<TRow>.RowDelegate AfterTargetAdapterProcessRow;
        protected ISourceAdapter<TRow> Source { get; private set; }
        
        public virtual void Prepare(ISourceAdapter<TRow> source)
        {
            Source = source;
        }

        public abstract void Process(TRow row);
        public virtual void BindParameters() {}
       
        protected void InvokeProcessErrorEvent(TRow row, Exception e)
        {
            OnTargetAdapterRowProcessError?.Invoke(this, row, e);
        }

        protected void InvokeAfterTargetAdapterProcessRowEvent(TRow row)
        {
            AfterTargetAdapterProcessRow?.Invoke(this, row);
        }

        protected BeforeProcessRowResult InvokeBeforeTargetAdapterProcessRowEvent(TRow row)
        {
            return BeforeTargetAdapterProcessRow?.Invoke(this, row) ?? BeforeProcessRowResult.Continue;
        }
    }
}
