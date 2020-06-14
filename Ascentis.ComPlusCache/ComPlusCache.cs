using System;
using System.Collections;
using System.Collections.Generic;

/* System.Runtime.InteropServices.COMException: 'The remote procedure call failed. (Exception from HRESULT: 0x800706BE)' */

namespace Ascentis.Infrastructure
{
    public class ComPlusCache : IDisposable, IEnumerable<KeyValuePair<string, object>>
    {
        private readonly DateTime _infiniteDateTime = new DateTime(9999, 1, 1);
        private readonly Retrier<ExternalCache> _externalCache = new Retrier<ExternalCache>(new ExternalCache());

        public ComPlusCache() {}

        public ComPlusCache(string name)
        {
            _externalCache.Retriable( externalCache => externalCache.Select(name));
        }

        public void Dispose()
        {
            _externalCache.Retriable( externalCache => externalCache.Dispose());
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return (_externalCache as IEnumerable<KeyValuePair<string, object>>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_externalCache as IEnumerable).GetEnumerator();
        }

        public long Trim(int percent)
        {
            return _externalCache.Retriable(externalCache => externalCache.Trim(percent));
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
    }
}
