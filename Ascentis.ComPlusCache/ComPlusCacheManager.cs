using System;

namespace Ascentis.Infrastructure
{
    public class ComPlusCacheManager
    {
        private readonly SolidComPlus<IExternalCacheManager, ExternalCacheManager> _externalCacheManager;

        public ComPlusCacheManager()
        {
            _externalCacheManager = new SolidComPlus<IExternalCacheManager, ExternalCacheManager>();
        }

        public void ClearAllCaches()
        {
            _externalCacheManager.Retriable(cacheManager => cacheManager.ClearAllCaches());
        }
    }
}
