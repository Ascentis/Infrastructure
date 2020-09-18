using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class ArgsChecker
    {
        public static ArgNamePair Arg(object arg, params object[] args)
        {
            return new ArgNamePair(arg, args);
        }

        public static void CheckForNull<TE>(IEnumerable<ArgNamePair> args) where TE : Exception
        {
            foreach (var arg in args)
                if (arg.Arg == null)
                    throw GenericObjectBuilder.Build<TE>(arg.ExceptionArgs);
        }

        public static void CheckForNull<TE>(IEnumerable<object> args, params object[] exceptionArgs) where TE : Exception
        {
            if (args.Any(arg => arg == null))
                throw GenericObjectBuilder.Build<TE>(exceptionArgs);
        }

        public static void CheckForNull<TE>(object arg, params object[] exceptionArgs) where TE : Exception
        {
            if (arg == null)
                throw GenericObjectBuilder.Build<TE>(exceptionArgs);
        }

        public static void Check<TE>(bool condition, params object[] exceptionArgs) where TE : Exception
        {
            if (!condition)
                throw GenericObjectBuilder.Build<TE>(exceptionArgs);
        }
    }
}
