using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{ 
    public static class GenericMethod
    {
        public static TDelegate BuildMethodDelegate<TDelegate, TClass>(string methodName) where TDelegate : Delegate
        {
            return (TDelegate) BuildMethodDelegate<TClass>(methodName, typeof(TDelegate));
        }

        public static Delegate BuildMethodDelegate<TClass>(string methodName, Type delegateType)
        {
            return BuildMethodDelegate(methodName, typeof(TClass), delegateType);
        }

        public static Delegate BuildMethodDelegate(string methodName, Type targetType, Type delegateType)
        {
            var methodInfo = targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            ArgsChecker.CheckForNull<NullReferenceException>(methodInfo, $"Method {methodName} not found in {targetType.Name} class");
            // ReSharper disable once PossibleNullReferenceException
            return methodInfo.CreateDelegate(delegateType);
        }
    }
}
