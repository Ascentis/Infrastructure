using System;
using System.Runtime.InteropServices;
using Ascentis.Framework;

namespace Ascentis.Infrastructure
{
    public class SolidComPlus<T>
    {
        public delegate void InitComObjectDelegate(T obj);
        private readonly ConcurrentObjectAccessor<T> _objectAccessor;
        private readonly Retrier<ConcurrentObjectAccessor<T>> _retrier;
        private readonly InitComObjectDelegate _initComObjectDelegate;

        private bool TestCanRetryOnComPlusError(Exception e, int retries)
        {
            if (!(e is COMException) || retries != 0) 
                return false;
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
                    // ignored any exception. COM object may be stale at this point
                }
            });
            return true;
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null)
        {
            _initComObjectDelegate = initObjectDelegate;
            _objectAccessor = new ConcurrentObjectAccessor<T>();
            _retrier = new Retrier<ConcurrentObjectAccessor<T>>(_objectAccessor, TestCanRetryOnComPlusError);
            initObjectDelegate?.Invoke(_objectAccessor.Reference);
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate, params object[] args)
        {
            _initComObjectDelegate = initObjectDelegate;
            _objectAccessor = new ConcurrentObjectAccessor<T>(args);
            _retrier = new Retrier<ConcurrentObjectAccessor<T>>(_objectAccessor, TestCanRetryOnComPlusError);
            initObjectDelegate?.Invoke(_objectAccessor.Reference);
        }

        public TFnRetType Retriable<TFnRetType>(ConcurrentObjectAccessor<T>.LockedFunctionDelegate<TFnRetType> functionDelegate)
        {
            return _retrier.Retriable(accessor => accessor.ExecuteReadLocked(functionDelegate));
        }

        public void Retriable(ConcurrentObjectAccessor<T>.LockedProcedureDelegate procedureDelegate)
        {
            _retrier.Retriable(accessor => accessor.ExecuteReadLocked(procedureDelegate));
        }
    }
}
