using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IStreamerFormatter
    {
        void Prepare(IStreamerAdapter source, object target);
        void Process(object[] row, object target);
        void UnPrepare(object target);
        void AbortedWithException(Exception e);
    }
}
