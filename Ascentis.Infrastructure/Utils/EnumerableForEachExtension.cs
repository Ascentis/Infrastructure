using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class EnumerableForEachExtension
    {
        public delegate void Method<in T>(T adapter);

        public static AggregateException ForEach<T>(this IEnumerable<T> adapters, Method<T> method,
            bool throwException = true)
        {
            return ForEach(adapters, method, typeof(Exception), throwException);
        }

        public static AggregateException ForEach<T>(this IEnumerable<T> adapters, Method<T> method, Type exceptionType, bool throwException = true)
        {
            List<Exception> exceptions = null;
            foreach (var adapter in adapters)
                try
                {
                    method(adapter);
                }
                catch (Exception e)
                {
                    if (exceptionType.IsInstanceOfType(e))
                        AutoInit.Ref(ref exceptions).Add(e);
                    else
                        throw;
                }

            if (throwException && exceptions != null)
                throw new AggregateException(exceptions);
            return exceptions != null ? new AggregateException(exceptions) : null;
        }
    }
}
