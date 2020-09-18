// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentQueue<T> : IConcurrentLinkedNodeCollectionBase<T>
    {
        void Enqueue(T item);
        void EnqueueRange(T[] items);
        void EnqueueRange(T[] items, int startIndex, int count);
        bool TryDequeue(out T result);
        T Dequeue();
    }
}
