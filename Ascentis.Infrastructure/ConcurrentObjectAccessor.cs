using System;
using System.Threading;

namespace Ascentis.Infrastructure
{
    public class ConcurrentObjectAccessor<T, TClass> where TClass : T
    {
        public delegate void LockedProcedureDelegate(T reference);
        public delegate TFnRetType LockedFunctionDelegate<out TFnRetType>(T reference);
        public delegate bool GateDelegate(T reference);
        public delegate void InitReferenceDelegate(T reference);

        private readonly object[] _constructorArgs;
        private ReaderWriterLockSlim _refLock;
        private T _reference;

        public T Reference
        {
            get
            {
                _refLock.EnterReadLock();
                try
                {
                    return _reference;
                }
                finally
                {
                    _refLock.ExitReadLock();
                }
            }
            private set => _reference = value; // Private caller controls locking
        }

        public ConcurrentObjectAccessor()
        {
            Reference = Activator.CreateInstance<TClass>();
            InitRefLock();
        }

        public ConcurrentObjectAccessor(params object[] args)
        {
            _constructorArgs = args;
            Reference = (T) Activator.CreateInstance(typeof(TClass), args);
            InitRefLock();
        }

        private void InitRefLock()
        {
            _refLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public TFnReturnType ExecuteReadLocked<TFnReturnType>(LockedFunctionDelegate<TFnReturnType> functionDelegate)
        {
            _refLock.EnterReadLock();
            try
            {
                return functionDelegate(Reference);
            }
            finally
            {
                _refLock.ExitReadLock();
            }
        }

        public void ExecuteReadLocked(LockedProcedureDelegate procedureDelegate)
        {
            ExecuteReadLocked((LockedFunctionDelegate<object>) (reference =>
            {
                procedureDelegate(reference);
                return null;
            }));
        }

        public TFnReturnType SwapNewAndExecute<TFnReturnType>(GateDelegate gateOpenDelegate, InitReferenceDelegate initReference, LockedFunctionDelegate<TFnReturnType> cleanupOldReference) 
        {
            _refLock.EnterUpgradeableReadLock();
            try
            {
                if (!gateOpenDelegate(Reference))
                    return default;
                T oldReference;
                T newReference;
                if (_constructorArgs != null)
                    newReference = (T)Activator.CreateInstance(typeof(T), _constructorArgs);
                else
                    newReference = Activator.CreateInstance<T>();
                initReference(newReference);
                _refLock.EnterWriteLock();
                try
                {
                    oldReference = _reference;
                    Reference = newReference;
                }
                finally
                {
                    _refLock.ExitWriteLock();
                }

                return cleanupOldReference(oldReference);
            }
            finally
            {
                _refLock.ExitUpgradeableReadLock();
            }
        }

        public TFnReturnType SwapNewAndExecute<TFnReturnType>(GateDelegate gateOpenDelegate, LockedFunctionDelegate<TFnReturnType> functionDelegate)
        {
            return SwapNewAndExecute(gateOpenDelegate, reference => { }, functionDelegate);
        }

        public void SwapNewAndExecute(GateDelegate gateOpenDelegate, LockedProcedureDelegate procedureDelegate)
        {
            SwapNewAndExecute(gateOpenDelegate, reference => {}, procedureDelegate);
        }

        public void SwapNewAndExecute(GateDelegate gateOpenDelegate, InitReferenceDelegate initReference, LockedProcedureDelegate procedureDelegate)
        {
            SwapNewAndExecute<object>(gateOpenDelegate, initReference,reference =>
            {
               procedureDelegate(reference);
               return null;
            });
        }

        public TFnReturnType SwapNewAndExecute<TFnReturnType>(LockedFunctionDelegate<TFnReturnType> functionDelegate)
        {
            return SwapNewAndExecute(reference => true, functionDelegate);
        }

        public TFnReturnType SwapNewAndExecute<TFnReturnType>(InitReferenceDelegate initReference, LockedFunctionDelegate<TFnReturnType> functionDelegate)
        {
            return SwapNewAndExecute(reference => true, initReference, functionDelegate);
        }

        public void SwapNewAndExecute(LockedProcedureDelegate procedureDelegate)
        {
            SwapNewAndExecute(reference => true, procedureDelegate);
        }

        public void SwapNewAndExecute(InitReferenceDelegate initReference, LockedProcedureDelegate procedureDelegate)
        {
            SwapNewAndExecute(reference => true, initReference, procedureDelegate);
        }
    }

    public class ConcurrentObjectAccessor<T> : ConcurrentObjectAccessor<T, T>
    {
        public ConcurrentObjectAccessor() {}

        public ConcurrentObjectAccessor(params object[] args) : base (args) {}
    }
}
