// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class SlimNode<T> where T : class
    {
        internal T Value { get; }
        internal SlimNode<T> Next;

        internal SlimNode(T value)
        {
            Value = value;
        }
    }
}
