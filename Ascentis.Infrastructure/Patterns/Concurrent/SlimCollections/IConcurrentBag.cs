// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentBag<T>
    {
        void Add(T value);
        void AddRange(T[] items, int startIndex, int count);
        void AddRange(T[] items);
        bool TryTake(out T retVal);
        bool TryPeek(out T retVal);
        T Take();
        void Clear();
        bool IsEmpty { get; }
        int Count { get; }
        int Length { get; }
    }
}
