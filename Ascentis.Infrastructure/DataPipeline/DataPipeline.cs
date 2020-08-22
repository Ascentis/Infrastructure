using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Ascentis.Infrastructure.DataPipeline.Exceptions;

namespace Ascentis.Infrastructure.DataPipeline
{
    public class DataPipeline<TRow>
    {
        public delegate void RowErrorDelegate(IAdapter adapter, object data, Exception e);
        public delegate void RowDelegate(IAdapter adapter, TRow row);

        // Return BeforeProcessRowResult.Abort to abort processing
        public delegate TargetAdapter.Base.TargetAdapter.BeforeProcessRowResult BeforeProcessRowDelegate(IAdapter adapter, TRow row);

        public event RowErrorDelegate OnSourceAdapterRowReadError; // Runs in SourceAdapter thread
        public event RowErrorDelegate OnTargetAdapterRowProcessError; // Runs in TargetAdapter thread
        public event BeforeProcessRowDelegate BeforeTargetAdapterProcessRow; // Runs in TargetAdapter thread
        public event RowDelegate AfterTargetAdapterProcessRow; // Runs in TargetAdapter thread

        public bool AbortOnSourceAdapterException { get; set; }
        public bool AbortOnTargetAdapterException { get; set; }
        protected ManualResetEvent FinishedEvent { get; }
        protected ISourceAdapter<TRow> CurrentSourceAdapter { get; private set; }

        public DataPipeline()
        {
            FinishedEvent = new ManualResetEvent(false);
        }

        private void SetupAndHealthCheckAdapters(ISourceAdapter<TRow> sourceAdapter,
            IEnumerable<ITargetAdapter<TRow>> targetAdapters,
            out int targetAdaptersCount)
        {
            sourceAdapter.OnSourceAdapterRowReadError += OnSourceAdapterRowReadError;
            sourceAdapter.AbortOnReadException = AbortOnSourceAdapterException;

            targetAdaptersCount = 0;
            foreach (var targetAdapter in targetAdapters)
            {
                targetAdapter.BeforeTargetAdapterProcessRow += BeforeTargetAdapterProcessRow;
                targetAdapter.AfterTargetAdapterProcessRow += AfterTargetAdapterProcessRow;
                targetAdapter.OnTargetAdapterRowProcessError += OnTargetAdapterRowProcessError;
                targetAdapter.AbortOnProcessException ??= AbortOnTargetAdapterException;
                targetAdaptersCount++;
            }

            sourceAdapter.ParallelLevel = targetAdaptersCount;
        }

        private void UnSetupAdapters(ISourceAdapter<TRow> sourceAdapter,
            IEnumerable<ITargetAdapter<TRow>> targetAdapters)
        {
            sourceAdapter.OnSourceAdapterRowReadError -= OnSourceAdapterRowReadError;
            foreach (var targetAdapter in targetAdapters)
            {
                targetAdapter.BeforeTargetAdapterProcessRow -= BeforeTargetAdapterProcessRow;
                targetAdapter.AfterTargetAdapterProcessRow -= AfterTargetAdapterProcessRow;
                targetAdapter.OnTargetAdapterRowProcessError -= OnTargetAdapterRowProcessError;
            }
        }

        private static void CheckDeadlockPotential(ISourceAdapter<TRow> sourceAdapter, IEnumerable<ITargetAdapter<TRow>> targetAdapters)
        {
            var totalTargetBufferSize = targetAdapters.Sum(targetAdapter => targetAdapter.BufferSize);
            if (sourceAdapter.RowsPoolSize > 0 && sourceAdapter.RowsPoolSize < totalTargetBufferSize)
                throw new DataPipelineException("Source adapter rows pool size can't be lesser than the sum of all target adapters buffer sizes. Deadlock would occur upon call to Pump()");
        }

        public virtual void Pump(ISourceAdapter<TRow> sourceAdapter, 
            ITargetAdapter<TRow> targetAdapter)
        {
            var targetAdapters = new [] { targetAdapter};
            Pump(sourceAdapter, targetAdapters);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public virtual void Pump(ISourceAdapter<TRow> sourceAdapter,
            IEnumerable<ITargetAdapter<TRow>> targetAdapters)
        {
            using var resetCurrentSourceAdapter = new Resettable<ISourceAdapter<TRow>>(CurrentSourceAdapter = sourceAdapter, adapter => CurrentSourceAdapter = null);
            using var setEventFinishedResettable = new Resettable<ManualResetEvent>(FinishedEvent, evt => evt.Set());
            SetupAndHealthCheckAdapters(sourceAdapter, targetAdapters, out var targetAdaptersCount);
            using var unSetupAdaptersResettable = new Resettable<object>(this, obj => UnSetupAdapters(sourceAdapter, targetAdapters));

            sourceAdapter.Prepare();
            using var unPrepareSourceAdapterResettable = new Resettable<ISourceAdapter<TRow>>(sourceAdapter, adapter => adapter.UnPrepare());

            foreach (var targetAdapter in targetAdapters)
                targetAdapter.Prepare(sourceAdapter);
            CheckDeadlockPotential(sourceAdapter, targetAdapters);

            var targetAdapterIndex = 0;
            var targetAdapterConveyors = new Conveyor<TRow>[targetAdaptersCount];
            foreach (var targetAdapter in targetAdapters)
            {
                targetAdapterConveyors[targetAdapterIndex++] = new Conveyor<TRow>((row, context) =>
                {
                    ((ITargetAdapter<TRow>) context).Process(row);
                    sourceAdapter.ReleaseRow(row);
                }, targetAdapter);
            }

            try
            {
                foreach (var targetAdapterConveyor in targetAdapterConveyors)
                    targetAdapterConveyor.Start();

                var sourceRows = sourceAdapter.RowsEnumerable;
                foreach (var row in sourceRows)
                    foreach (var targetAdapterConveyor in targetAdapterConveyors)
                        targetAdapterConveyor.InsertPacket(row);

                foreach (var targetAdapterConveyor in targetAdapterConveyors)
                    targetAdapterConveyor.StopAndWait();
                targetAdapters.ForEach(adapter => adapter.UnPrepare());
            }
            catch (Exception e)
            {
                targetAdapterConveyors.ForEach(conveyor => conveyor.Stop(), typeof(InvalidOperationException), false);
                targetAdapters.ForEach(adapter => adapter.AbortedWithException(e));
                throw;
            }
        }

        public virtual IEnumerable<string> SourceColumnNames()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            /* DO NOT CONVERT this to LINQ expression. This method needs yield to work. We need to lazily
               wait for CurrentSourceAdapter to be initialized when Pump() is called */
            if (CurrentSourceAdapter == null)
                throw new InvalidOperationException(
                    "Can't use ColumnNames() iterator unless Pump() has been called and CurrentSourceAdapter initialized");
            foreach (var columnName in CurrentSourceAdapter.ColumnNames())
                yield return columnName;
        }
    }
}
