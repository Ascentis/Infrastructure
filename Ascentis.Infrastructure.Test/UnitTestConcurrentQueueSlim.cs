using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    public class UnitTestConcurrentQueueSlim
    {
        private readonly BoundedParallel _parallel;
        public UnitTestConcurrentQueueSlim()
        {
            _parallel = new BoundedParallel(2, 2);
            _parallel.For(0, 1000, i => { }); // warm-up
        }

        [TestMethod]
        public void TestBasic()
        {
            var queue = new ConcurrentQueueSlim<int>();
            queue.Add(10);
            queue.Add(12);
            queue.Add(14);
            queue.Add(16);
            Assert.AreEqual(10, queue.Take());
            Assert.AreEqual(12, queue.Take());
            Assert.AreEqual(14, queue.Take());
            Assert.AreEqual(16, queue.Take());
            Assert.IsTrue(queue.Empty);
        }

        [TestMethod]
        public void TestIterate()
        {
            var queue = new ConcurrentQueueSlim<int>();
            queue.Add(10);
            queue.Add(12);
            queue.Add(14);
            var sum = queue.Sum();
            Assert.AreEqual(36, sum);
        }


        [TestMethod]
        public void TestThreadedConcurrentQueueSlim()
        {
            const int loopCount = 50000;

            var done = false;
            var sum = 0;
            var bag = new ConcurrentQueueSlim<int>();
            var threadInserter = new Thread(_ =>
            {
                for (var i = 1; i <= loopCount; i++)
                    bag.Enqueue(i);
                done = true;
            });
            threadInserter.Start();
            
            _parallel.For(0, loopCount, i =>
            {
                while (true)
                {
                    if (done && bag.Empty)
                        break;
                    if (!bag.TryDequeue(out var n))
                        continue;
                    Interlocked.Add(ref sum, n);
                }
            });
            threadInserter.Join();
            Assert.AreEqual(1250025000, sum);
        }
    }
}
