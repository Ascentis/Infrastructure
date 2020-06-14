using System;

namespace Ascentis.Infrastructure
{
    public class ComPlusCacheManager : IDisposable
    {
        private readonly SolidComPlus<ExternalCacheManager> _externalCacheManager;

        public ComPlusCacheManager()
        {
            _externalCacheManager = new SolidComPlus<ExternalCacheManager>();
        }

        public void ClearAllCaches()
        {
            _externalCacheManager.Retriable(cacheManager => cacheManager.ClearAllCaches());
        }

        public void Dispose()
        {
            _externalCacheManager.Retriable(externalCacheManager => externalCacheManager.Dispose());
        }
    }
}
