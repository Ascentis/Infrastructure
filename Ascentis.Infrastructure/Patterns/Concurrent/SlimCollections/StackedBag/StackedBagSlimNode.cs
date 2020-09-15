// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class StackedBagSlimNode<T> : SlimNodeBase<T, StackedBagSlimNode<T>>
    {
        public StackedBagSlimNode(T value) : base(value) { }

        internal override SlimNodeBase<T> GetNext()
        {
            return Next;
        }
    }
}
