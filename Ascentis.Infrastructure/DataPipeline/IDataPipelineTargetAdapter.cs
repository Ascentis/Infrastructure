using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface IDataPipelineTargetAdapter<TRow>
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnTargetAdapterRowProcessError;
        public bool? AbortOnProcessException { get; set; }
        void Prepare(IDataPipelineSourceAdapter<TRow> source);
        void Process(TRow row);
        void UnPrepare();
        void AbortedWithException(Exception e);
    }
}
