using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class ArgsChecker
    {
        public static ArgNamePair Arg(object arg, string argName)
        {
            return new ArgNamePair(arg, argName);
        }

        public static void CheckForNull<TE>(IEnumerable<ArgNamePair> args) where TE : Exception
        {
            foreach (var arg in args)
                if (arg.Arg == null)
                    throw GenericObjectBuilder.Build<TE>(arg.ArgName);
        }

        public static void CheckForNull<TE>(IEnumerable<object> args, string exceptionStr) where TE : Exception
        {
            if (args.Any(arg => arg == null))
                throw GenericObjectBuilder.Build<TE>(exceptionStr);
        }

        public static void CheckForNull<TE>(object arg, string argName) where TE : Exception
        {
            CheckForNull<TE>(new [] {Arg(arg, argName)});
        }
    }
}
