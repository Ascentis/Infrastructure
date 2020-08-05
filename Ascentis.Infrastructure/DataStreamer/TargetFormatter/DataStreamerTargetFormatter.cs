namespace Ascentis.Infrastructure.DataStreamer.TargetFormatter
{
    public abstract class DataStreamerTargetFormatter : IDataStreamerTargetFormatter
    {
        protected IDataStreamerSourceAdapter Source { get; private set; }
        protected object Target { get; private set; }

        public virtual void Prepare(IDataStreamerSourceAdapter source, object target)
        {
            Source = source;
            Target = target;
        }

        public abstract void Process(object[] row);
        public virtual void UnPrepare(object target) { }
        public virtual void AbortedWithException(System.Exception e) { }
    }
}
