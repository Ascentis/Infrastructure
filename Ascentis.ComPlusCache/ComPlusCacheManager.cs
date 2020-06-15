using System;

namespace Ascentis.Infrastructure
{
    public class ComPlusCacheManager : IDisposable
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

        public void Dispose()
        {
            try
            {
                _externalCacheManager.NonRetriable(externalCacheManager => externalCacheManager.Dispose());
            }
            catch (Exception)
            {
                // Ignore. COM+ object could be dead
            }
        }
    }
}
