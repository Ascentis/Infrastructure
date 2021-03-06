﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

/*
   This class allows to build an "interface" into a class (NOT an object)

   C# doesn't support interfaces into plain classes nor static virtual methods
   therefore making it hard to create a hierarchy of classes that can depend on logic 
   overridden by descendant classes using virtual static methods.

   With this class an "interface" can be obtained to a class essentially hooking up to
   the target classes by assigning delegates in descendant classes of ClassInterface that
   match public static methods in the target class (or parents) by name.

   In order to implement a few such "overridden" static methods simply declare methods 
   in the descendant class using the "new" modifier and the new version of the method will be
   picked up by ClassInterface.
   
   If the prototype of the delegates don't match the target class public static methods
   then the reflection based property assignment will fail with System.ArgumentException. 
*/

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    [SuppressMessage("ReSharper", "ConvertToLambdaExpression")]
    public class ClassInterface
    {
        private sealed class TypeNamePair : Tuple<Type, string>
        {
            internal TypeNamePair(Type type, string name) : base(type, name) {}
            internal Type Type => Item1;
            internal string Name => Item2;
        }

        private static readonly ConcurrentDictionary<TypeNamePair, Delegate> StaticMethodDelegatesCache;
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> ClassInterfaceDelegatePropertiesCache;

        static ClassInterface()
        {
            StaticMethodDelegatesCache = new ConcurrentDictionary<TypeNamePair, Delegate>();
            ClassInterfaceDelegatePropertiesCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        }

        public ClassInterface(Type targetType)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            var properties = ClassInterfaceDelegatePropertiesCache.GetOrAdd(GetType(), type =>
            {
                return type.GetProperties().Where(prop => typeof(Delegate).IsAssignableFrom(prop.PropertyType)).ToList();
            });
            foreach (var prop in properties)
            {
                var method = StaticMethodDelegatesCache.GetOrAdd(new TypeNamePair(targetType, prop.Name), cacheKey =>
                {
                    return GenericMethod.BuildMethodDelegate(cacheKey.Name, cacheKey.Type, prop.PropertyType, bindingFlags);
                });
                prop.SetValue(this, method);
            }
        }
    }
    
    public class ClassInterface<TClass> : ClassInterface
    {
        public ClassInterface() : base(typeof(TClass)) { }
    }
}
