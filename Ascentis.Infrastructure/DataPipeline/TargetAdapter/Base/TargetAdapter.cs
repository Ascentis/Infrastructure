using System;
using System.Data.Common;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter.Base
{
    public abstract class TargetAdapter
    {
        public delegate void AdapterDelegate(IAdapter adapter);
        public delegate void CommandDelegate(IAdapter adapter, DbCommand cmd);
        public enum BeforeProcessRowResult
        {
            Continue,
            Abort
        }

        public int Id { get; set; }
        public bool? AbortOnProcessException { get; set; }
        public virtual int BufferSize => 1;
        public virtual void UnPrepare() { }
        public virtual void AbortedWithException(Exception e) { }
    }
}
