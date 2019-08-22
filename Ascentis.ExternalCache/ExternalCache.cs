using System.Runtime.Caching;
using System;
using System.Collections.Concurrent;

namespace Ascentis.Infrastructure
{
    public class ExternalCache : System.EnterpriseServices.ServicedComponent
    {
        private static readonly ConcurrentDictionary<string, MemoryCache> Caches;

        static ExternalCache()
        {
            Caches = new ConcurrentDictionary<string, MemoryCache>();
        }

        private MemoryCache _cache;
        private MemoryCache Cache
        {
            get
            {
                CheckCache();
                return _cache;
            }
            set => _cache = value;
        }

        // ReSharper disable once EmptyConstructor
        public ExternalCache() {}

        public void Select(string cacheName)
        {
            // ReSharper disable once InconsistentNaming
            Cache = cacheName == "default" ? MemoryCache.Default : Caches.GetOrAdd(cacheName, _cacheName => new MemoryCache(_cacheName));
        }

        private void CheckCache()
        {
            if (_cache != null) return;
            _cache = MemoryCache.Default;
        }

        public bool Add(string key, object item)
        {
            return Cache.Add(new CacheItem(key, item), null);
        }
        public bool Add(string key, object item, DateTimeOffset absoluteExpiration)
        {
            return Cache.Add(key, item, absoluteExpiration);
        }

        public bool Add(string key, object item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy {SlidingExpiration = slidingExpiration};
            return Cache.Add(key, item, cacheItemPolicy);
        }

        public object AddOrGetExisting(string key, object value)
        {
            return Cache.AddOrGetExisting(key, value, null);
        }

        public bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public object Get(string key)
        {
            return Cache.Get(key);
        }

        public object Remove(string key)
        {
            return Cache.Remove(key);
        }

        public void Set(string key, object value, DateTimeOffset absoluteExpiration)
        {
            Cache.Set(key, value, absoluteExpiration);
        }

        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy {SlidingExpiration = slidingExpiration};
            Cache.Set(key, value, cacheItemPolicy);
        }
    }
}
