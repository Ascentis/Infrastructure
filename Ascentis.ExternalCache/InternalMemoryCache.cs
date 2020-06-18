using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ascentis.Infrastructure
{
    [Serializable]
    internal class InternalMemoryCache : IEnumerable<KeyValuePair<string, object>>
    {
        private const int DefaultExpireCycleCheck = 5000;
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, ExternalCacheItem>> _caches = new ConcurrentDictionary<string, ConcurrentDictionary<string, ExternalCacheItem>>();
        private ConcurrentDictionary<string, ExternalCacheItem> _cache;
        private static int _lastExpirerRunTicks;

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

        static InternalMemoryCache()
        {
            _expireTimer.Change(DefaultExpireCycleCheck, DefaultExpireCycleCheck);
        }

        public static void SetCacheExpirationCycleCheck(int cycleMs)
        {
            _expireTimer.Change(cycleMs, cycleMs);
        }

        private string _name;

        public static void ClearAll()
        {
            foreach(var cache in _caches)
                cache.Value.Clear();
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == Name)
                    return;
                _name = value;
                _cache = _caches.GetOrAdd(Name, (key) => new ConcurrentDictionary<string, ExternalCacheItem>());
            }
        }

        public InternalMemoryCache()
        {
            Name = "default";
        }

        public InternalMemoryCache(string name)
        {
            Name = name;
        }

        private object KickSlidingExpirationForward(ExternalCacheItem item)
        {
            if (item != null && item.OriginalSlidingExpiration != TimeSpan.Zero)
                item.Policy.SlidingExpiration = item.OriginalSlidingExpiration;
            return item?.Value;
        }

        public bool Add(ExternalCacheItem item)
        {
            return _cache.TryAdd(item.Key, item);
        }

        public object AddOrGetExisting(ExternalCacheItem item)
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

        public void Set(ExternalCacheItem item)
        {
            _cache.AddOrUpdate(item.Key, (key) => item, (key, oldValue) => item);
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
                list.Add(new KeyValuePair<string, object>(item.Key, item.Value));
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
