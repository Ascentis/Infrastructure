using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    /* Don't use this class directly, but rather do it thorough ComPlusCache. ComPlusCache can handle COM+ service crashes
       and automatically retry operations */

    [Guid("78088bd8-739f-4397-adba-cc7ea259e654")]
    public class ExternalCache : System.EnterpriseServices.ServicedComponent, IExternalCache
    {
        private static readonly CacheItemPolicy DefaultCacheItemPolicy = new CacheItemPolicy();
        private InternalMemoryCache _cache;

        private InternalMemoryCache Cache
        {
            get
            {
                CheckCache();
                return _cache;
            }
        }

        // ReSharper disable once EmptyConstructor
        public ExternalCache() {}

        public void Select(string cacheName)
        {
            if (Cache.Name == cacheName) return;
            if(_cache == null)
                _cache = new InternalMemoryCache(cacheName);
            Cache.Name = cacheName;
        }

        private void CheckCache()
        {
            if (_cache != null)
                return;
            _cache = new InternalMemoryCache();
        }

        private static object BuildCacheItem(object source)
        {
            var itemType = source.GetType();
            if (Dynamo.IsPrimitive(itemType) || itemType == typeof(Dynamo))
                return source;
            return new Dynamo(source);
        }

        public bool Add(string key, object item)
        {
            return Cache.Add(new ExternalCacheItem(key, BuildCacheItem(item), DefaultCacheItemPolicy));
        }

        public bool Add(string key, string item)
        {
            return Cache.Add(new ExternalCacheItem(key, item, DefaultCacheItemPolicy));
        }

        public bool Add(string key, object item, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            return Cache.Add(new ExternalCacheItem(key, BuildCacheItem(item), policy));
        }

        public bool Add(string key, string item, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            return Cache.Add(new ExternalCacheItem(key, item, policy));
        }

        public bool Add(string key, object item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = slidingExpiration
            };
            return Cache.Add(new ExternalCacheItem(key, BuildCacheItem(item), cacheItemPolicy));
        }

        public bool Add(string key, string item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = slidingExpiration
            };
            return Cache.Add(new ExternalCacheItem(key, item, cacheItemPolicy));
        }

        public object AddOrGetExisting(string key, object value)
        {
            return Cache.AddOrGetExisting(new ExternalCacheItem(key, BuildCacheItem(value), DefaultCacheItemPolicy));
        }

        public object AddOrGetExisting(string key, string value)
        {
            return Cache.AddOrGetExisting(new ExternalCacheItem(key, value, DefaultCacheItemPolicy));
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

        public void Set(string key, object value, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            Cache.Set(new ExternalCacheItem(key, BuildCacheItem(value), policy));
        }

        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = slidingExpiration
            };
            Cache.Set(new ExternalCacheItem(key, BuildCacheItem(value), cacheItemPolicy));
        }

        public void Set(string key, string value, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            Cache.Set(new ExternalCacheItem(key, value, policy));
        }

        public void Set(string key, string value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = slidingExpiration
            };
            Cache.Set(new ExternalCacheItem(key, value, cacheItemPolicy));
        }

        public void Clear()
        {
            Cache.Clear();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return (Cache as IEnumerable<KeyValuePair<string, object>>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Cache as IEnumerable).GetEnumerator();
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public bool CompareValue(string key, object value)
        {
            var obj = Get(key);
            if (obj == null && value == null)
                return true;
            if (obj == null && value != null || obj != null && value == null)
                return false;
            return obj.GetType() == value.GetType() && value == obj;
        }

        public void SelfTest()
        {
        }
    }
}
