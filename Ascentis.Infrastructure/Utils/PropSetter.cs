using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class PropSetter
    {
        public static void SetProp(PropertyInfo prop, object target, object value)
        {
            switch (value)
            {
                case int i:
                    if (prop.PropertyType == typeof(short) || prop.PropertyType == typeof(short?))
                        prop.SetValue(target, (short)i);
                    else
                        prop.SetValue(target, value);
                    break;
                default:
                    prop.SetValue(target, value);
                    break;
            }
        }
    }
}
