using System.Reflection;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("049a63fb-bb7c-48e5-b0cc-dedc69234df4")]
    public class ExternalCacheItem : System.EnterpriseServices.ServicedComponent
    {
        private readonly Dynamo _container;
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
            var type = value.GetType();
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                _container[prop.Name] = prop.GetValue(value);
        }

        public void CopyTo(object target)
        {
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