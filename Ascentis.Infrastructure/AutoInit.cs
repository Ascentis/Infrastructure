namespace Ascentis.Infrastructure
{
    public static class AutoInit
    { 
        public static T Ref<T>(ref T exceptions) where T : new()
        {
            return exceptions ??= new T();
        }
    }
}
