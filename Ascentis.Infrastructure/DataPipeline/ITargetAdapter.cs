using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface ITargetAdapter<TRow> : IAdapter
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnTargetAdapterRowProcessError;
        public bool? AbortOnProcessException { get; set; }
        void Prepare(ISourceAdapter<TRow> source);
        void Process(TRow row);
        void UnPrepare();
        void AbortedWithException(Exception e);
        int BufferSize { get; }
    }
}
