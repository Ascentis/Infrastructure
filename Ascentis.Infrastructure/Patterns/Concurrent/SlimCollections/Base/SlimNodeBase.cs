// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class SlimNodeBase<T, TNext> : SlimNodeBase<T> where TNext : class
    {
        internal volatile TNext Next;

        internal SlimNodeBase(T value) : base(value) {}
    }

    public abstract class SlimNodeBase<T>
    {
        internal T Value;

        internal SlimNodeBase(T value)
        {
            Value = value;
        }

        internal abstract SlimNodeBase<T> GetNext();
    }
}
