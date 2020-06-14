using System.Collections.Specialized;
using System.Runtime.Caching;

namespace Ascentis.Infrastructure
{
    public class InternalMemoryCache : MemoryCache
    {
        public InternalMemoryCache() : base("_default"){}
        public InternalMemoryCache(string name) : base(name) {}
        public InternalMemoryCache(string name, NameValueCollection config = null) : base (name, config) {}
    }
}
