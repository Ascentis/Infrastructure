using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface IDataPipelineTargetAdapter<in TTarget, TRow>
    {
        void Prepare(IDataPipelineSourceAdapter<TRow> source, TTarget target);
        void Process(TRow row);
        void UnPrepare();
        void AbortedWithException(Exception e);
    }
}
