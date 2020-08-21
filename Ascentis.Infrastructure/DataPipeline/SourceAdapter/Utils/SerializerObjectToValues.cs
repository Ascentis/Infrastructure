using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils
{
    public static class SerializerObjectToValues
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> PropertiesMetadataCache;

        static SerializerObjectToValues()
        {
            PropertiesMetadataCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        }

        public static object[] ObjectToValuesArray(object obj)
        {
            var properties = PropertiesMetadataCache.GetOrAdd(obj.GetType(), type => type.GetProperties().ToList());
            var values = new object[properties.Count];
            ObjectToValuesArray(obj, values);
            return values;
        }

        public static object[] ObjectToValuesArray(object obj, object[] values)
        {
            var properties = PropertiesMetadataCache.GetOrAdd(obj.GetType(), type => type.GetProperties().ToList());
            values ??= new object[properties.Count];
            var index = 0;
            foreach (var prop in properties)
                values[index++] = prop.GetValue(obj);
            return values;
        }
    }
}
