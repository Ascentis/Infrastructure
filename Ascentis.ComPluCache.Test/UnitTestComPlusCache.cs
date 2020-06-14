using System;
using System.Dynamic;
using Ascentis.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestComPlusCache
    {
        [TestCleanup]
        public void TestCleanup()
        {
            using (var externalCacheManager = new ComPlusCacheManager())
                externalCacheManager.ClearAllCaches();
        }

        [TestMethod]
        public void TestCreate()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache())
            {
                Assert.IsNotNull(comPlusCache);
            }
        }

        [TestMethod]
        public void TestCreateNamed()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                Assert.IsNotNull(comPlusCache);
            }
        }

        [TestMethod]
        public void TestSetAndGetViaDictionaryAccess()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                comPlusCache["Entry 1"] = "Hello";
                Assert.AreEqual("Hello", comPlusCache["Entry 1"]);
            }
        }

        [TestMethod]
        public void TestIterate()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                comPlusCache["Entry 1"] = "Hello";
                foreach (var entry in comPlusCache)
                {
                    Assert.AreEqual("Hello", entry.Value);
                    Assert.AreEqual("Entry 1", entry.Key);
                }
            }
        }

        [TestMethod]
        public void TestTrim()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache())
            {
                Assert.AreEqual(0, comPlusCache.Trim(100));
            }
        }

        [TestMethod]
        public void TestContains()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                comPlusCache["Entry 1"] = "Hello";
                Assert.IsTrue(comPlusCache.Contains("Entry 1"));
            }
        }

        [TestMethod]
        public void TestAdd()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                comPlusCache.Add("Entry 1", "Hello");
                comPlusCache.Add("Entry 2", new object());
                comPlusCache.Add("Entry 3", "Hello", new DateTime(9999, 1, 1));
                comPlusCache.Add("Entry 4", new object(), new DateTime(9999, 1, 1));
                comPlusCache.Add("Entry 5", "Hello", new TimeSpan(1, 1, 1, 1));
                comPlusCache.Add("Entry 6", new object(), new TimeSpan(1, 1, 1, 1));
                Assert.IsTrue(comPlusCache.Contains("Entry 1"));
                Assert.IsTrue(comPlusCache.Contains("Entry 2"));
                Assert.IsTrue(comPlusCache.Contains("Entry 3"));
                Assert.IsTrue(comPlusCache.Contains("Entry 4"));
                Assert.IsTrue(comPlusCache.Contains("Entry 5"));
                Assert.IsTrue(comPlusCache.Contains("Entry 6"));
            }
        }

        [TestMethod]
        public void TestAddOrGetExisting()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                comPlusCache.Trim(100);
                Assert.AreEqual(null, comPlusCache.AddOrGetExisting("Entry 1", "Hello"));
                Assert.AreEqual("Hello", comPlusCache["Entry 1"]);
                var obj = new object();
                Assert.AreEqual(null, comPlusCache.AddOrGetExisting("Entry 2", obj));
                Assert.IsNotNull(comPlusCache["Entry 2"]);
                Assert.IsTrue(comPlusCache["Entry 2"] is DynamicObject);
            }
        }

        [TestMethod]
        public void TestRemove()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                comPlusCache.Add("Entry 1", "Hello");
                Assert.IsTrue(comPlusCache.Contains("Entry 1"));
                comPlusCache.Remove("Entry 1");
                Assert.IsFalse(comPlusCache.Contains("Entry 1"));
            }
        }

        [TestMethod]
        public void TestSetAndGet()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache("TestCache"))
            {
                comPlusCache.Set("Entry 1", "Hello", new DateTime(9999, 1, 1));
                Assert.AreEqual("Hello", comPlusCache.Get("Entry 1"));
                comPlusCache.Set("Entry 2", "Hello 2", new TimeSpan(365, 0, 0, 0));
                Assert.AreEqual("Hello 2", comPlusCache.Get("Entry 2"));
                comPlusCache.Set("Entry 3", new object(), new DateTime(9999, 1, 1));
                Assert.IsTrue(comPlusCache.Contains("Entry 3"));
                comPlusCache.Set("Entry 4", new object(), new TimeSpan(365, 0, 0, 0));
                Assert.IsTrue(comPlusCache.Contains("Entry 4"));
            }
        }
    }
}
