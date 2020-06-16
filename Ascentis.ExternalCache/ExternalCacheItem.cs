using System.Runtime.Caching;

namespace Ascentis.Infrastructure
{
    public class ExternalCacheItem : CacheItem
    {
        public CacheItemPolicy Policy { get; set; }

        public ExternalCacheItem(string key, object value, CacheItemPolicy policy = null) : base(key, value)
        {
            Policy = policy;
        }
    }
}
