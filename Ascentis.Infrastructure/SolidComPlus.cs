using System;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    public class SolidComPlus<T, TClass> where TClass : T
    {
        private const int MaxRetries = 1;
        public delegate void InitComObjectDelegate(T obj);
        private readonly TlsAccessor<T, TClass> _objectAccessor;
        private readonly Retrier<TlsAccessor<T, TClass>> _retrier;
        private readonly InitComObjectDelegate _initComObjectDelegate;

        private bool TestCanRetryOnComPlusError(Exception e, int retries)
        {
            if (e is COMException && (e.Message.Contains("The remote procedure call failed") ||
                                      e.Message.Contains("The RPC server is unavailable")))
            {
                _objectAccessor.Reference = default(T);
                _initComObjectDelegate?.Invoke(_objectAccessor.Reference);
            }

            return retries <= MaxRetries - 1;
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null)
        {
            _initComObjectDelegate = initObjectDelegate;
            _objectAccessor = new TlsAccessor<T, TClass>((newComObj) =>
            {
                initObjectDelegate?.Invoke(newComObj);
            });
            _objectAccessor.IgnoreRefDisposalExceptions = true;
            _retrier = new Retrier<TlsAccessor<T, TClass>>(_objectAccessor, TestCanRetryOnComPlusError);
            initObjectDelegate?.Invoke(_objectAccessor.Reference);
        }

        public TFnRetType Retriable<TFnRetType>(ConcurrentObjectAccessor<T, TClass>.LockedFunctionDelegate<TFnRetType> functionDelegate)
        {
            return _retrier.Retriable(accessor => functionDelegate(accessor.Reference));
        }

        public void Retriable(ConcurrentObjectAccessor<T, TClass>.LockedProcedureDelegate procedureDelegate)
        {
            _retrier.Retriable(accessor => procedureDelegate(accessor.Reference));
        }

        public TFnRetType NonRetriable<TFnRetType>(ConcurrentObjectAccessor<T, TClass>.LockedFunctionDelegate<TFnRetType> functionDelegate)
        {
            return _retrier.Retriable(accessor => functionDelegate(accessor.Reference), MaxRetries);
        }

        public void NonRetriable(ConcurrentObjectAccessor<T, TClass>.LockedProcedureDelegate procedureDelegate)
        {
            _retrier.Retriable(accessor => procedureDelegate(accessor.Reference), MaxRetries);
        }
    }

    public class SolidComPlus<T> : SolidComPlus<T, T>
    {
        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null) : base(initObjectDelegate) {}
    }
}
