using System.Data;

namespace Ascentis.Infrastructure.DataStreamer.TargetFormatter
{
    public abstract class DataStreamerTargetFormatter<TTarget, TRow> : IDataStreamerTargetFormatter<TTarget, TRow>
    {
        protected IDataStreamerSourceAdapter<TRow> Source { get; private set; }
        protected TTarget Target { get; private set; }

        public virtual void Prepare(IDataStreamerSourceAdapter<TRow> source, TTarget target)
        {
            Source = source;
            Target = target;
        }

        public abstract void Process(TRow row);
        public virtual void UnPrepare() { }
        public virtual void AbortedWithException(System.Exception e) { }
    }
}
