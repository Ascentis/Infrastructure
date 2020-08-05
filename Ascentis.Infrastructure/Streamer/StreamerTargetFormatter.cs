using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class StreamerTargetFormatter : IStreamerTargetFormatter
    {
        protected IStreamerSourceAdapter Source { get; private set; }

        public virtual void Prepare(IStreamerSourceAdapter source, object target)
        {
            Source = source;
        }

        public abstract void Process(object[] row, object target);

        public virtual void UnPrepare(object target) { }

        public virtual void AbortedWithException(Exception e) { }
    }
}
