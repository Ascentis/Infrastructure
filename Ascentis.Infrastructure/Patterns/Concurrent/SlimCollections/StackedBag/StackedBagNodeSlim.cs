// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class StackedBagNodeSlim<T> : BaseNodeSlim<T, StackedBagNodeSlim<T>>
    {
        public StackedBagNodeSlim(T value) : base(value) { }

        internal override BaseNodeSlim<T> GetNext()
        {
            return Next;
        }
    }
}
