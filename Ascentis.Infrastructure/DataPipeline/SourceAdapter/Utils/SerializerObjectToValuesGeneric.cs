using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils
{
    public static class SerializerObjectToValues<T>
    {
        private delegate void MoveValueToArraySlotDelegate(object[] values, int index, T obj);
        private static readonly List<MoveValueToArraySlotDelegate> PropertyMovers;

        private static void AddPropertyMover<TRet>(PropertyInfo prop)
        {
            var propAccessor = (Func<T, TRet>)prop.GetMethod.CreateDelegate(typeof(Func<T, TRet>));
            PropertyMovers.Add((values, index, obj) => values[index] = propAccessor(obj));
        }

        static SerializerObjectToValues()
        {
            PropertyMovers = new List<MoveValueToArraySlotDelegate>();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.PropertyType == typeof(int))
                    AddPropertyMover<int>(prop);
                else if (prop.PropertyType == typeof(short))
                    AddPropertyMover<short>(prop);
                else if (prop.PropertyType == typeof(long))
                    AddPropertyMover<long>(prop);
                else if (prop.PropertyType == typeof(string))
                    AddPropertyMover<string>(prop);
                else if (prop.PropertyType == typeof(decimal))
                    AddPropertyMover<decimal>(prop);
                else if (prop.PropertyType == typeof(bool))
                    AddPropertyMover<bool>(prop);
                else if (prop.PropertyType == typeof(DateTime))
                    AddPropertyMover<DateTime>(prop);
                else if (prop.PropertyType == typeof(double))
                    AddPropertyMover<double>(prop);
                else if (prop.PropertyType == typeof(float))
                    AddPropertyMover<float>(prop);
                else
                    /* Fallback to pure Reflection. Significantly slower than using specialized delegates */
                    PropertyMovers.Add((values, index, obj) => values[index] = prop.GetValue(obj));
            }
        }

        public static object[] ObjectToValuesArray(T obj)
        {
            var values = new object[PropertyMovers.Count];
            ObjectToValuesArray(obj, values);

            return values;
        }

        public static object[] ObjectToValuesArray(T obj, object[] values)
        {
            var index = 0;
            values ??= new object[PropertyMovers.Count];
            foreach (var propMover in PropertyMovers)
                propMover(values, index++, obj);

            return values;
        }
    }
}
