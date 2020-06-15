using System;
using System.Runtime.InteropServices;
using Ascentis.Framework;

namespace Ascentis.Infrastructure
{
    public class SolidComPlus<T, TClass> where TClass : T
    {
        private const int MaxRetries = 1;
        public delegate void InitComObjectDelegate(T obj);
        private readonly ConcurrentObjectAccessor<T, TClass> _objectAccessor;
        private readonly Retrier<ConcurrentObjectAccessor<T, TClass>> _retrier;
        private readonly InitComObjectDelegate _initComObjectDelegate;

        private bool TestCanRetryOnComPlusError(Exception e, int retries)
        {
            if (e is COMException && (e.Message.Contains("The remote procedure call failed") ||
                                      e.Message.Contains("The RPC server is unavailable")))
            {
                _objectAccessor.SwapNewAndExecute(comObj => true, newComObj =>
                {
                    if (_initComObjectDelegate == null)
                        return;
                    _initComObjectDelegate(newComObj);
                }, oldComObj =>
                {
                    try
                    {
                        if (oldComObj is IDisposable disposable)
                            disposable.Dispose();
                    }
                    catch (Exception)
                    {
                        // Ignore exceptions disposing COM+ object. The object could be dead
                    }
                });
            }

            return retries <= MaxRetries - 1;
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null)
        {
            _initComObjectDelegate = initObjectDelegate;
            _objectAccessor = new ConcurrentObjectAccessor<T, TClass>();
            _retrier = new Retrier<ConcurrentObjectAccessor<T, TClass>>(_objectAccessor, TestCanRetryOnComPlusError);
            initObjectDelegate?.Invoke(_objectAccessor.Reference);
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate, params object[] args)
        {
            _initComObjectDelegate = initObjectDelegate;
            _objectAccessor = new ConcurrentObjectAccessor<T, TClass>(args);
            _retrier = new Retrier<ConcurrentObjectAccessor<T, TClass>>(_objectAccessor, TestCanRetryOnComPlusError);
            initObjectDelegate?.Invoke(_objectAccessor.Reference);
        }

        public TFnRetType Retriable<TFnRetType>(ConcurrentObjectAccessor<T, TClass>.LockedFunctionDelegate<TFnRetType> functionDelegate)
        {
            return _retrier.Retriable(accessor => accessor.ExecuteReadLocked(functionDelegate));
        }

        public void Retriable(ConcurrentObjectAccessor<T, TClass>.LockedProcedureDelegate procedureDelegate)
        {
            _retrier.Retriable(accessor => accessor.ExecuteReadLocked(procedureDelegate));
        }

        public TFnRetType NonRetriable<TFnRetType>(ConcurrentObjectAccessor<T, TClass>.LockedFunctionDelegate<TFnRetType> functionDelegate)
        {
            return _retrier.Retriable(accessor => accessor.ExecuteReadLocked(functionDelegate), MaxRetries);
        }

        public void NonRetriable(ConcurrentObjectAccessor<T, TClass>.LockedProcedureDelegate procedureDelegate)
        {
            _retrier.Retriable(accessor => accessor.ExecuteReadLocked(procedureDelegate), MaxRetries);
        }
    }

    public class SolidComPlus<T> : SolidComPlus<T, T>
    {
        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null) : base(initObjectDelegate) {}

        public SolidComPlus(InitComObjectDelegate initObjectDelegate, params object[] args) : base(initObjectDelegate, args) {}
    }
}
