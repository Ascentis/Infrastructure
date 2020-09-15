using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    public class UnitTestConcurrentStackedBagSlim
    {
        private readonly BoundedParallel _parallel;
        public UnitTestConcurrentStackedBagSlim()
        {
            _parallel = new BoundedParallel(2, 4);
            _parallel.For(0, 1000, i => { }); // warm-up
        }

        [TestMethod]
        public void TestBasic()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            bag.Add(10);
            Assert.IsTrue(bag.TryPeek(out var n));
            Assert.AreEqual(10, n);
            Assert.AreEqual(10, bag.Take());
        }

        [TestMethod]
        public void TestPushRange()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            var arr = new [] {10, 12, 14};
            bag.PushRange(arr);
            Assert.AreEqual(14, bag.Take());
            Assert.AreEqual(12, bag.Take());
            Assert.AreEqual(10, bag.Take());
            Assert.IsTrue(bag.IsEmpty);
        }

        [TestMethod]
        public void TestToArray()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            bag.Add(10);
            bag.Add(11);
            bag.Add(12);
            var arr = bag.ToArray();
            Assert.AreEqual(12, arr[0]);
            Assert.AreEqual(11, arr[1]);
            Assert.AreEqual(10, arr[2]);
        }

        [TestMethod]
        public void TestBasicIterate()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            bag.Add(10);
            bag.Add(11);
            bag.Add(13);
            var sum = bag.Sum();
            Assert.AreEqual(34, sum);
        }

        [TestMethod]
        public void TestContains()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            bag.Add(10);
            bag.Add(11);
            bag.Add(13);
            Assert.IsTrue(bag.Contains(11));
        }

        [TestMethod]
        public void TestCount()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            bag.Add(10);
            bag.Add(11);
            bag.Add(13);
            Assert.AreEqual(3, bag.Count);
        }

        [TestMethod]
        public void TestCopyTo()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            bag.Add(10);
            bag.Add(11);
            bag.Add(13);
            var arr = new int[3];
            bag.CopyTo(arr, 0);
            Assert.AreEqual(13, arr[0]);
            Assert.AreEqual(11, arr[1]);
            Assert.AreEqual(10, arr[2]);
        }

        [TestMethod]
        public void TestAddThreeCheckEmptyAndTryTakeShouldErrorOut()
        {
            var bag = new ConcurrentStackedBagSlim<int>();
            bag.Add(10);
            bag.Add(9);
            bag.Add(8);
            Assert.AreEqual(8, bag.Take());
            Assert.AreEqual(9, bag.Take());
            Assert.AreEqual(10, bag.Take());
            Assert.IsTrue(bag.IsEmpty);
            Assert.ThrowsException<InvalidOperationException>(() => bag.Take());
        }

        [TestMethod]
        public void TestThreadedConcurrentBagSlim()
        {
            const int loopCount = 50000;

            var done = false;
            var sum = 0;
            var bag = new ConcurrentStackedBagSlim<int>();
            var threadInserter = new Thread(_ =>
            {
                for (var i = 1; i <= loopCount; i++)
                    bag.Add(i);
                done = true;
            });
            threadInserter.Start();
            _parallel.For(0, loopCount, i =>
            {
                while (true)
                {
                    if (done && bag.IsEmpty)
                        break;
                    if (bag.TryTake(out var n))
                        Interlocked.Add(ref sum, n);
                }
            });
            Assert.AreEqual(1250025000, sum);
        }

        [TestMethod]
        public void TestThreadedAddAndIterate()
        {
            const int loopCount = 1000000;

            var bag = new ConcurrentStackedBagSlim<int>();
            var threadInserter = new Thread(_ =>
            {
                for (var i = 1; i <= loopCount; i++)
                    bag.Add(i);
            });
            threadInserter.Start();
            Thread.Sleep(10);
            var cnt = 0;
            foreach (var _ in bag)
                cnt++;
            threadInserter.Join();
            Assert.AreNotEqual(0, cnt);
        }

        [TestMethod]
        public void TestThreadedRegularConcurrentBag()
        {
            const int loopCount = 50000;

            var done = false;
            var sum = 0;
            var bag = new ConcurrentBag<int>();
            var threadInserter = new Thread(_ =>
            {
                for (var i = 1; i <= loopCount; i++)
                    bag.Add(i);
                done = true;
            });
            threadInserter.Start();
            _parallel.For(0, loopCount, i =>
            {
                while (true)
                {
                    if (done && bag.Count <= 0)
                        break;
                    if (!bag.TryTake(out var n))
                        continue;
                    Interlocked.Add(ref sum, n);
                }
            });
            Assert.AreEqual(1250025000, sum);
        }

        [TestMethod]
        public void TestThreadedRegularConcurrentQueue()
        {
            const int loopCount = 200000;

            var done = false;
            var sum = 0;
            var bag = new ConcurrentQueue<int>();
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
            Assert.AreEqual(1250025000, sum);
        }

        [TestMethod]
        public void TestThreadedConcurrentStack()
        {
            const int loopCount = 50000;

            var done = false;
            var sum = 0;
            var bag = new ConcurrentStack<int>();
            var threadInserter = new Thread(_ =>
            {
                for (var i = 1; i <= loopCount; i++)
                    bag.Push(i);
                done = true;
            });
            threadInserter.Start();
            _parallel.For(0, loopCount, i =>
            {
                while (true)
                {
                    if (done && bag.Count <= 0)
                        break;
                    if (!bag.TryPop(out var n))
                        continue;
                    Interlocked.Add(ref sum, n);
                }
            });
            Assert.AreEqual(1250025000, sum);
        }
    }
}
