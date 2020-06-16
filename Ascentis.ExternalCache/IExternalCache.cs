using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("87CCFC0F-73BE-4AB1-A23C-F3EA5B419D82")]
    public interface IExternalCache : IDisposable, IEnumerable<KeyValuePair<string, object>>
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
        bool CompareValue(string key, object value);
        void SelfTest();
    }
}