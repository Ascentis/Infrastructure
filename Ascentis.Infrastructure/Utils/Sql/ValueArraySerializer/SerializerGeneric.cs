using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer
{
    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    public class Serializer<T> : Serializer
    {
        private static readonly List<MoveValueToArraySlotDelegate<T>> PropertyMovers;
        private static readonly IDictionary<string, int> FieldNames;
        private static readonly int[] DefaultMap;

        private int _fieldsEnabledCount;
        private IOnOffArray _propertySerializationEnabledArray;
        
        private static void AddPropertyMover<TRet>(PropertyInfo prop)
        {
            var propAccessor = (Func<T, TRet>)prop.GetMethod.CreateDelegate(typeof(Func<T, TRet>));
            PropertyMovers.Add((values, index, obj) => values[index] = propAccessor(obj));
        }

        static Serializer()
        {
            if (typeof(T) == typeof(object))
                return;
            PropertyMovers = new List<MoveValueToArraySlotDelegate<T>>();
            FieldNames = new Dictionary<string, int>();
            var idx = 0;
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
                FieldNames.Add(prop.Name, idx++);
            }

            DefaultMap = new int[FieldNames.Count];
            for (var i = 0; i < DefaultMap.Length; i++)
                DefaultMap[i] = i;
        }

        public IOnOffArray OnOffsEnabled
        {
            get => _propertySerializationEnabledArray;
            set
            {
                if (value == _propertySerializationEnabledArray)
                    return;
                _propertySerializationEnabledArray = value;

                if (_propertySerializationEnabledArray == null)
                {
                    _fieldsEnabledCount = 0;
                    return;
                }

                for (var i = 0; i < _propertySerializationEnabledArray.Count; i++)
                    if (_propertySerializationEnabledArray[i])
                        _fieldsEnabledCount++;
            }
        }

        private static void CheckNotBaseObject(string methodName)
        {
            if (typeof(T) == typeof(object))
                throw new InvalidOperationException($"Method {methodName} can't be used with base object type");
        }

        public static IOnOffArray BuildFieldEnabledFlagArray()
        {
            CheckNotBaseObject("BuildFieldEnabledFlagArray");
            return new OnOffArray(PropertyMovers.Count);
        }

        public static IOnOffArray BuildFieldEnabledFlagArray(T obj)
        {
            var propertyMovers = GetPropertyMovers(obj);
            return new OnOffArray(propertyMovers.Count);
        }

        public int[] FieldMap { get; set; }

        public static int IndexOfProperty(string name)
        {
            CheckNotBaseObject("IndexOfProperty");
            return FieldNames[name];
        }

        public static int IndexOfProperty(T obj, string name)
        {
            var properties = GetPropertiesForObject(obj);
            return properties[name];
        }

        private static ICollection GetPropertyMovers(T obj)
        {
            var propertyMovers = typeof(T) != typeof(object)
                ? PropertyMovers
                : (ICollection) GetPropertyMoversForObject(obj);
            return propertyMovers;
        }

        private static int[] GetFieldMap(T obj, int[] map)
        {
            return map ?? (typeof(T) != typeof(object) ? DefaultMap : GetFieldMap(obj));
        }

        private static int FieldsCount(T obj, IOnOffArray onOffsEnabled, int[] map)
        {
            var count = 0;
            var index = 0;
            var propertyMovers = GetPropertyMovers(obj);
            var fieldsMap = GetFieldMap(obj, map);
            foreach (var dummy in propertyMovers)
            {
                var localIndex = index++;
                if (fieldsMap[localIndex] < 0)
                    continue;
                if (!(onOffsEnabled?[localIndex] ?? true))
                    continue;
                count++;
            }

            return count;
        }

        private static int[] BuildFieldMap(IDictionary<string, int> properties, ICollection<string> columnsMap)
        {
            var map = new int[properties.Count];
            for (var i = 0; i < map.Length; i++)
                map[i] = -1;
            var index = 0;
            foreach (var item in columnsMap)
            {
                if (!properties.TryGetValue(item, out var columnIndex))
                    throw new InvalidOperationException($"Column {item} not found in class {nameof(T)}");
                map[columnIndex] = index++;
            }

            return map;
        }

        public static int[] BuildFieldMap(ICollection<string> columnsMap)
        {
            CheckNotBaseObject("BuildFieldMap");
            return BuildFieldMap(FieldNames, columnsMap);
        }

        public static int[] BuildFieldMap(T obj, ICollection<string> columnsMap)
        {
            return BuildFieldMap(GetPropertiesForObject(obj), columnsMap);
        }

        public object[] ObjectToValuesArray(T obj)
        {
            var values = new object[_fieldsEnabledCount > 0 ? _fieldsEnabledCount : GetPropertyMovers(obj).Count];
            ObjectToValuesArray(obj, values, _propertySerializationEnabledArray, FieldMap);

            return values;
        }

        public object[] ObjectToValuesArray(T obj, object[] values)
        {
            ObjectToValuesArray(obj, values, _propertySerializationEnabledArray, FieldMap);

            return values;
        }

        public static object[] ObjectToValuesArray(
            T obj, 
            IOnOffArray onOffsEnabled = null, 
            int[] map = null)
        {
            var values = new object[FieldsCount(obj, onOffsEnabled, map)];
            ObjectToValuesArray(obj, values, onOffsEnabled, map);

            return values;
        }

        public static object[] ObjectToValuesArray(
            T obj, 
            object[] values, 
            IOnOffArray onOffsEnabled = null, 
            int[] map = null)
        {
            var sourceIndex = 0;
            var targetIndex = 0;
            var indexMap = GetFieldMap(obj, map);
            var skipCount = 0;
            values ??= new object[FieldsCount(obj, onOffsEnabled, indexMap)];
            var propertyMovers = GetPropertyMovers(obj);

            foreach (var propMover in propertyMovers)
            {
                if ((onOffsEnabled?[sourceIndex] ?? true) && indexMap[sourceIndex] >= 0)
                    ((MoveValueToArraySlotDelegate<T>)propMover)(values, indexMap[sourceIndex] - skipCount, obj);
                if (!(onOffsEnabled?[sourceIndex] ?? true))
                    skipCount++;
                sourceIndex++;
            }

            return values;
        }
    }
}
