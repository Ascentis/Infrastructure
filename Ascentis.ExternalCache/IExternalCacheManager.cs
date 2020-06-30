using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("5e2151b8-6bc4-49f1-b30d-c33ac9c3a211")]
    public interface IExternalCacheManager
    {
        void ClearAllCaches();
        void SetCacheExpirationCycleCheck(int cycleMs);
    }
}