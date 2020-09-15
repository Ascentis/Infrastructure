// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentStack<T>
    {
        void Push(T item);
        void PushRange(T[] items);
        void PushRange(T[] items, int startIndex, int count);
        bool TryPop(out T result);
        bool TryPeek(out T result);
        T Pop();
        void Clear();
        bool Empty { get; }
    }
}
