using System;
using System.Reflection;
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
            if (value is ExternalCacheItem item)
                foreach (var prop in item._container.GetDynamicMemberNames())
                    _container[prop] = item._container[prop];
            else {
                var type = value.GetType();
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    _container[prop.Name] = prop.GetValue(value);
            }
        }

        public void CopyTo(object target)
        {
            if (target is ExternalCacheItem targetItem)
                foreach (var prop in _container.GetDynamicMemberNames())
                    targetItem[prop] = _container[prop];
            else {
                var type = target.GetType();
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!_container.PropertyExists(prop.Name))
                        continue;
                    prop.SetValue(target, _container[prop.Name]);
                }
            }
        }
    }
}