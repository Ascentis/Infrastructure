using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestExternalCache
    {
        [TestMethod]
        public void TestCreateExternalCache()
        {
            var externalCache = new ExternalCache();
            Assert.IsNotNull(externalCache);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var externalCacheManager = new ExternalCacheManager();
            externalCacheManager.ClearAllCaches();
            Thread.Sleep(1000);
        }

        [TestMethod]
        public void TestAddObjectAndGet()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            item["P2"] = "Property 2";
            externalCache.Add("Item 1", item);
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.AreEqual("Property 1", item["P1"]);
            Assert.AreEqual("Property 2", item["P2"]);
            item["P1"] = "Property 3";
            item["P2"] = "Property 4";
            /* Contrary to documentation, method Add() will operate the same as
               AddOrGetExisting() yet return False if the item already exists */
            Assert.IsFalse(externalCache.Add("Item 1", item));
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.AreEqual("Property 3", item["P1"]);
            Assert.AreEqual("Property 4", item["P2"]);
        }

        private void CheckItem1(ExternalCache externalCache)
        {
            var item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNotNull(item);
            Thread.Sleep(2000);
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNull(item);
        }

        [TestMethod]
        public void TestAddObjectAndLetItExpireByTicks()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 1", item, new TimeSpan(10000000)); // 10000 ticks = 1ms 
            CheckItem1(externalCache);
        }

        [TestMethod]
        public void TestAddObjectAndLetItExpireByAbsoluteTime()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 1", item, new DateTimeOffset(DateTime.Now.AddMilliseconds(1000)));
            CheckItem1(externalCache);
        }

        [TestMethod]
        public void TestSelect()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 1", item);
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNotNull(item);
            externalCache.Select("Second Cache");
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNull(item);
            item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 2", item);
            item = (ExternalCacheItem)externalCache.Get("Item 2");
            Assert.IsNotNull(item);
            externalCache.Select("default");
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNotNull(item);
            item = (ExternalCacheItem)externalCache.Get("Item 2");
            Assert.IsNull(item);
        }

        [TestMethod]
        public void TestClear()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 1", item);
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.AreEqual("Property 1", item["P1"]);
            externalCache.Clear();
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNull(item);
        }

        [TestMethod]
        public void TestClearAllCaches()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 1", item);
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.AreEqual("Property 1", item["P1"]);
            externalCache.Select("Cache 2");
            externalCache.Add("Item 2", item);
            item = (ExternalCacheItem)externalCache.Get("Item 2");
            Assert.AreEqual("Property 1", item["P1"]);
            var externalCacheManager = new ExternalCacheManager();
            externalCacheManager.ClearAllCaches();
            Thread.Sleep(1000);
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNull(item);
            item = (ExternalCacheItem)externalCache.Get("Item 2");
            Assert.IsNull(item);
            externalCache.Select("default");
            item = (ExternalCacheItem)externalCache.Get("Item 1");
            Assert.IsNull(item);
            item = (ExternalCacheItem)externalCache.Get("Item 2");
            Assert.IsNull(item);
        }

        [TestMethod]
        public void TestAddOrGetExisting()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Item 1";
            externalCache.Add("Item 1", item);
            //item = new ExternalCacheItem();
            item["P1"] = "Item 2";
            item = (ExternalCacheItem)externalCache.AddOrGetExisting("Item 1", item);
            Assert.AreEqual("Item 1", item["P1"]);
        }

        [TestMethod]
        public void TestContains()
        {
            var externalCache = new ExternalCache();
            Assert.IsFalse(externalCache.Contains("Item 1"));
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 1", item, new TimeSpan(10000000)); // 10000 ticks = 1ms 
            Assert.IsTrue(externalCache.Contains("Item 1"));
        }

        [TestMethod]
        public void TestRemove()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Add("Item 1", item, new TimeSpan(10000000)); // 10000 ticks = 1ms 
            Assert.IsTrue(externalCache.Contains("Item 1"));
            externalCache.Remove("Item 1");
            Assert.IsFalse(externalCache.Contains("Item 1"));
        }

        [TestMethod]
        public void TestSetObjectAndLetItExpireByTicks()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Set("Item 1", item, new TimeSpan(10000000)); // 10000 ticks = 1ms 
            CheckItem1(externalCache);
        }

        [TestMethod]
        public void TestSetObjectAndLetItExpireByAbsoluteTime()
        {
            var externalCache = new ExternalCache();
            var item = new ExternalCacheItem();
            item["P1"] = "Property 1";
            externalCache.Set("Item 1", item, new DateTimeOffset(DateTime.Now.AddMilliseconds(1000)));
            CheckItem1(externalCache);
        }
    }
}
