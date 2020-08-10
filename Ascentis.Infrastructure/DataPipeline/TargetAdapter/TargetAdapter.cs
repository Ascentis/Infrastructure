using System;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter
{
    public abstract class TargetAdapter
    {
        public enum BeforeProcessRowResult
        {
            Continue,
            Abort
        }

        public string Id { get; set; }
        public bool? AbortOnProcessException { get; set; }
        public virtual int BufferSize => 1;
        public virtual void UnPrepare() { }
        public virtual void AbortedWithException(Exception e) { }
    }
}
