using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class QueuedBagSlimNode<T> : SlimNodeBase<T, QueuedBagSlimNode<T>>
    {
        internal volatile bool Ground;

        internal QueuedBagSlimNode(T value) : base(value)
        {
            Ground = true;
        }

        internal void EnsureUngrounded()
        {
            if (!Ground)
                return;
            SpinWait? spinner = null;
            while (Ground)
            {
                spinner ??= new SpinWait();
                // ReSharper disable once ConstantConditionalAccessQualifier
                spinner?.SpinOnce();
            }
        }

        internal override SlimNodeBase<T> GetNext()
        {
            if (Next?.Next != null)
                Next.EnsureUngrounded();
            return (!Next?.Ground ?? false) ? Next : null;
        }
    }
}
