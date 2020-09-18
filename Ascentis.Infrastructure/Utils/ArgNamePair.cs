// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ArgNamePair
    {
        public object Arg { get; }
        public object[] ExceptionArgs { get; }

        public ArgNamePair(object arg, params object[] exceptionArgs)
        {
            Arg = arg;
            ExceptionArgs = exceptionArgs;
        }
    }
}
