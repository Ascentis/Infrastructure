using System.Runtime.Caching;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("1f12e287-8b0d-446b-80dd-7c02fe4d339e")]
    public class ExternalCacheManager : System.EnterpriseServices.ServicedComponent
    {
        public void ClearAllCaches()
        {
            MemoryCache.Default.Trim(100);
            foreach (var cache in ExternalCache.Caches)
                cache.Value.Trim(100);
            ExternalCache.Caches.Clear();
        }
    }
}