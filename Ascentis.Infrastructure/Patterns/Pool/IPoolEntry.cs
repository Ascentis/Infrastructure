// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IPoolEntry
    {
        void Retain();
        void Release();
    }
}
