using System.Diagnostics;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class QueuedBagNodeSlim<T> : BaseLinkedNode<T, QueuedBagNodeSlim<T>>
    {
        internal volatile bool Ground;

        public QueuedBagNodeSlim()
        {
            Ground = true;
        }

        private void EnsureUngrounded()
        {
            Debug.Assert(Next != null, "EnsureUngrounded() can't be called on the last node of the structure");
            if (!Ground)
                return;
            SpinWait? spinner = null;
            while (Ground)
                Spin(ref spinner);
        }

        internal override T Value
        {
            get
            {
                EnsureUngrounded();
                return base.Value;
            }
        }
    }
}
