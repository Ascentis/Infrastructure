using System.Runtime.InteropServices;

namespace Ascentis.Infrastructure
{
    [Guid("f81e612d-9b32-4b92-abed-df79107986e9")]
    public interface IExternalCacheItem
    {
        dynamic Container { get; }
        object this[string key] { get; set; }
        void CopyFrom(object value);
        void CopyTo(object target);
    }
}