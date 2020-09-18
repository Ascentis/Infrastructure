using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class ObjectExtensions
    {
        private const string SourceValueWillOverflowTargetDataType = "Source value will overflow target datatype";

        private static void Overflow()
        {
            throw new OverflowException(SourceValueWillOverflowTargetDataType);
        }

        private static long CheckValue(long value, long min, long max)
        {
            if (value < min || value > max)
                Overflow();
            return value;
        }

        private static ulong CheckValue(ulong value, ulong min, ulong max)
        {
            if (value < min || value > max)
                Overflow();
            return value;
        }

        private static double CheckValue(double value, double min, double max)
        {
            if (value < min || value > max)
                Overflow();
            return value;
        }

        private static float CheckValue(float value, float min, float max)
        {
            if (value < min || value > max)
                Overflow();
            return value;
        }

        private static decimal CheckValue(decimal value, decimal min, decimal max)
        {
            if (value < min || value > max)
                Overflow();
            return value;
        }

        private static object Convert(string input, Type targetType)
        {
            if (targetType == typeof(byte) || targetType == typeof(byte?))
                return byte.Parse(input);
            if (targetType == typeof(sbyte) || targetType == typeof(sbyte?))
                return sbyte.Parse(input);
            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return bool.Parse(input);
            if (targetType == typeof(sbyte) || targetType == typeof(sbyte?))
                return sbyte.Parse(input);
            if (targetType == typeof(short) || targetType == typeof(short?))
                return short.Parse(input);
            if (targetType == typeof(ushort) || targetType == typeof(ushort?))
                return ushort.Parse(input);
            if (targetType == typeof(int) || targetType == typeof(int?))
                return int.Parse(input);
            if (targetType == typeof(uint) || targetType == typeof(uint?))
                return uint.Parse(input);
            if (targetType == typeof(long) || targetType == typeof(long?))
                return long.Parse(input);
            if (targetType == typeof(ulong) || targetType == typeof(ulong?))
                return ulong.Parse(input);
            if (targetType == typeof(double) || targetType == typeof(double?))
                return double.Parse(input);
            if (targetType == typeof(float) || targetType == typeof(float?))
                return float.Parse(input);
            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return decimal.Parse(input);
            return input;
        }

        private static object Convert(char input, Type targetType)
        {
            return Convert(input.ToString(), targetType);
        }

        private static object Convert(short input, Type targetType)
        {
            return Convert((long)input, targetType);
        }

        private static object Convert(ushort input, Type targetType)
        {
            return Convert((ulong)input, targetType);
        }

        private static object Convert(sbyte input, Type targetType)
        {
            return Convert((short)input, targetType);
        }

        private static object Convert(byte input, Type targetType)
        {
            return Convert((ushort)input, targetType);
        }

        private static object Convert(int input, Type targetType)
        {
            return Convert((long)input, targetType);
        }

        private static object Convert(uint input, Type targetType)
        {
            return Convert((ulong)input, targetType);
        }

        private static object Convert(long input, Type targetType)
        {
            if (targetType == typeof(short) || targetType == typeof(short?))
                return (short)CheckValue(input, short.MinValue, short.MaxValue);
            if (targetType == typeof(int) || targetType == typeof(int?))
                return (int)CheckValue(input, int.MinValue, int.MaxValue);
            if (targetType == typeof(ushort) || targetType == typeof(ushort?))
                return (ushort)CheckValue(input, ushort.MinValue, ushort.MaxValue);
            if (targetType == typeof(uint) || targetType == typeof(uint?))
                return (uint)CheckValue(input, uint.MinValue, uint.MaxValue);
            if (targetType == typeof(ulong) || targetType == typeof(ulong?))
                return CheckValue((ulong)input, ulong.MinValue, ulong.MaxValue);
            return input;
        }

        private static object Convert(ulong input, Type targetType)
        {
            if (targetType == typeof(short) || targetType == typeof(short?))
                return (short)CheckValue(input, 0, (ulong)short.MaxValue);
            if (targetType == typeof(int) || targetType == typeof(int?))
                return (int)CheckValue(input, 0, int.MaxValue);
            if (targetType == typeof(long) || targetType == typeof(long?))
                return (long) CheckValue(input, 0, long.MaxValue);
            if (targetType == typeof(ushort) || targetType == typeof(ushort?))
                return (ushort)CheckValue(input, 0, (ulong)short.MaxValue);
            if (targetType == typeof(uint) || targetType == typeof(uint?))
                return (uint)CheckValue(input, 0, uint.MaxValue);
            return input;
        }

        private static object Convert(double input, Type targetType)
        {
            if (targetType == typeof(short) || targetType == typeof(short?))
                return (short)CheckValue(input, short.MinValue, short.MaxValue);
            if (targetType == typeof(int) || targetType == typeof(int?))
                return (int)CheckValue(input, int.MinValue, int.MaxValue);
            if (targetType == typeof(long) || targetType == typeof(long?))
                return (long)CheckValue(input, long.MinValue, long.MaxValue);
            if (targetType == typeof(ushort) || targetType == typeof(ushort?))
                return (ushort)CheckValue(input, ushort.MinValue, ushort.MaxValue);
            if (targetType == typeof(uint) || targetType == typeof(uint?))
                return (uint)CheckValue(input, uint.MinValue, uint.MaxValue);
            if (targetType == typeof(ulong) || targetType == typeof(ulong?))
                return (ulong)CheckValue(input, ulong.MinValue, ulong.MaxValue);
            return input;
        }

        private static object Convert(float input, Type targetType)
        {
            if (targetType == typeof(short) || targetType == typeof(short?))
                return (short)CheckValue(input, short.MinValue, short.MaxValue);
            if (targetType == typeof(int) || targetType == typeof(int?))
                return (int)CheckValue(input, int.MinValue, int.MaxValue);
            if (targetType == typeof(long) || targetType == typeof(long?))
                return (long)CheckValue(input, long.MinValue, long.MaxValue);
            if (targetType == typeof(ushort) || targetType == typeof(ushort?))
                return (ushort)CheckValue(input, ushort.MinValue, ushort.MaxValue);
            if (targetType == typeof(uint) || targetType == typeof(uint?))
                return (uint)CheckValue(input, uint.MinValue, uint.MaxValue);
            if (targetType == typeof(ulong) || targetType == typeof(ulong?))
                return (ulong)CheckValue(input, ulong.MinValue, ulong.MaxValue);
            return input;
        }

        private static object Convert(decimal input, Type targetType)
        {
            if (targetType == typeof(short) || targetType == typeof(short?))
                return (short)CheckValue(input, short.MinValue, short.MaxValue);
            if (targetType == typeof(int) || targetType == typeof(int?))
                return (int)CheckValue(input, int.MinValue, int.MaxValue);
            if (targetType == typeof(long) || targetType == typeof(long?))
                return (long)CheckValue(input, long.MinValue, long.MaxValue);
            if (targetType == typeof(ushort) || targetType == typeof(ushort?))
                return (ushort)CheckValue(input, ushort.MinValue, ushort.MaxValue);
            if (targetType == typeof(uint) || targetType == typeof(uint?))
                return (uint)CheckValue(input, uint.MinValue, uint.MaxValue);
            if (targetType == typeof(ulong) || targetType == typeof(ulong?))
                return (ulong)CheckValue(input, ulong.MinValue, ulong.MaxValue);
            return input;
        }

        private static object Convert(bool input, Type targetType)
        {
            if (targetType == typeof(short) || targetType == typeof(short?) ||
                targetType == typeof(int) || targetType == typeof(long))
                return (short)(input ? 1 : 0);
            if (targetType == typeof(ushort) || targetType == typeof(ushort?) ||
                targetType == typeof(uint) || targetType == typeof(ulong))
                return (ushort)(input ? 1 : 0);
            if (targetType == typeof(int?))
                return input ? 1 : 0;
            if (targetType == typeof(long?))
                return (long)(input ? 1 : 0);
            if (targetType == typeof(uint?))
                return (uint)(input ? 1 : 0);
            if (targetType == typeof(ulong?))
                return (ulong)(input ? 1 : 0);
            return input;
        }

        public static void SetPropertyValue(PropertyInfo prop, object target, object value)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));
            switch (value)
            {
                case string s:
                    prop.SetValue(target, Convert(s, prop.PropertyType));
                    break;
                case char c:
                    prop.SetValue(target, Convert(c, prop.PropertyType));
                    break;
                case sbyte sb:
                    prop.SetValue(target, Convert(sb, prop.PropertyType));
                    break;
                case byte bb:
                    prop.SetValue(target, Convert(bb, prop.PropertyType));
                    break;
                case short sh:
                    prop.SetValue(target, Convert(sh, prop.PropertyType));
                    break;
                case ushort us:
                    prop.SetValue(target, Convert(us, prop.PropertyType));
                    break;
                case int i:
                    prop.SetValue(target, Convert(i, prop.PropertyType));
                    break;
                case uint ui:
                    prop.SetValue(target, Convert(ui, prop.PropertyType));
                    break;
                case long l:
                    prop.SetValue(target, Convert(l, prop.PropertyType));
                    break;
                case ulong ul:
                    prop.SetValue(target, Convert(ul, prop.PropertyType));
                    break;
                case double d:
                    prop.SetValue(target, Convert(d, prop.PropertyType));
                    break;
                case float f:
                    prop.SetValue(target, Convert(f, prop.PropertyType));
                    break;
                case decimal dec:
                    prop.SetValue(target, Convert(dec, prop.PropertyType));
                    break;
                case bool b:
                    prop.SetValue(target, Convert(b, prop.PropertyType));
                    break;
                default:
                    prop.SetValue(target, value);
                    break;
            }
        }

        public static void SetPropertyValue(string propName, object target, object value)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            var propInfo = target.GetType().GetProperty(propName);
            SetPropertyValue(propInfo, target, value);
        }

        public static void SetPropertyValue(this object target, string propName, object value)
        {
            SetPropertyValue(propName, target, value);
        }

        public static void SetPropertyValue(this object target, PropertyInfo prop, object value)
        {
            SetPropertyValue(prop, target, value);
        }
    }
}
