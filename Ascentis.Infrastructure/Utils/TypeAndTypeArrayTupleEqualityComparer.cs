using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class TypeAndTypeArrayTupleEqualityComparer : IEqualityComparer<Tuple<Type, Type[]>>, IEqualityComparer
    {
        public bool Equals(Tuple<Type, Type[]> x, Tuple<Type, Type[]> y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (x.Item1 == null)
                throw new ArgumentNullException(nameof(x.Item1));
            if (y.Item1 == null)
                throw new ArgumentNullException(nameof(y.Item1));
            if (x.Item2 == null)
                throw new ArgumentNullException(nameof(x.Item2));
            if (y.Item2 == null)
                throw new ArgumentNullException(nameof(y.Item2));
            if (x.Item1 != y.Item1)
                return false;
            return x.Item2.Length == y.Item2.Length && !x.Item2.Where((t, i) => t != y.Item2[i]).Any();
        }

        public int GetHashCode(Tuple<Type, Type[]> obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (obj.Item1 == null)
                throw new ArgumentNullException(nameof(obj.Item1));
            if (obj.Item2 == null)
                throw new ArgumentNullException(nameof(obj.Item2));
            var result = obj.Item1.GetHashCode();
            foreach (var type in obj.Item2) 
                result ^= type != null ? type.GetHashCode() : 0;
            return result;
        }

        public new bool Equals(object x, object y)
        {
            return Equals((Tuple<Type, Type[]>) x, (Tuple<Type, Type[]>) y);
        }

        public int GetHashCode(object obj)
        {
            return GetHashCode((Tuple<Type, Type[]>) obj);
        }
    }
}
