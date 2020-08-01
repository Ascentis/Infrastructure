namespace Ascentis.Infrastructure
{
    public static class AutoInit
    { 
        public static T Ref<T>(ref T obj) where T : new()
        {
            return obj ??= new T();
        }

        public static T Ref<T>(ref T obj, params object[] args)
        {
            return obj ??= GenericObjectBuilder.Build<T>(args);
        }
    }
}
