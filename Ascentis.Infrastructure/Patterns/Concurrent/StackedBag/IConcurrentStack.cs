// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public interface IConcurrentStack<T>
    {
        void Push(T item);
        bool TryPop(out T result);
        bool TryPeek(out T result);
        T Pop();
        void Clear();
        bool Empty { get; }
    }
}
