using System;
using System.Collections;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class Disposer
    {
        public static void Dispose<T>(ref T obj)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (obj == null)
                return;
            if (obj is IList list)
                Dispose(list);
            else if (obj is IEnumerable enumerable)
                Dispose(enumerable);
            if (obj is IDisposable disposable)
                disposable.Dispose();
            obj = default;
        }

        public static void Dispose(IList objs)
        {
            for (var i = 0; i < objs.Count; i++)
            {
                if (!(objs[i] is IDisposable disposable)) 
                    continue;
                disposable.Dispose();
                objs[i] = null;
            }
        }

        public static void Dispose(IEnumerable objs)
        {
            foreach(var obj in objs)
            {
                if (!(obj is IDisposable disposable))
                    continue;
                disposable.Dispose();
            }
        }
    }
}
