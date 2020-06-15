using System;
using System.Threading;

namespace UnitTestAsyncDisposer
{
    public class TestObject : IDisposable
    {
        public static volatile int DisposedCount;
        public volatile bool Disposed;
        public string Name;

        public TestObject () {}

        public TestObject(string name)
        {
            Name = name;
        }

        public void Dispose()
        {
            Disposed = true;
            Interlocked.Increment(ref DisposedCount);
        }
    }
}
