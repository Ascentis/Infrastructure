using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer
{
    internal class SerializerHelper
    {
        internal delegate void MoveValueToArraySlotDelegate<in T>(object[] values, int index, T obj);

        private static readonly ConcurrentDictionary<Type, List<MoveValueToArraySlotDelegate<object>>> PropertyMovers;
        private static readonly ConcurrentDictionary<Type, Dictionary<string, int>> Properties;
        private static readonly ConcurrentDictionary<Type, int[]> DefaultFieldMaps;

        static SerializerHelper()
        {
            PropertyMovers = new ConcurrentDictionary<Type, List<MoveValueToArraySlotDelegate<object>>>();
            DefaultFieldMaps = new ConcurrentDictionary<Type, int[]>();
            Properties = new ConcurrentDictionary<Type, Dictionary<string, int>>();
        }

        internal static List<MoveValueToArraySlotDelegate<object>> GetPropertyMoversForObject(object obj)
        {
            var propertyMovers = PropertyMovers.GetOrAdd(obj.GetType(), type =>
            {
                var propMovers = new List<MoveValueToArraySlotDelegate<object>>();
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    propMovers.Add((values, index, theObj) =>
                    {
                        values[index] = prop.GetValue(theObj);
                    });
                }

                return propMovers;
            });
            return propertyMovers;
        }

        internal static int[] GetFieldMapForObject(object obj)
        {
            var fieldMaps = DefaultFieldMaps.GetOrAdd(obj.GetType(), type =>
            {
                var propertyMovers = GetPropertyMoversForObject(obj);
                var map = new int[((ICollection) propertyMovers).Count];
                for (var i = 0; i < map.Length; i++)
                    map[i] = i;
                return map;
            });
            return fieldMaps;
        }

        internal static Dictionary<string, int> GetPropertiesForObject(object obj)
        {
            var properties = Properties.GetOrAdd(obj.GetType(), type =>
            {
                var propsDict = new Dictionary<string, int>();
                var index = 0;
                foreach (var property in type.GetProperties())
                    propsDict.Add(property.Name, index++);
                return propsDict;
            });
            return properties;
        }
    }
}
