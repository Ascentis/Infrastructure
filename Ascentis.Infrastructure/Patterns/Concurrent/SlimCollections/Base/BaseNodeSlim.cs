// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class BaseNodeSlim<T, TNext> : BaseNodeSlim<T> where TNext : class
    {
        internal volatile TNext Next;

        internal BaseNodeSlim(T value) : base(value) {}
    }

    public abstract class BaseNodeSlim<T>
    {
        internal T Value;

        internal BaseNodeSlim(T value)
        {
            Value = value;
        }

        internal abstract BaseNodeSlim<T> GetNext();
    }
}
