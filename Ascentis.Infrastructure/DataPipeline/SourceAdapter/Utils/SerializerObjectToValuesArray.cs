using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils
{
    public class SerializerObjectToValuesArray
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> PropertiesMetadataCache;

        static SerializerObjectToValuesArray()
        {
            PropertiesMetadataCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        }

        public static object[] ObjectToValuesArray(object obj)
        {
            var properties = PropertiesMetadataCache.GetOrAdd(obj.GetType(), type => type.GetProperties().ToList());
            var values = new object[properties.Count];
            var index = 0;
            foreach (var prop in properties)
                values[index++] = prop.GetValue(obj);
            return values;
        }
    }
}
