using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestStressExternalCache
    {
        [TestMethod]
        public void TestStressConcurrentAddGetAndRemove()
        {
            const int threadCount = 20;
            const int loops = 1000;
            var totalLoops = 0;
            var threads = new Thread[threadCount];
            for (var i = 0; i < threadCount; i++)
                (threads[i] = new Thread(context =>
                {
                    Assert.AreEqual(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
                    using (var externalCache = new ExternalCache())
                    {
                        for (var j = 0; j < loops; j++)
                        {
                            var item = new Dynamo();
                            item["P1"] = "Property " + j;
                            externalCache.Add($"Item {(int) context}-{j}", item);

                            Assert.IsTrue(externalCache.Contains($"Item {(int) context}-{j}"));
                            var returnedItem = (Dynamo) externalCache.Get($"Item {(int) context}-{j}");
                            Assert.AreEqual("Property " + j, returnedItem["P1"]);
                            externalCache.Remove($"Item {(int) context}-{j}");
                            Assert.IsFalse(externalCache.Contains($"Item {(int) context}-{j}"));
                            Interlocked.Increment(ref totalLoops);
                        }
                    }
                })).Start(i);
            foreach (var thread in threads)
                thread.Join();
            Assert.AreEqual(threadCount * loops, totalLoops);
        }
    }
}
