using System;

namespace Ascentis.Infrastructure.DataStreamer
{
    public abstract class DataStreamerTargetFormatter : IDataStreamerTargetFormatter
    {
        protected IDataStreamerSourceAdapter Source { get; private set; }

        public virtual void Prepare(IDataStreamerSourceAdapter source, object target)
        {
            Source = source;
        }

        public abstract void Process(object[] row, object target);
        public virtual void UnPrepare(object target) { }
        public virtual void AbortedWithException(Exception e) { }
    }
}
