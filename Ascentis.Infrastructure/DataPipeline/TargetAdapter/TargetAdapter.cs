using System;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter
{
    public abstract class TargetAdapter<TRow> : ITargetAdapter<TRow>
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnTargetAdapterRowProcessError;
        public event DataPipeline<TRow>.RowDelegate OnTargetAdapterProcessRow;
        public string Id { get; set; }
        public bool? AbortOnProcessException { get; set; }
        protected ISourceAdapter<TRow> Source { get; private set; }
        public virtual int BufferSize => 1;

        public virtual void Prepare(ISourceAdapter<TRow> source)
        {
            Source = source;
        }

        public abstract void Process(TRow row);
        public virtual void UnPrepare() { }
        public virtual void AbortedWithException(Exception e) { }

        protected void InvokeProcessErrorEvent(TRow row, Exception e)
        {
            OnTargetAdapterRowProcessError?.Invoke(this, row, e);
        }

        protected void InvokeTargetAdapterProcessRowEvent(TRow row)
        {
            OnTargetAdapterProcessRow?.Invoke(this, row);
        }
    }
}
