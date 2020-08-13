// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class WritableTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public WritableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
