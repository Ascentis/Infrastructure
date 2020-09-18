using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    public class UnitTestConcurrentQueuedBagSlim
    {
        private readonly BoundedParallel _parallel;
        public UnitTestConcurrentQueuedBagSlim()
        {
            _parallel = new BoundedParallel(2, 2);
            _parallel.For(0, 1000, i =>
            {
                Thread.Yield();
            }); // thread-pool warm-up
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
            Assert.IsFalse(queue.TryTake(out var _));
            Assert.IsTrue(queue.IsEmpty);
        }

        [TestMethod]
        public void TestBasicKeepCount()
        {
            var queue = new ConcurrentQueueSlim<int>(true);
            queue.Add(10);
            queue.Add(12);
            queue.Add(14);
            queue.Add(16);
            Assert.AreEqual(4, queue.Count);
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
        public void TestPushRange()
        {
            var bag = new ConcurrentQueueSlim<int>();
            bag.Add(5);
            var arr = new[] { 10, 12, 14 };
            bag.AddRange(arr);
            Assert.AreEqual(5, bag.Take());
            Assert.AreEqual(10, bag.Take());
            Assert.AreEqual(12, bag.Take());
            Assert.AreEqual(14, bag.Take());
            Assert.IsTrue(bag.IsEmpty);
        }

        [TestMethod]
        public void TestPushRangeOneElement()
        {
            var bag = new ConcurrentQueueSlim<int>();
            bag.Add(5);
            var arr = new[] { 10 };
            bag.AddRange(arr);
            Assert.AreEqual(5, bag.Take());
            Assert.AreEqual(10, bag.Take());
            Assert.IsTrue(bag.IsEmpty);
        }

        [TestMethod]
        public void TestPushRangeInParallel()
        {
            const int itemCount = 10000;
            var bag = new ConcurrentQueueSlim<int>();
            var threadAdd = new Thread(_ =>
            {
                for (var i = 1; i <= itemCount; i++)
                {
                    bag.Add(i);
                    Thread.Yield();
                }
            });
            threadAdd.Start();
            var arr = new[] { itemCount + 1, itemCount + 2, itemCount + 3, itemCount + 4, itemCount + 5 };
            bag.AddRange(arr);
            threadAdd.Join();
            Assert.AreEqual(itemCount + arr.Length, bag.Count);
            var sum = bag.Sum();
            Assert.AreEqual(50055015, sum);
        }


        [TestMethod]
        public void TestThreadedConcurrentQueueSlim()
        {
            const int loopCount = 200000;

            var done = false;
            var sum = 0;
            var bag = new ConcurrentQueueSlim<int>();
            var threadInserter = new Thread(_ =>
            {
                var parallel = new BoundedParallel(2, 4);
                parallel.For(1, loopCount + 1, (i) =>
                {
                    bag.Enqueue(i > 50000 ? 0 : i);
                });
                done = true;
            });
            threadInserter.Start();
            _parallel.For(0, loopCount, i =>
            {
                while (true)
                {
                    if (done && bag.IsEmpty)
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
