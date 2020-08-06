using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public static class Except
    {
        public delegate void Method();

        public static void Silence<TE>(Method method) where TE : Exception
        {
            try
            {
                method();
            }
            catch (TE)
            {

            }
        }
    }
}
