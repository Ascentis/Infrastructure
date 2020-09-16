using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class QueuedBagNodeSlim<T> : BaseNodeSlim<T, QueuedBagNodeSlim<T>>
    {
        internal volatile bool Ground;

        internal QueuedBagNodeSlim(T value) : base(value)
        {
            Ground = false;
        }

        public QueuedBagNodeSlim() : base(default)
        {
            Ground = true;
        }

        internal void EnsureUngrounded()
        {
            if (!Ground)
                return;
            SpinWait? spinner = null;
            while (Ground)
                Spin(ref spinner);
        }

        internal T GetUngroundedValue()
        {
            EnsureUngrounded();
            return Value;
        }

        internal override BaseNodeSlim<T> GetNext()
        {
            if (Next?.Next != null)
                Next.EnsureUngrounded();
            return (!Next?.Ground ?? false) ? Next : null;
        }
    }
}
