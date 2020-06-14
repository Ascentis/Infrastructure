using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("d0eec9c2-8cc1-40ad-96fd-22908fe467a5")]
    public interface IExternalCache : IEnumerable<KeyValuePair<string, object>>
    {
        void Select(string cacheName);
        bool Add(string key, object item);
        bool Add(string key, string item);
        bool Add(string key, object item, DateTime absoluteExpiration);
        bool Add(string key, string item, DateTime absoluteExpiration);
        bool Add(string key, object item, TimeSpan slidingExpiration);
        bool Add(string key, string item, TimeSpan slidingExpiration);
        object AddOrGetExisting(string key, object value);
        object AddOrGetExisting(string key, string value);
        bool Contains(string key);
        object Get(string key);
        object Remove(string key);
        void Set(string key, object value, DateTime absoluteExpiration);
        void Set(string key, object value, TimeSpan slidingExpiration);
        void Set(string key, string value, DateTime absoluteExpiration);
        void Set(string key, string value, TimeSpan slidingExpiration);
        void Clear();
    }
}