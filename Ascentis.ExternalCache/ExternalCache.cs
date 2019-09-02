using System.Runtime.Caching;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Ascentis.Framework;

namespace Ascentis.Infrastructure
{
    [Guid("78088bd8-739f-4397-adba-cc7ea259e654")]
    public class ExternalCache : System.EnterpriseServices.ServicedComponent, IExternalCache
    {
        private const string DefaultMemoryCacheName = "default";
        public static readonly ConcurrentDictionary<string, ConcurrentObjectAccessor<MemoryCache>> Caches;
        private static readonly CacheItemPolicy DefaultCacheItemPolicy;

        static ExternalCache()
        {
            Caches = new ConcurrentDictionary<string, ConcurrentObjectAccessor<MemoryCache>>();
            DefaultCacheItemPolicy = new CacheItemPolicy
            {
                /* We need to use an async "disposer" with IDisposable items in the cache because the MemoryCache.Remove() apparently
                   keeps a reference to the removed item and tries to do something with it even after the removal callback is called.
                   This causes an exception when Remove() method is called if calling Dispose() method directly in RemovedCallback.
                   Something worth nothing to is that the pattern used to actually dequeue and dispose items is one where the full
                   queue is copied over to the disposing thread and new queue initialized. This is to avoid the problem of a call to
                   RemovedCallback causing an item getting inserted into the queue and getting picked up right away before the callback
                   returns. If that were to happen the result will be the same as if calling Dispose() within the RemovedCallback delegate */
                RemovedCallback = arguments =>
                {
                    if (!(arguments.CacheItem.Value is IDisposable disposableItem))
                        return;
                    AsyncDisposer.Enqueue(disposableItem);
                }
            };
        }

        private ConcurrentObjectAccessor<MemoryCache> _cache;

        private ConcurrentObjectAccessor<MemoryCache> Cache
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
            if (cacheName == DefaultMemoryCacheName)
                cacheName = "_" + DefaultMemoryCacheName;
            Cache = Caches.GetOrAdd(cacheName, name => new ConcurrentObjectAccessor<MemoryCache>(name, null));
        }

        private void CheckCache()
        {
            if (_cache != null)
                return;
            Select(DefaultMemoryCacheName);
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
            return (bool) Cache.ExecuteReadLocked( cache => cache.Add(new CacheItem(key, BuildCacheItem(item)), DefaultCacheItemPolicy));
        }

        public bool Add(string key, string item)
        {
            return (bool) Cache.ExecuteReadLocked( cache => cache.Add(new CacheItem(key, item), DefaultCacheItemPolicy));
        }

        public bool Add(string key, object item, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            return (bool) Cache.ExecuteReadLocked( cache => cache.Add(key, BuildCacheItem(item), policy));
        }

        public bool Add(string key, string item, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            return (bool) Cache.ExecuteReadLocked( cache => cache.Add(key, item, policy));
        }

        public bool Add(string key, object item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            return (bool) Cache.ExecuteReadLocked( cache => cache.Add(key, BuildCacheItem(item), cacheItemPolicy));
        }

        public bool Add(string key, string item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            return (bool) Cache.ExecuteReadLocked( cache => cache.Add(key, item, cacheItemPolicy));
        }

        public object AddOrGetExisting(string key, object value)
        {
            return Cache.ExecuteReadLocked( cache => cache.AddOrGetExisting(key, BuildCacheItem(value), DefaultCacheItemPolicy));
        }

        public object AddOrGetExisting(string key, string value)
        {
            return Cache.ExecuteReadLocked( cache => cache.AddOrGetExisting(key, value, DefaultCacheItemPolicy));
        }

        public bool Contains(string key)
        {
            return (bool) Cache.ExecuteReadLocked( cache => cache.Contains(key));
        }

        public object Get(string key)
        {
            return Cache.ExecuteReadLocked( cache => cache.Get(key));
        }

        public object Remove(string key)
        {
            return Cache.ExecuteReadLocked( cache => cache.Remove(key));
        }

        public void Set(string key, object value, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            Cache.ExecuteReadLocked( cache => cache.Set(key, BuildCacheItem(value), policy));
        }

        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            Cache.ExecuteReadLocked( cache => cache.Set(key, BuildCacheItem(value), cacheItemPolicy));
        }

        public void Set(string key, string value, DateTime absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            Cache.ExecuteReadLocked( cache => cache.Set(key, value, policy));
        }

        public void Set(string key, string value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            Cache.ExecuteReadLocked( cache => cache.Set(key, value, cacheItemPolicy));
        }

        public void Clear()
        {
            Cache.ExecuteReadLocked( cache => cache.Trim(100));
        }
    }
}
