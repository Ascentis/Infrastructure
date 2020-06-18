using System;
using System.Collections;
using System.Collections.Generic;

namespace Ascentis.Infrastructure
{
    public class ComPlusCache : IEnumerable<KeyValuePair<string, object>>, IDisposable
    {
        private const string Reserved = "-<.Reserved.>-";
        public delegate TRt ValueFactory<out TRt>();
        public delegate TRt UpdateValueFactory<out TRt>();
        private delegate object SetCacheEntry();
        private delegate object UpdateSetCacheEntry();
        private readonly DateTime _infiniteDateTime = new DateTime(9999, 1, 1);
        private readonly SolidComPlus<IExternalCache, ExternalCache> _externalCache;

        public ComPlusCache()
        {
            _externalCache = new SolidComPlus<IExternalCache, ExternalCache>();
        }

        public ComPlusCache(string name)
        {
            _externalCache = new SolidComPlus<IExternalCache, ExternalCache>(externalCache =>
            {
                externalCache.Select(name);
            });
        }

        public void Dispose()
        {
            _externalCache.Dispose();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _externalCache.Retriable(externalCache => externalCache?.GetEnumerator() ?? throw new InvalidOperationException());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _externalCache.Retriable(externalCache => ((IEnumerable) externalCache)?.GetEnumerator() ?? throw new InvalidOperationException());
        }

        public object this[string key]
        {
            get => _externalCache.Retriable(externalCache => externalCache.Get(key));
            set => _externalCache.Retriable(externalCache => externalCache.Set(key, value, _infiniteDateTime));
        }

        public bool Contains(string key)
        {
            return _externalCache.Retriable(externalCache => externalCache.Contains(key));
        }

        public bool Add(string key, object item)
        {
            return _externalCache.Retriable(externalCache => externalCache.Add(key, item));
        }

        public bool Add(string key, string item)
        {
            return _externalCache.Retriable(externalCache => externalCache.Add(key, item));
        }

        public bool Add(string key, object item, DateTime absoluteExpiration)
        {
            return _externalCache.Retriable(externalCache => externalCache.Add(key, item, absoluteExpiration));
        }

        public bool Add(string key, string item, DateTime absoluteExpiration)
        {
            return _externalCache.Retriable(externalCache => externalCache.Add(key, item, absoluteExpiration));
        }

        public bool Add(string key, object item, TimeSpan slidingExpiration)
        {
            return _externalCache.Retriable(externalCache => externalCache.Add(key, item, slidingExpiration));
        }

        public bool Add(string key, string item, TimeSpan slidingExpiration)
        {
            return _externalCache.Retriable(externalCache => externalCache.Add(key, item, slidingExpiration));
        }

        public object AddOrGetExisting(string key, object value)
        {
            return _externalCache.Retriable(externalCache => externalCache.AddOrGetExisting(key, value));
        }

        public object AddOrGetExisting(string key, string value)
        {
            return _externalCache.Retriable(externalCache => externalCache.AddOrGetExisting(key, value));
        }

        public object Get(string key)
        {
            return _externalCache.Retriable(externalCache => externalCache.Get(key));
        }

        public object Remove(string key)
        {
            return _externalCache.Retriable(externalCache => externalCache.Remove(key));
        }

        public void Set(string key, object value, DateTime absoluteExpiration)
        {
            _externalCache.Retriable(externalCache => externalCache.Set(key, value, absoluteExpiration));
        }

        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            _externalCache.Retriable(externalCache => externalCache.Set(key, value, slidingExpiration));
        }

        public void Set(string key, string value, DateTime absoluteExpiration)
        {
            _externalCache.Retriable(externalCache => externalCache.Set(key, value, absoluteExpiration));
        }

        public void Set(string key, string value, TimeSpan slidingExpiration)
        {
            _externalCache.Retriable(externalCache => externalCache.Set(key, value, slidingExpiration));
        }

        public void Clear()
        {
            _externalCache.Retriable(externalCache => externalCache.Clear());
        }

        /* ConcurrentDictionary style methods using delegates to create values only when needed */

        private TRt GetOrAdd<TRt>(string key, SetCacheEntry setCacheEntry)
        {
            TRt obj;
            do
            {
                obj = (TRt)_externalCache.Retriable(externalCache => externalCache.Get(key));
                if (obj != null)
                { 
                    if (!(obj is string s)) break;
                    if (s == Reserved) continue;
                    break;
                }
                obj = (TRt)_externalCache.Retriable(externalCache => externalCache.AddOrGetExisting(key, Reserved));
                if (obj != null) continue;
                return (TRt)setCacheEntry();
            } while (true);
            return obj;
        }

