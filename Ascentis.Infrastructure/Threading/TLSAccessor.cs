using System;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class TlsAccessor<T, TClass> : IDisposable where TClass : T
    {
        private LocalDataStoreSlot _refSlot;
        public delegate void InitObjectDelegate(T obj);
        private readonly object[] _constructorArgs;
        public bool IgnoreRefDisposalExceptions;
        private readonly InitObjectDelegate _initObjectDelegate;

        public TlsAccessor(InitObjectDelegate initObjectDelegate)
        {
            _initObjectDelegate = initObjectDelegate;
            InitRefSlot();
        }

        public TlsAccessor(InitObjectDelegate initObjectDelegate, params object[] args) : this(initObjectDelegate)
        {
            _constructorArgs = args;
        }

        public void Dispose()
        {
            Thread.FreeNamedDataSlot($"TlsRef-{GetHashCode()}");
        }

        private void InitRefSlot()
        {
            _refSlot = Thread.GetNamedDataSlot($"TlsRef-{GetHashCode()}");
        }

        public T Reference
        {
            get
            {
                var refObj = Thread.GetData(_refSlot);
                if (refObj != null)
                    return (T) refObj;
                if(_constructorArgs != null)
                    refObj = (TClass)Activator.CreateInstance(typeof(TClass), _constructorArgs);
                else
                    refObj = (TClass)Activator.CreateInstance(typeof(TClass));
                _initObjectDelegate?.Invoke((T)refObj);
                Thread.SetData(_refSlot, refObj);
                return (T)refObj;
            }
            set
            {
                var refObj = Reference;
                if(refObj != null && refObj is IDisposable disposable)
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception)
                    {
                        if (!IgnoreRefDisposalExceptions)
                            throw;
                    }
                Thread.SetData(_refSlot, value);
            }
        }
    }
}
