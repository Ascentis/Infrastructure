// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentLinkedNodeCollectionBase<T>
    {
        bool TryPeek(out T retVal);
        void Clear();
        bool IsEmpty { get; }
    }
}
