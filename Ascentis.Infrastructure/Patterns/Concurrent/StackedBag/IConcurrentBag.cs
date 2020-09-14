// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentBag<T>
    {
        void Add(T value);
        bool TryTake(out T retVal);
        bool TryPeek(out T retVal);
        T Take();
        void Clear();
        bool Empty { get; }
    }
}
