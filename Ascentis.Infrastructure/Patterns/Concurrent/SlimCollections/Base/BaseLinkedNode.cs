using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public abstract class BaseLinkedNode<T, TNext> where TNext : class
    {
        internal virtual T Value { get; set; }
        internal volatile TNext Next;
    }
}
