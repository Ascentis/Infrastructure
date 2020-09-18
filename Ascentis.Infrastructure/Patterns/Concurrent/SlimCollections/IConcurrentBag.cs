// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentBag<T> : IConcurrentLinkedNodeCollectionBase<T>
    {
        void Add(T value);
        void AddRange(T[] items, int startIndex, int count);
        void AddRange(T[] items);
        bool TryTake(out T retVal);
        T Take();
        int Count { get; } // Count could result on a full scan of the collection
        int Length { get; } // Only supported when collection uses "KeepCount" semantics
    }
}
