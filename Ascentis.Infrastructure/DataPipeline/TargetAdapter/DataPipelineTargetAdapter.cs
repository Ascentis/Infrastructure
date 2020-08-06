namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter
{
    public abstract class DataPipelineTargetAdapter<TRow> : IDataPipelineTargetAdapter<TRow>
    {
        protected IDataPipelineSourceAdapter<TRow> Source { get; private set; }

        public virtual void Prepare(IDataPipelineSourceAdapter<TRow> source)
        {
            Source = source;
        }

        public abstract void Process(TRow row);
        public virtual void UnPrepare() { }
        public virtual void AbortedWithException(System.Exception e) { }
    }
}
