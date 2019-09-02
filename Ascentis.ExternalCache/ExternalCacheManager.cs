using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("1f12e287-8b0d-446b-80dd-7c02fe4d339e")]
    public class ExternalCacheManager : System.EnterpriseServices.ServicedComponent, IExternalCacheManager
    {
        public void ClearAllCaches()
        {
            foreach (var cacheContainer in ExternalCache.Caches)
            {
                cacheContainer.Value.SwapNewAndExecute(cache => cache.GetCount() > 0, cache =>
                {
                    cache.Trim(100);
                    cache.Dispose();
                });
            }
        }
    }
}