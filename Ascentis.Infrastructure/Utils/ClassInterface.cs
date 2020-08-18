using System;
using System.Reflection;

/*
   This class allows to build an "interface" into a class (NOT an object)

   C# doesn't support interfaces into plain classes nor static virtual methods
   therefore making it hard to create a hierarchy of classes that can depend on logic 
   overridden by descendant classes using virtual static methods.

   With this class an "interface" can be obtained to a class essentially hooking up to
   the target classes by assigning delegates in descendant classes of ClassInterface that
   match public static methods in the target class (or parents) by name.

   In order to implement a few such as "overridden" static methods simply declare methods 
   in the descendant class using the "new" modified and the new version of the method will be
   picked up by ClassInterface.
   
   If the prototype of the delegates don't match the target class public static methods
   then the reflection based property assignment will fail with System.ArgumentException. 
*/

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ClassInterface
    {
        public ClassInterface(Type targetType)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            foreach (var prop in GetType().GetProperties())
            {
                if (!typeof(Delegate).IsAssignableFrom(prop.PropertyType))
                    continue;
                var method = GenericMethod.BuildMethodDelegate(prop.Name, targetType, prop.PropertyType, bindingFlags);
                prop.SetValue(this, method);
            }
        }
    }
    
    public class ClassInterface<TClass> : ClassInterface
    {
        public ClassInterface() : base(typeof(TClass)) { }
    }
}
