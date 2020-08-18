using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ClassInterface
    {
        public ClassInterface(Type targetType)
        {
            foreach (var prop in GetType().GetProperties())
            {
                if (!typeof(Delegate).IsAssignableFrom(prop.PropertyType))
                    continue;
                var method = GenericMethod.BuildMethodDelegate(prop.Name, targetType, prop.PropertyType);
                prop.SetValue(this, method);
            }
        }
    }
    
    public class ClassInterface<TClass> : ClassInterface
    {
        public ClassInterface() : base(typeof(TClass)) { }
    }
}
