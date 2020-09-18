using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class Spinner
    {
        public static void Spin(ref SpinWait? spinner)
        {
            spinner ??= new SpinWait();
            // ReSharper disable once ConstantConditionalAccessQualifier
            spinner?.SpinOnce();
        }
    }
}
