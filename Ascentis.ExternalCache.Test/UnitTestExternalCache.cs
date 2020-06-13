using System;
using System.Dynamic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utf8Json;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestExternalCache
    {
        [TestMethod]
        public void TestCreateExternalCache()
        {
            using (var externalCache = new ExternalCache())
                Assert.IsNotNull(externalCache);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (var externalCacheManager = new ExternalCacheManager())
                externalCacheManager.ClearAllCaches();
            Thread.Sleep(1000);
        }

        [TestMethod]
        public void TestAddObjectAndGet()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                var _item = new Dynamo();
                _item["P1"] = "Property 1";
                _item["P2"] = "Property 2";
                externalCache.Add("Item 1", _item);
                var item = (Dynamo) externalCache.Get("Item 1");
                Assert.AreEqual("Property 1", item["P1"]);
                Assert.AreEqual("Property 2", item["P2"]);
                item["P1"] = "Property 3";
                item["P2"] = "Property 4";
                Assert.IsNotNull(externalCache.Remove("Item 1"));
                Assert.IsTrue(externalCache.Add("Item 1", item));
                item = (Dynamo) externalCache.Get("Item 1");
                Assert.AreEqual("Property 3", item["P1"]);
                Assert.AreEqual("Property 4", item["P2"]);
            }
        }

        [TestMethod]
        public void TestAdd10000ObjectsAndGet()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                for (var i = 0; i < 10000; i++)
                {
                    var _item = new Dynamo();
                    _item["P1"] = "Property 1";
                    _item["P2"] = i;
                    externalCache.Add($"Item {i}", _item);
                    var item = (Dynamo) externalCache.Get($"Item {i}");
                    Assert.AreEqual("Property 1", item["P1"]);
                    Assert.AreEqual(i, item["P2"]);
                }
            }
        }

        [TestMethod]
        public void TestAdd10000StringsAndGet()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                for (var i = 0; i < 10000; i++)
                {
                    externalCache.Add($"Item {i}", $"Hello {i}");
                    var item = (string)externalCache.Get($"Item {i}");
                    Assert.AreEqual($"Hello {i}", item);
                }
            }
        }

        [TestMethod]
        public void TestAddArrayWith100000StringsAsJsonAndGet()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                var arr = new dynamic[100000];
                for (var i = 0; i < 100000; i++)
                {
                    arr[i] = new ExpandoObject();
                    arr[i].P1 = i;
                    arr[i].P2 = "Hello World";
                }
                var json = JsonSerializer.Serialize(arr);
                externalCache.Add($"Item", json);
                var item = (byte[])externalCache.Get($"Item");
                // ReSharper disable once CoVariantArrayConversion
                var objs = (dynamic[])JsonSerializer.Deserialize<ExpandoObject[]>(item);
                for (var i = 0; i < 100000; i++)
                {
                    Assert.AreEqual(objs[i].P1, i);
                    Assert.AreEqual(objs[i].P2, "Hello World");
                }
            }
        }

        private void CheckItem1(ExternalCache externalCache)
        {
            var item = (Dynamo) externalCache.Get("Item 1");
            Assert.IsNotNull(item);
            Thread.Sleep(2000);
            item = (Dynamo) externalCache.Get("Item 1");
            Assert.IsNull(item);
        }

        [TestMethod]
        public void TestAddObjectAndLetItExpireByTicks()
        {
            using (var externalCache = new ExternalCache())
            {
                var item = new Dynamo();
                item["P1"] = "Property 1";
                externalCache.Add("Item 1", item, new TimeSpan(10000000)); // 10000 ticks = 1ms 
                CheckItem1(externalCache);
            }
        }

        [TestMethod]
        public void TestAddObjectAndLetItExpireByAbsoluteTime()
        {
            using (var externalCache = new ExternalCache())
            {
                var item = new Dynamo();
                item["P1"] = "Property 1";
                externalCache.Add("Item 1", item, DateTime.Now.AddMilliseconds(1000));
                CheckItem1(externalCache);
            }
        }

        [TestMethod]
        public void TestSelect()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                var _item = new Dynamo();
                _item["P1"] = "Property 1";
                externalCache.Add("Item 1", _item);
                var item = (Dynamo) externalCache.Get("Item 1");
                Assert.IsNotNull(item);
                externalCache.Select("Second Cache");
                item = (Dynamo) externalCache.Get("Item 1");
                Assert.IsNull(item);
                // ReSharper disable once InconsistentNaming
                var _item2 = new Dynamo();
                _item2["P1"] = "Property 1";
                externalCache.Add("Item 2", _item2);

                item = (Dynamo) externalCache.Get("Item 2");
                Assert.IsNotNull(item);
                externalCache.Select("default");
                item = (Dynamo) externalCache.Get("Item 1");
                Assert.IsNotNull(item);
                item = (Dynamo) externalCache.Get("Item 2");
                Assert.IsNull(item);
            }
        }

        [TestMethod]
        public void TestClear()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                var _item = new Dynamo();
                _item["P1"] = "Property 1";
                externalCache.Add("Item 1", _item);

                var item = (Dynamo) externalCache.Get("Item 1");
                Assert.AreEqual("Property 1", item["P1"]);
                externalCache.Clear();
                item = (Dynamo) externalCache.Get("Item 1");
                Assert.IsNull(item);
            }
        }

        [TestMethod]
        public void TestClearAllCaches()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                var _item = new Dynamo();
                _item["P1"] = "Property 1";
                externalCache.Add("Item 1", _item);

                var item = (Dynamo) externalCache.Get("Item 1");
                Assert.AreEqual("Property 1", item["P1"]);
                externalCache.Select("Cache 2");
                externalCache.Add("Item 2", item);
                item = (Dynamo) externalCache.Get("Item 2");
                Assert.AreEqual("Property 1", item["P1"]);
                using (var externalCacheManager = new ExternalCacheManager())
                    externalCacheManager.ClearAllCaches();
                Thread.Sleep(1000);
                item = (Dynamo) externalCache.Get("Item 1");
                Assert.IsNull(item);
                item = (Dynamo) externalCache.Get("Item 2");
                Assert.IsNull(item);
                externalCache.Select("default");
                item = (Dynamo) externalCache.Get("Item 1");
                Assert.IsNull(item);
                item = (Dynamo) externalCache.Get("Item 2");
                Assert.IsNull(item);
            }
        }

        [TestMethod]
        public void TestAddOrGetExisting()
        {
            using (var externalCache = new ExternalCache())
            {
                // ReSharper disable once InconsistentNaming
                var _item = new Dynamo();
                    _item["P1"] = "Item 1";
                    externalCache.Add("Item 1", _item);
                Dynamo item = new Dynamo();
                item["P1"] = "Item 2";
                item = (Dynamo) externalCache.AddOrGetExisting("Item 1", item);
                Assert.AreEqual("Item 1", item["P1"]);
            }
        }

        [TestMethod]
        public void TestTryAddSameTwice()
        {
            using (var externalCache = new ExternalCache())
            {
                Assert.IsTrue(externalCache.Add("Item 1", "Value 1"));
                var item = (string)externalCache.Get("Item 1");
                Assert.AreEqual("Value 1", item);
                Assert.IsFalse(externalCache.Add("Item 1", "Value 2"));
                item = (string)externalCache.Get("Item 1");
                Assert.AreEqual("Value 1", item);
            }
        }

        [TestMethod]
        public void TestContains()
        {
            using (var externalCache = new ExternalCache())
            {
                Assert.IsFalse(externalCache.Contains("Item 1"));
                var item = new Dynamo();
                item["P1"] = "Property 1";
                externalCache.Add("Item 1", item, new TimeSpan(10000000)); // 10000 ticks = 1ms 

                Assert.IsTrue(externalCache.Contains("Item 1"));
            }
        }

        [TestMethod]
        public void TestRemove()
        {
            using (var externalCache = new ExternalCache())
            {
                var item = new Dynamo();
                item["P1"] = "Property 1";
                externalCache.Add("Item 1", item);

                Assert.IsTrue(externalCache.Contains("Item 1"));
                externalCache.Remove("Item 1");
                Assert.IsFalse(externalCache.Contains("Item 1"));
            }
        }

        [TestMethod]
        public void TestSetObjectAndLetItExpireByTicks()
        {
            using (var externalCache = new ExternalCache())
            {
                var item = new Dynamo();
                item["P1"] = "Property 1";
                externalCache.Set("Item 1", item, new TimeSpan(10000000)); // 10000 ticks = 1ms 

                CheckItem1(externalCache);
            }
        }

        [TestMethod]
        public void TestSetObjectAndLetItExpireByAbsoluteTime()
        {
            using (var externalCache = new ExternalCache())
            {
                var item = new Dynamo();
                item["P1"] = "Property 1";
                externalCache.Set("Item 1", item, DateTime.Now.AddMilliseconds(1000));

                CheckItem1(externalCache);
            }
        }

        [TestMethod]
        public void TestAddStringAndGet()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Add("Item 1", "Value 1");
                var item = (string) externalCache.Get("Item 1");
                Assert.AreEqual("Value 1", item);
            }
        }

        private void CheckItem1AsString(ExternalCache externalCache)
        {
            var item = (string)externalCache.Get("Item 1");
            Assert.IsNotNull(item);
            Thread.Sleep(2000);
            item = (string)externalCache.Get("Item 1");
            Assert.IsNull(item);
        }

        [TestMethod]
        public void TestAddStringAndLetItExpireByTicks()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Add("Item 1", "Value 1", new TimeSpan(10000000)); // 10000 ticks = 1ms 
                CheckItem1AsString(externalCache);
            }
        }

        [TestMethod]
        public void TestAddStringAndLetItExpireByAbsoluteTime()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Add("Item 1", "Value 1", DateTime.Now.AddMilliseconds(1000));
                CheckItem1AsString(externalCache);
            }
        }

        [TestMethod]
        public void TestAddOrGetExistingString()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Add("Item 1", "Value 1");
                var item = (string) externalCache.AddOrGetExisting("Item 1", "Value 2");
                Assert.AreEqual("Value 1", item);
            }
        }

        [TestMethod]
        public void TestSetStringAndLetItExpireByTicks()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Set("Item 1", "Value 1", new TimeSpan(10000000)); // 10000 ticks = 1ms 
                CheckItem1AsString(externalCache);
            }
        }

        [TestMethod]
        public void TestSetStringTwice()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Set("Item 1", "Value 1", new TimeSpan(10000000)); // 10000 ticks = 1ms 
                var item = (string)externalCache.Get("Item 1");
                Assert.AreEqual("Value 1", item);
                externalCache.Set("Item 1", "Value 2", new TimeSpan(10000000)); // 10000 ticks = 1ms 
                item = (string)externalCache.Get("Item 1");
                Assert.AreEqual("Value 2", item);
            }
        }

        [TestMethod]
        public void TestSetStringAndLetItExpireByAbsoluteTime()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Set("Item 1", "Value 1", DateTime.Now.AddMilliseconds(1000));
                CheckItem1AsString(externalCache);
            }
        }

        [TestMethod]
        public void TestAddDecimalAndGet()
        {
            using (var externalCache = new ExternalCache())
            {
                externalCache.Add("Item 1", (decimal) 10.5);
                var item = (decimal)externalCache.Get("Item 1");
                Assert.AreEqual((decimal)10.5, item);
            }
        }
    }
}
