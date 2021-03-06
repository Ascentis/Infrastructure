﻿using System;

namespace Ascentis.Infrastructure.DataPipeline
{
    public interface ITargetAdapter<TRow> : IAdapter
    {
        public event DataPipeline<TRow>.RowErrorDelegate OnTargetAdapterRowProcessError;
        public event DataPipeline<TRow>.BeforeProcessRowDelegate BeforeTargetAdapterProcessRow;
        public event DataPipeline<TRow>.RowDelegate AfterTargetAdapterProcessRow;
        public bool? AbortOnProcessException { get; set; }
        void Prepare(ISourceAdapter<TRow> source);
        void Process(TRow row);
        void UnPrepare();
        void AbortedWithException(Exception e);
        int BufferSize { get; }
    }
}
