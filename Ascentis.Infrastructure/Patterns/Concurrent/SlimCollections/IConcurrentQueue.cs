// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentQueue<T>
    {
        void Enqueue(T item);
        void EnqueueRange(T[] items);
        void EnqueueRange(T[] items, int startIndex, int count);
        bool TryDequeue(out T result);
        bool TryPeek(out T result);
        T Dequeue();
        void Clear();
        bool Empty { get; }
    }
}
