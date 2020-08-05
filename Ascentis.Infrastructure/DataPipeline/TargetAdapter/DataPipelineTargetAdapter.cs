namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter
{
    public abstract class DataPipelineTargetAdapter<TTarget, TRow> : IDataPipelineTargetAdapter<TTarget, TRow>
    {
        protected IDataPipelineSourceAdapter<TRow> Source { get; private set; }
        protected TTarget Target { get; private set; }

        public virtual void Prepare(IDataPipelineSourceAdapter<TRow> source, TTarget target)
        {
            Source = source;
            Target = target;
        }

        public abstract void Process(TRow row);
        public virtual void UnPrepare() { }
        public virtual void AbortedWithException(System.Exception e) { }
    }
}
