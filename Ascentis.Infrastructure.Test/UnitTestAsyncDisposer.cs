using System;
using System.Threading;
using Ascentis.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestAsyncDisposer
{
    [TestClass]
    public class UnitTestAsyncDisposer
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AsyncDisposer.DisposeInterval = 500;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestObject.DisposedCount = 0;
        }

        [TestMethod]
        public void TestEnqueueDisposable()
        {
            var obj = new TestObject();
            AsyncDisposer.Enqueue(obj);
            Thread.Sleep(1000);
            Assert.IsTrue(obj.Disposed);
        }

        [TestMethod]
        public void TestStress()
        {
            const int threadCount = 4;
            const int loops = 1000;
            var totalLoops = 0;
            var threads = new Thread[threadCount];
            for (var i = 0; i < threadCount; i++)
                (threads[i] = new Thread(context =>
                {
                    for (var j = 0; j < loops; j++)
                    {
                        var item = new TestObject();
                        AsyncDisposer.Enqueue(item);
                        Interlocked.Increment(ref totalLoops);
                    }
                })).Start(i);
            foreach (var thread in threads)
                thread.Join();
            Assert.AreEqual(threadCount * loops, totalLoops);
            Thread.Sleep(1000);
            Assert.AreEqual(threadCount * loops, TestObject.DisposedCount);
        }

        [TestMethod]
        public void TestCreateInstanceFails()
        {
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new AsyncDisposer();
                Assert.Fail("Creating instance of AsyncDisposer class should have thrown exception");
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
