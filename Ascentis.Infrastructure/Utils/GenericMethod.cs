using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{ 
    public static class GenericMethod
    {
        public const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        public static TDelegate BuildMethodDelegate<TDelegate, TClass>(string methodName) where TDelegate : Delegate
        {
            return (TDelegate) BuildMethodDelegate<TClass>(methodName, typeof(TDelegate));
        }

        public static Delegate BuildMethodDelegate<TClass>(string methodName, Type delegateType, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            return BuildMethodDelegate(methodName, typeof(TClass), delegateType, bindingFlags);
        }

        public static Delegate BuildMethodDelegate(string methodName, Type targetType, Type delegateType, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new InvalidOperationException($"{nameof(delegateType)} must be of type Delegate");
            var methodInfo = targetType.GetMethod(methodName, bindingFlags);
            ArgsChecker.CheckForNull<NullReferenceException>(methodInfo, $"Method {methodName} not found in {targetType.Name} class");
            // ReSharper disable once PossibleNullReferenceException
            return methodInfo.CreateDelegate(delegateType);
        }
    }
}
