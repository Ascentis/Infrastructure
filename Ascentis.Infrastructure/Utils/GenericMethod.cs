using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{ 
    public static class GenericMethod
    {
        public static TDelegate BuildMethodDelegate<TDelegate, TClass>(string methodName) where TDelegate : Delegate
        {
            var type = typeof(TClass);
            var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            ArgsChecker.CheckForNull<NullReferenceException>(methodInfo, $"Method {methodName} not found in {typeof(TClass).Name} class");
            // ReSharper disable once PossibleNullReferenceException
            return (TDelegate)methodInfo.CreateDelegate(typeof(TDelegate));
        }
    }
}
