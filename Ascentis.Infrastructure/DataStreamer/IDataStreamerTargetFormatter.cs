using System;

namespace Ascentis.Infrastructure.DataStreamer
{
    public interface IDataStreamerTargetFormatter
    {
        void Prepare(IDataStreamerSourceAdapter source, object target);
        void Process(object[] row, object target);
        void UnPrepare(object target);
        void AbortedWithException(Exception e);
    }
}
