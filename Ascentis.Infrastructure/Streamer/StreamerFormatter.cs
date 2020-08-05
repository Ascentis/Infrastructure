using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class StreamerFormatter : IStreamerFormatter
    {
        protected IStreamerAdapter Source { get; private set; }

        public virtual void Prepare(IStreamerAdapter source, object target)
        {
            Source = source;
        }

        public abstract void Process(object[] row, object target);

        public virtual void UnPrepare(object target) { }

        public virtual void AbortedWithException(Exception e) { }
    }
}
