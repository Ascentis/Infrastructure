using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class GenericObjectBuilder
    {
        public delegate T ConstructorDelegate<out T>(params object[] args);

        private delegate object ConstructorDelegate(params object[] args);
        private static readonly ConcurrentDictionary<Tuple<Type, Type[]>, ConstructorDelegate> CachedConstructorDelegates;

        public static int CachedConstructorDelegatesCount => CachedConstructorDelegates.Count;

        static GenericObjectBuilder()
        {
            CachedConstructorDelegates = new ConcurrentDictionary<Tuple<Type, Type[]>, ConstructorDelegate>(new TypeAndTypeArrayTupleEqualityComparer());
        }

        public static void ClearConstructorDelegatesCache()
        {
            CachedConstructorDelegates.Clear();
        }

        private static ConstructorDelegate CreateConstructor(Type type, params Type[] parameters)
        {
            var constructorInfo = type.GetConstructor(parameters);
            if (constructorInfo == null)
                throw new InvalidOperationException("Could not find constructor with the supplied parameter types");

            // define a object[] parameter
            var paramExpr = Expression.Parameter(typeof(Object[]));

            // To feed the constructor with the right parameters, we need to generate an array 
            // of parameters that will be read from the initialize object array argument.
            var constructorParameters = parameters.Select((paramType, index) =>
                // convert the object[index] to the right constructor parameter type.
                Expression.Convert(
                    // read a value from the object[index]
                    Expression.ArrayAccess(paramExpr, Expression.Constant(index)), paramType)).ToArray();
            // just call the constructor.
            var body = Expression.New(constructorInfo, constructorParameters.ToArray<Expression>());

            var constructor = Expression.Lambda<ConstructorDelegate>(body, paramExpr);
            return constructor.Compile();
        }

        public static T Build<T>(params object[] args)
        {
            var paramTypes = new Type[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    throw new ArgumentException("No parameter can be null calling Build<T>(params)");
                paramTypes[i] = args[i].GetType();
            }

            return Build<T>(paramTypes, args);
        }

        private static ConstructorDelegate GetConstructor(Type type, Type[] paramTypes)
        {
            return CachedConstructorDelegates.GetOrAdd(new Tuple<Type, Type[]>(type, paramTypes), k => CreateConstructor(k.Item1, k.Item2));
        }

        public static T Build<T>(Type[] paramTypes, params object[] args)
        {
            var type = typeof(T);
            var constructor = GetConstructor(type, paramTypes);
            return (T) constructor(args);
        }

        public static ConstructorDelegate<T> Builder<T>(params Type[] paramTypes)
        {
            var type = typeof(T);
            var constructor =  GetConstructor(type, paramTypes);
            return args => (T) constructor(args);
        }
    }
}
