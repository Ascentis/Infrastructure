﻿using System;

namespace Ascentis.Infrastructure.DataPipeline.TargetAdapter
{
    public abstract class DataPipelineTargetAdapter<TRow> : IDataPipelineTargetAdapter<TRow>
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnTargetAdapterRowProcessError;
        public bool AbortOnProcessException { get; set; }
        public int ParallelLevel { get; set; }
        protected IDataPipelineSourceAdapter<TRow> Source { get; private set; }

        public virtual void Prepare(IDataPipelineSourceAdapter<TRow> source)
        {
            Source = source;
        }

        public abstract void Process(TRow row);
        public virtual void UnPrepare() { }
        public virtual void AbortedWithException(System.Exception e) { }

        protected void InvokeProcessErrorEvent(TRow row, Exception e)
        {
            OnTargetAdapterRowProcessError?.Invoke(row, e);
        }
    }
}
