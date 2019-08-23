using System.Runtime.Caching;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("78088bd8-739f-4397-adba-cc7ea259e654")]
    public class ExternalCache : System.EnterpriseServices.ServicedComponent
    {
        public static readonly ConcurrentDictionary<string, MemoryCache> Caches;

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
            Cache = cacheName == "default"
                ? MemoryCache.Default
                : Caches.GetOrAdd(cacheName, _cacheName => new MemoryCache(_cacheName));
        }

        private void CheckCache()
        {
            if (_cache != null) return;
            _cache = MemoryCache.Default;
        }

        private static ExternalCacheItem BuildCacheItem(object source)
        {
            var cacheItem = new ExternalCacheItem();
            cacheItem.CopyFrom(source);
            return cacheItem;
        }

        public bool Add(string key, object item)
        {
            return Cache.Add(new CacheItem(key, BuildCacheItem(item)), null);
        }

        public bool Add(string key, object item, DateTimeOffset absoluteExpiration)
        {
            return Cache.Add(key, BuildCacheItem(item), absoluteExpiration);
        }

        public bool Add(string key, object item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = slidingExpiration };
            return Cache.Add(key, BuildCacheItem(item), cacheItemPolicy);
        }

        public object AddOrGetExisting(string key, object value)
        {
            return Cache.AddOrGetExisting(key, BuildCacheItem(value), null);
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
            Cache.Set(key, BuildCacheItem(value), absoluteExpiration);
        }

        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = slidingExpiration };
            Cache.Set(key, BuildCacheItem(value), cacheItemPolicy);
        }

        public void Clear()
        {
            Cache.Trim(100);
        }
    }
}
