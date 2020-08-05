using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IStreamerTargetFormatter
    {
        void Prepare(IStreamerSourceAdapter source, object target);
        void Process(object[] row, object target);
        void UnPrepare(object target);
        void AbortedWithException(Exception e);
    }
}
