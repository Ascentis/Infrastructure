using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestComPlusCache
    {
        [TestCleanup]
        public void TestCleanup()
        {
            var externalCacheManager = new ComPlusCacheManager();
            externalCacheManager.ClearAllCaches();
        }

        [TestMethod]
        public void TestCreate()
        {
            var comPlusCache = new ComPlusCache();
            Assert.IsNotNull(comPlusCache);
        }

        [TestMethod]
        public void TestCreateNamed()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            Assert.IsNotNull(comPlusCache);
        }

        [TestMethod]
        public void TestSetAndGetViaDictionaryAccess()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            comPlusCache["Entry 1"] = "Hello";
            Assert.AreEqual("Hello", comPlusCache["Entry 1"]);
        }

        [TestMethod]
        public void TestIterate()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            comPlusCache["Entry 1"] = "Hello";
            foreach (var entry in comPlusCache)
            {
                Assert.AreEqual("Hello", entry.Value);
                Assert.AreEqual("Entry 1", entry.Key);
            }
        }

        [TestMethod]
        public void TestContains()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            comPlusCache["Entry 1"] = "Hello";
            Assert.IsTrue(comPlusCache.Contains("Entry 1"));
        }

        [TestMethod]
        public void TestAdd()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            Assert.IsTrue(comPlusCache.Add("Entry 1", "Hello"));
            Assert.IsFalse(comPlusCache.Add("Entry 1", "Hello"));
            Assert.IsTrue(comPlusCache.Add("Entry 2", new object()));
            Assert.IsTrue(comPlusCache.Add("Entry 3", "Hello", new DateTime(9999, 1, 1)));
            Assert.IsTrue(comPlusCache.Add("Entry 4", new object(), new DateTime(9999, 1, 1)));
            Assert.IsTrue(comPlusCache.Add("Entry 5", "Hello", new TimeSpan(1, 1, 1, 1)));
            Assert.IsTrue(comPlusCache.Add("Entry 6", new object(), new TimeSpan(1, 1, 1, 1)));
            Assert.IsTrue(comPlusCache.Contains("Entry 1"));
            Assert.IsTrue(comPlusCache.Contains("Entry 2"));
            Assert.IsTrue(comPlusCache.Contains("Entry 3"));
            Assert.IsTrue(comPlusCache.Contains("Entry 4"));
            Assert.IsTrue(comPlusCache.Contains("Entry 5"));
            Assert.IsTrue(comPlusCache.Contains("Entry 6"));
        }

        [TestMethod]
        public void TestAddOrGetExisting()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            comPlusCache.Clear();
            Assert.AreEqual(null, comPlusCache.AddOrGetExisting("Entry 1", "Hello"));
            Assert.AreEqual("Hello", comPlusCache["Entry 1"]);
            var obj = new object();
            Assert.AreEqual(null, comPlusCache.AddOrGetExisting("Entry 2", obj));
            Assert.IsNotNull(comPlusCache["Entry 2"]);
            Assert.IsTrue(comPlusCache["Entry 2"] is DynamicObject);
            Assert.AreEqual("Hi", comPlusCache.GetOrAdd("Entry 3", () => "Hi"));
            Assert.AreEqual("Hi", comPlusCache.GetOrAdd("Entry 3", () => "Hello"));

            Assert.AreEqual(123, comPlusCache.GetOrAdd("Entry 4", () => 123));
            Assert.AreEqual(123, comPlusCache.GetOrAdd("Entry 4", () => 345));
        }

        [TestMethod]
        public void TestAddOrUpdate()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            comPlusCache.Clear();
            Assert.AreEqual("World", comPlusCache.AddOrUpdate("Entry 1", () => "World", () => "Hello"));
            Assert.AreEqual("World", comPlusCache["Entry 1"]);
            Assert.AreEqual("Brave New World",
                comPlusCache.AddOrUpdate("Entry 1", () => "Hello World", () => "Brave New World"));
            Assert.AreEqual("Brave New World 2",
                comPlusCache.AddOrUpdate("Entry 1", () => "Hello World", () => "Brave New World 2"));
            Assert.AreEqual("Brave New World 3",
                comPlusCache.AddOrUpdate("Entry 1", () => "Hello World", () => "Brave New World 3"));
            comPlusCache.AddOrUpdate("Entry 2", () => new Dynamo(), () => null);
            Assert.IsNotNull(comPlusCache["Entry 2"]);
            Assert.IsTrue(comPlusCache["Entry 2"] is DynamicObject);
        }

        [TestMethod]
        public void TestRemove()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            comPlusCache.Add("Entry 1", "Hello");
            Assert.IsTrue(comPlusCache.Contains("Entry 1"));
            comPlusCache.Remove("Entry 1");
            Assert.IsFalse(comPlusCache.Contains("Entry 1"));
        }

        [TestMethod]
        public void TestSetAndGet()
        {
            var comPlusCache = new ComPlusCache("TestCache");
            comPlusCache.Set("Entry 1", "Hello", new DateTime(9999, 1, 1));
            Assert.AreEqual("Hello", comPlusCache.Get("Entry 1"));
            comPlusCache.Set("Entry 2", "Hello 2", new TimeSpan(365, 0, 0, 0));
            Assert.AreEqual("Hello 2", comPlusCache.Get("Entry 2"));
            comPlusCache.Set("Entry 3", new object(), new DateTime(9999, 1, 1));
            Assert.IsTrue(comPlusCache.Contains("Entry 3"));
            comPlusCache.Set("Entry 4", new object(), new TimeSpan(365, 0, 0, 0));
            Assert.IsTrue(comPlusCache.Contains("Entry 4"));
        }

        [TestMethod]
        public void TestStressConcurrent()
        {
            const int threadCount = 4;
            const int loops = 1000;
            var totalLoops = 0;
            var threads = new Thread[threadCount];
            for (var i = 0; i < threadCount; i++)
                (threads[i] = new Thread(context =>
                {
                    var externalCache = new ComPlusCache();
                    for (var j = 0; j < loops; j++)
                    {
                        var item = new Dynamo();
                        var item2 = new Dynamo();
                        item["P1"] = "Property " + j;
                        externalCache.AddOrUpdate($"Item {(int) context}-{j}", () => item, () => item2);
                        Assert.IsTrue(externalCache.Contains($"Item {(int) context}-{j}"));
                        var returnedItem = (Dynamo) externalCache.Get($"Item {(int) context}-{j}");
                        Assert.AreEqual("Property " + j, returnedItem["P1"]);
                        externalCache.Remove($"Item {(int) context}-{j}");
                        Assert.IsFalse(externalCache.Contains($"Item {(int) context}-{j}"));
                        externalCache.AddOrUpdate($"Item {j}", () => item, () => item2);
                        Assert.IsTrue(externalCache.Contains($"Item {j}"));
                        var item3 = externalCache.GetOrAdd($"Item {j}", () => item2);
                        Assert.IsNotNull(item3);
                        Interlocked.Increment(ref totalLoops);
                    }

                })).Start(i);
            foreach (var thread in threads)
                thread.Join();
            Assert.AreEqual(threadCount * loops, totalLoops);
        }

        [TestMethod]
        public void TestAddOrUpdateParallelWithSameComPlusReference()
        {
            var cd = new ComPlusCache("Concurrent");
            //var dict = new ConcurrentDictionary<string, int>();
            cd.Clear();
            var addCnt = 0;
            var updCnt = 0;
            var totalCnt = 0;
            Parallel.For(0, 10000, i =>
            {
                Interlocked.Increment(ref totalCnt);
                cd.AddOrUpdate("1", () =>
                {
                    Interlocked.Increment(ref addCnt);
                    return 1;
                }, () =>
                {
                    Interlocked.Increment(ref updCnt);
                    return new Random().Next(10000);
                });
            });
            var cnt = 0;
            // ReSharper disable once UnusedVariable
            foreach (var item in cd)
                cnt++;
            Assert.AreEqual(1, cnt);
            Assert.AreEqual(10000, totalCnt);
            Assert.AreEqual(1, addCnt);
            Assert.AreEqual(9999, updCnt);
            Assert.AreNotEqual(-1, (int)cd["1"]);
            
            int value = (int)cd.GetOrAdd("2", () => 100);
            Assert.AreEqual(100, value);

            // Should return 100, as key 2 is already set to that value
            value = (int)cd.GetOrAdd("2", () => 10000);
            Assert.AreEqual(100, value);
        }
    }
}
