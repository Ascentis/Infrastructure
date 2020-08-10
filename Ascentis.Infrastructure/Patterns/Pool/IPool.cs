// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IPool<T>
    {
        void Release(PoolEntry<T> obj);
    }
}