        public object GetOrAdd(string key, ValueFactory<object> builder)
        {
            return GetOrAdd(key, builder, _infiniteDateTime);
        }

        public string GetOrAdd(string key, ValueFactory<string> builder)
        {
            return GetOrAdd(key, builder, _infiniteDateTime);
        }

        public object GetOrAdd(string key, ValueFactory<object> builder, DateTime absoluteExpiration)
        {
            return GetOrAdd<object>(key, () =>
                _externalCache.Retriable(externalCache =>
                {
                    var v = builder();
                    externalCache.Set(key, v, absoluteExpiration);
                    return v;
                }));
        }

        public string GetOrAdd(string key, ValueFactory<string> builder, DateTime absoluteExpiration)
        {
            return GetOrAdd<string>(key, () =>
                _externalCache.Retriable(externalCache =>
                {
                    var v = builder();
                    externalCache.Set(key, v, absoluteExpiration);
                    return v;
                }));
        }

        public object GetOrAdd(string key, ValueFactory<object> builder, TimeSpan slidingExpiration)
        {
            return GetOrAdd<object>(key, () =>
                _externalCache.Retriable(externalCache =>
                {
                    var v = builder();
                    externalCache.Set(key, v, slidingExpiration);
                    return v; 
                }));
        }

        public string GetOrAdd(string key, ValueFactory<string> builder, TimeSpan slidingExpiration)
        {
            return GetOrAdd<string>(key, () =>
            {
                var v = builder();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, slidingExpiration));
                return v;
            });
        }

        private TRt AddOrUpdate<TRt>(string key, SetCacheEntry addSetCacheEntry, UpdateSetCacheEntry updateSetCacheEntry)
        {
            do
            {
                if (_externalCache.Retriable(externalCache => externalCache.Contains(key))
                    && !_externalCache.Retriable(externalCache => externalCache.CompareValue(key, Reserved)))
                    return (TRt)updateSetCacheEntry();
                if (_externalCache.Retriable(externalCache => externalCache.Add(key, Reserved)))
                    return (TRt)addSetCacheEntry();
            } while (true);
        }

        public object AddOrUpdate(string key, ValueFactory<object> addValueFactory, UpdateValueFactory<object> updateValueFactory)
        {
            return AddOrUpdate<object>(key, () =>
            {
                var v = addValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, _infiniteDateTime));
                return v;
            }, () =>
            {
                var v = updateValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, _infiniteDateTime));
                return v;
            });
        }

        public string AddOrUpdate(string key, ValueFactory<string> addValueFactory, UpdateValueFactory<string> updateValueFactory)
        {
            return AddOrUpdate<string>(key, () =>
            {
                var v = addValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, _infiniteDateTime));
                return v;
            }, () =>
            {
                var v = updateValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, _infiniteDateTime));
                return v;
            });
        }

        public object AddOrUpdate(string key, ValueFactory<object> addValueFactory, UpdateValueFactory<object> updateValueFactory, DateTime absoluteExpiration)
        {
            return AddOrUpdate<object>(key, () =>
            {
                var v = addValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, absoluteExpiration));
                return v;
            }, () =>
            {
                var v = updateValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, absoluteExpiration));
                return v;
            });
        }

        public string AddOrUpdate(string key, ValueFactory<string> addValueFactory, UpdateValueFactory<string> updateValueFactory, DateTime absoluteExpiration)
        {
            return AddOrUpdate<string>(key, () =>
            {
                var v = addValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, absoluteExpiration));
                return v;
            }, () =>
            {
                var v = updateValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, absoluteExpiration));
                return v;
            });
        }

        public object AddOrUpdate(string key, ValueFactory<object> addValueFactory, UpdateValueFactory<object> updateValueFactory, TimeSpan slidingExpiration)
        {
            return AddOrUpdate<object>(key, () =>
            {
                var v = addValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, slidingExpiration));
                return v;
            }, () =>
            {
                var v = updateValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, slidingExpiration));
                return v;
            });
        }

        public string AddOrUpdate(string key, ValueFactory<string> addValueFactory, UpdateValueFactory<string> updateValueFactory, TimeSpan slidingExpiration)
        {
            return AddOrUpdate<string>(key, () =>
            {
                var v = addValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, slidingExpiration));
                return v;
            }, () =>
            {
                var v = updateValueFactory();
                _externalCache.Retriable(externalCache => externalCache.Set(key, v, slidingExpiration));
                return v;
            });
        }
    }
}
