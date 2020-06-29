using System;
using System.Runtime.Caching;

namespace Ascentis.Infrastructure
{
    public class SimpleCacheItem : CacheItem
    {
        public CacheItemPolicy Policy { get; set; }
        public TimeSpan OriginalSlidingExpiration { get; }

        public SimpleCacheItem(string key, object value, CacheItemPolicy policy = null) : base(key, value)
        {
            Policy = policy;
            if (policy != null) OriginalSlidingExpiration = policy.SlidingExpiration;
        }
    }
}
