using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    /* Don't use this class directly, but rather do it thorough ComPlusCacheManager. ComPlusCacheManager can handle COM+ service crashes
       and automatically retry operations */

    [Guid("1f12e287-8b0d-446b-80dd-7c02fe4d339e")]
    public class ExternalCacheManager : System.EnterpriseServices.ServicedComponent, IExternalCacheManager
    {
        public void ClearAllCaches()
        {
            SimpleMemoryCache.ClearAll();
        }

        public void SetCacheExpirationCycleCheck(int cycleMs)
        {
            SimpleMemoryCache.SetCacheExpirationCycleCheck(cycleMs);
        }
    }
}