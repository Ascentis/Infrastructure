using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface IDataPipelineTargetAdapter<TRow>
    {
        void Prepare(IDataPipelineSourceAdapter<TRow> source);
        void Process(TRow row);
        void UnPrepare();
        void AbortedWithException(Exception e);
    }
}
