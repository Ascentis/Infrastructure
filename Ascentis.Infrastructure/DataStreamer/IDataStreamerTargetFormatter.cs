using System;

namespace Ascentis.Infrastructure.DataStreamer
{
    public interface IDataStreamerTargetFormatter<in TTarget, TRow>
    {
        void Prepare(IDataStreamerSourceAdapter<TRow> source, TTarget target);
        void Process(TRow row);
        void UnPrepare();
        void AbortedWithException(Exception e);
    }
}
