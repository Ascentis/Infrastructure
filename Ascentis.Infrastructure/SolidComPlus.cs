using System;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    public class SolidComPlus<T, TClass> : IDisposable where TClass : T
    {
        private const int MaxRetries = 1;
        public delegate TFnRetType FunctionDelegate<out TFnRetType>(T reference);
        public delegate void ProcedureDelegate(T reference);
        public delegate void InitComObjectDelegate(T obj);
        private readonly TlsAccessor<T, TClass> _objectAccessor;
        private readonly Retrier<TlsAccessor<T, TClass>> _retrier;

        private bool TestCanRetryOnComPlusError(Exception e, int retries)
        {
            if (e is COMException && (e.Message.Contains("The remote procedure call failed") ||
                                      e.Message.Contains("The RPC server is unavailable")))
            {
                _objectAccessor.Reference = default;
                // ReSharper disable once UnusedVariable
                var readRef = _objectAccessor.Reference; // This access to Reference causes newly created instance to be initialized
            }

            return retries <= MaxRetries - 1;
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null)
        {
            _objectAccessor = new TlsAccessor<T, TClass>(newComObj =>
            {
                initObjectDelegate?.Invoke(newComObj);
            });
            _objectAccessor.IgnoreRefDisposalExceptions = true;
            _retrier = new Retrier<TlsAccessor<T, TClass>>(_objectAccessor, TestCanRetryOnComPlusError);
            initObjectDelegate?.Invoke(_objectAccessor.Reference);
        }

        public void Dispose()
        {
            _objectAccessor.Dispose();
        }

        public TFnRetType Retriable<TFnRetType>(FunctionDelegate<TFnRetType> functionDelegate)
        {
            return _retrier.Retriable(accessor => functionDelegate(accessor.Reference));
        }

        public void Retriable(ProcedureDelegate procedureDelegate)
        {
            _retrier.Retriable(accessor => procedureDelegate(accessor.Reference));
        }

        public TFnRetType NonRetriable<TFnRetType>(FunctionDelegate<TFnRetType> functionDelegate)
        {
            return _retrier.Retriable(accessor => functionDelegate(accessor.Reference), MaxRetries);
        }

        public void NonRetriable(ProcedureDelegate procedureDelegate)
        {
            _retrier.Retriable(accessor => procedureDelegate(accessor.Reference), MaxRetries);
        }
    }

    public class SolidComPlus<T> : SolidComPlus<T, T>
    {
        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null) : base(initObjectDelegate) {}
    }
}
