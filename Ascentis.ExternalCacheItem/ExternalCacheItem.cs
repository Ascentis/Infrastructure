using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("049a63fb-bb7c-48e5-b0cc-dedc69234df4")]
    public class ExternalCacheItem : System.EnterpriseServices.ServicedComponent, IExternalCacheItem
    {
        // ReSharper disable once InconsistentNaming
        public readonly Dynamo _container; // keep name as if private but needs to be public. Need this for remoting serialization to work
        public dynamic Container => _container;

        public ExternalCacheItem()
        {
            _container = new Dynamo();
        }

        public object this[string key]
        {
            get => _container[key];
            set => _container[key] = value;
        }

        public void CopyFrom(object value)
        {
            if(value is ExternalCacheItem externalCacheItem)
                _container.CopyFrom(externalCacheItem._container);
            else 
                _container.CopyFrom(value);
        }

        public void CopyTo(object target)
        {
            if(target is ExternalCacheItem externalCacheItem)
                _container.CopyTo(externalCacheItem._container);
            else 
                _container.CopyTo(target);
        }
    }
}