using System;
using System.Runtime.InteropServices;
using Ascentis.Framework;

namespace Ascentis.Infrastructure
{
    public class SolidComPlus<T> where T: new()
    {
        public delegate void InitComObjectDelegate(object obj);
        private readonly ConcurrentObjectAccessor<T> _objectAccessor;
        private readonly Retrier<ConcurrentObjectAccessor<T>> _retrier;
        private object[] _constructorArgs;
        private InitComObjectDelegate _initComObjectDelegate;

        private bool CanRetryOnCOMPlusError(Exception e, int retries)
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
                if (oldComObj is IDisposable disposable)
                    disposable.Dispose();
            });
            return true;
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate = null)
        {
            _initComObjectDelegate = initObjectDelegate;
            _objectAccessor = new ConcurrentObjectAccessor<T>();
            _retrier = new Retrier<ConcurrentObjectAccessor<T>>(_objectAccessor, CanRetryOnCOMPlusError);
        }

        public SolidComPlus(InitComObjectDelegate initObjectDelegate, params object[] args)
        {
            _initComObjectDelegate = initObjectDelegate;
            _constructorArgs = args;
            _objectAccessor = new ConcurrentObjectAccessor<T>(args);
            _retrier = new Retrier<ConcurrentObjectAccessor<T>>(_objectAccessor, CanRetryOnCOMPlusError);
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
