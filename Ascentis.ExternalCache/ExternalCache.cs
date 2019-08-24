using System.Runtime.Caching;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ascentis.Infrastructure
{
    [Guid("78088bd8-739f-4397-adba-cc7ea259e654")]
    public class ExternalCache : System.EnterpriseServices.ServicedComponent, IExternalCache
    {
        private const string DefaultMemoryCacheName = "default";
        public static readonly ConcurrentDictionary<string, MemoryCache> Caches;
        private static readonly CacheItemPolicy DefaultCacheItemPolicy;
        private static readonly Timer DisposalTimer;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly ConcurrentQueue<IDisposable> Disposables;

        static ExternalCache()
        {
            const int disposeInterval = 5000;
            Disposables = new ConcurrentQueue<IDisposable>();
            Caches = new ConcurrentDictionary<string, MemoryCache>();
            DefaultCacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = arguments =>
                {
                    if (!(arguments.CacheItem.Value is IDisposable disposableItem))
                        return;
                    DisposalTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    DisposalTimer.Change(disposeInterval, disposeInterval);
                    Disposables.Enqueue(disposableItem);
                }
            };

            (DisposalTimer = new Timer(state =>
            {
                while(Disposables.TryDequeue(out var item))
                    item.Dispose();
            })).Change(disposeInterval, disposeInterval);
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
            Cache = cacheName == DefaultMemoryCacheName
                ? MemoryCache.Default
                : Caches.GetOrAdd(cacheName, _cacheName => new MemoryCache(_cacheName));
        }

        private void CheckCache()
        {
            if (_cache != null)
                return;
            Select(DefaultMemoryCacheName);
        }

        private static ExternalCacheItem BuildCacheItem(object source)
        {
            var cacheItem = new ExternalCacheItem();
            cacheItem.CopyFrom(source);
            return cacheItem;
        }

        public bool Add(string key, object item)
        {
            return Cache.Add(new CacheItem(key, BuildCacheItem(item)), DefaultCacheItemPolicy);
        }

        public bool Add(string key, string item)
        {
            return Cache.Add(new CacheItem(key, item), DefaultCacheItemPolicy);
        }

        public bool Add(string key, object item, DateTimeOffset absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            return Cache.Add(key, BuildCacheItem(item), policy);
        }

        public bool Add(string key, string item, DateTimeOffset absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            return Cache.Add(key, item, policy);
        }

        public bool Add(string key, object item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            return Cache.Add(key, BuildCacheItem(item), cacheItemPolicy);
        }

        public bool Add(string key, string item, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            return Cache.Add(key, item, cacheItemPolicy);
        }

        public object AddOrGetExisting(string key, object value)
        {
            return Cache.AddOrGetExisting(key, BuildCacheItem(value), DefaultCacheItemPolicy);
        }

        public object AddOrGetExisting(string key, string value)
        {
            return Cache.AddOrGetExisting(key, value, DefaultCacheItemPolicy);
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
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            Cache.Set(key, BuildCacheItem(value), policy);
        }

        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            Cache.Set(key, BuildCacheItem(value), cacheItemPolicy);
        }

        public void Set(string key, string value, DateTimeOffset absoluteExpiration)
        {
            var policy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                AbsoluteExpiration = absoluteExpiration
            };
            Cache.Set(key, value, policy);
        }

        public void Set(string key, string value, TimeSpan slidingExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                RemovedCallback = DefaultCacheItemPolicy.RemovedCallback,
                SlidingExpiration = slidingExpiration
            };
            Cache.Set(key, value, cacheItemPolicy);
        }

        public void Clear()
        {
            Cache.Trim(100);
        }
    }
}
