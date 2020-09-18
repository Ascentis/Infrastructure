using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class BaseLinkedNode<T, TNext> : BaseLinkedNode<T> where TNext : class
    {
        internal volatile TNext Next;
    }

    public abstract class BaseLinkedNode<T>
    {
        internal virtual T Value { get; set; }

        public static void Spin(ref SpinWait? spinner)
        {
            spinner ??= new SpinWait();
            // ReSharper disable once ConstantConditionalAccessQualifier
            spinner?.SpinOnce();
        }
    }
}
