// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ArgNamePair
    {
        public object Arg { get; }
        public string ArgName { get; }

        public ArgNamePair(object arg, string argName)
        {
            Arg = arg;
            ArgName = argName;
        }
    }
}
