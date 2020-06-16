using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Ascentis.Infrastructure
{
    [Serializable]
    public class InternalMemoryCache : IEnumerable<KeyValuePair<string, object>>
    {
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, ExternalCacheItem>> _caches = new ConcurrentDictionary<string, ConcurrentDictionary<string, ExternalCacheItem>>();
        private ConcurrentDictionary<string, ExternalCacheItem> _cache;

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
                if (value == Name) return;
                _name = value;
                _cache = _caches.GetOrAdd(Name, (key) => new ConcurrentDictionary<string, ExternalCacheItem>());
            }
        }

        public InternalMemoryCache()
        {
            Name = "default";
            _cache = _caches.GetOrAdd(Name, (key) => new ConcurrentDictionary<string, ExternalCacheItem>());
        }

        public InternalMemoryCache(string name)
        {
            Name = name;
            _cache = _caches.GetOrAdd(name, (key) => new ConcurrentDictionary<string, ExternalCacheItem>());
        }

        public bool Add(ExternalCacheItem item)
        {
            return _cache.TryAdd(item.Key, item);
        }

        public object AddOrGetExisting(ExternalCacheItem item)
        {
            var aItem = _cache.GetOrAdd(item.Key, key => item);
            return aItem?.Value;
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        public object Get(string key)
        {
            return _cache.TryGetValue(key, out var item) ? item.Value : null;
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
            var tmpDict = new Dictionary<string, object>();
            foreach (var item in _cache) 
                tmpDict.Add(item.Key, item.Value.Value);
            return tmpDict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
