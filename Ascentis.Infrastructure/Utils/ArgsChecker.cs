using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class ArgsChecker
    {
        public delegate object[] ExceptionParamsBuilder();

        public static object[] EArgs(params object[] exceptionArgs)
        {
            return exceptionArgs;
        }

        public static void CheckForNull<TE>(IEnumerable<object> args, ExceptionParamsBuilder exceptionArgsBuilder) where TE : Exception
        {
            if (args.Any(arg => arg == null))
                throw GenericObjectBuilder.Build<TE>(exceptionArgsBuilder());
        }
    }
}
