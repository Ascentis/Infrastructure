using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;

namespace Ascentis.Infrastructure
{
    [Serializable]
    public class SimpleMemoryCache : IEnumerable<KeyValuePair<string, object>>
    {
        private const int DefaultExpireCycleCheck = 5000;
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, SimpleCacheItem>> _caches = new ConcurrentDictionary<string, ConcurrentDictionary<string, SimpleCacheItem>>();
        private static int _lastExpirerRunTicks;

        public delegate object ValueBuilder(string key);

        private ConcurrentDictionary<string, SimpleCacheItem> _cache;

        private static Timer _expireTimer = new Timer(source =>
        {
            if (_lastExpirerRunTicks == 0)
                _lastExpirerRunTicks = Environment.TickCount;
            foreach (var cache in _caches)
                   foreach (var item in cache.Value)
                   {
                       if (item.Value.Policy.SlidingExpiration != TimeSpan.Zero)
                       { 
                           item.Value.Policy.SlidingExpiration = new TimeSpan(item.Value.Policy.SlidingExpiration.Ticks - Math.Abs(Environment.TickCount - _lastExpirerRunTicks) * 10000);
                           if (item.Value.Policy.SlidingExpiration.Ticks <= 0)
                           {
                               // ReSharper disable once UnusedVariable
                               cache.Value.TryRemove(item.Key, out var oldItem);
                           }
                       } else if (item.Value.Policy.AbsoluteExpiration <= DateTime.Now)
                           // ReSharper disable once UnusedVariable
                           cache.Value.TryRemove(item.Key, out var oldItem);
                   }
            _lastExpirerRunTicks = Environment.TickCount;
        });

        static SimpleMemoryCache()
        {
            _expireTimer.Change(DefaultExpireCycleCheck, DefaultExpireCycleCheck);
        }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (value == Name)
                    return;
                _name = value;
                _cache = _caches.GetOrAdd(Name, (key) => new ConcurrentDictionary<string, SimpleCacheItem>());
            }
        }

        public static void SetCacheExpirationCycleCheck(int cycleMs)
        {
            _expireTimer.Change(cycleMs, cycleMs);
        }

        public static void ClearAll()
        {
            foreach(var cache in _caches)
                cache.Value.Clear();
        }

        public SimpleMemoryCache()
        {
            Name = "default";
        }

        public SimpleMemoryCache(string name)
        {
            Name = name;
        }

        private object KickSlidingExpirationForward(SimpleCacheItem item)
        {
            if (item != null && item.OriginalSlidingExpiration != TimeSpan.Zero)
                item.Policy.SlidingExpiration = item.OriginalSlidingExpiration;
            return item?.Value;
        }

        public bool Add(SimpleCacheItem item)
        {
            return _cache.TryAdd(item.Key, item);
        }

        public object AddOrGetExisting(string key, ValueBuilder builder, CacheItemPolicy policy = null)
        {
            var added = false; 
            var aItem = _cache.GetOrAdd(key, k =>
            {
                added = true;
                return new SimpleCacheItem(k, builder(k), policy);
            });
            return !added ? KickSlidingExpirationForward(aItem) : null;
        }

        public object AddOrGetExisting(string key, object obj, CacheItemPolicy policy = null)
        {
            return AddOrGetExisting(key, k => obj, policy);
        }

        public object AddOrGetExisting(SimpleCacheItem item)
        {
            var added = false;
            var aItem = _cache.GetOrAdd(item.Key, key =>
            {
                added = true;
                return item;
            });
            return !added ? KickSlidingExpirationForward(aItem) : null;
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        public object Get(string key)
        {
            return _cache.TryGetValue(key, out var item) ? KickSlidingExpirationForward(item) : null;
        }

        public object Remove(string key)
        {
            return _cache.TryRemove(key, out var item) ? item.Value : null;
        }

        public void Set(SimpleCacheItem item)
        {
            _cache.AddOrUpdate(item.Key, key => item, (key, oldValue) => item);
        }

        public void Set(string key, ValueBuilder builder, CacheItemPolicy policy = null)
        {
            _cache.AddOrUpdate(key, 
                k => new SimpleCacheItem(k, builder(key), policy), 
                (k, oldValue) => new SimpleCacheItem(k, builder(k), policy));
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public bool IsEmpty()
        {
            return _cache.IsEmpty;
        }
        
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            var list = new List<KeyValuePair<string, object>>();
            foreach (var item in _cache) 
                list.Add(new KeyValuePair<string, object>(item.Key, item.Value.Value));
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
