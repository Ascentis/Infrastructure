using System;
using System.Threading;

namespace Ascentis.Infrastructure
{
    public class TlsAccessor<T, TClass> where TClass : T
    {
        private LocalDataStoreSlot _refSlot;
        private readonly object[] _constructorArgs;
        public bool IgnoreRefDisposalExceptions;

        public TlsAccessor()
        {
            InitRefSlot();
        }

        public TlsAccessor(params object[] args)
        {
            _constructorArgs = args;
            InitRefSlot();
        }

        private void InitRefSlot()
        {
            _refSlot = Thread.GetNamedDataSlot($"COMRef-{GetHashCode()}");
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
