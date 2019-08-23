using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestExternalCacheItem
    {
        [TestMethod]
        public void TestCreateExternalCacheItem()
        {
            var item = new ExternalCacheItem();
            Assert.IsNotNull(item);
        }

        [TestMethod]
        public void TestSetPropertiesOfContainer()
        {
            var item = new ExternalCacheItem();
            item.Container.Prop1 = 1;
            item.Container.Prop2 = "Hello";
            Assert.AreEqual(1, item.Container.Prop1);
            Assert.AreEqual("Hello", item.Container.Prop2);
        }

        [TestMethod]
        public void TestSetPropertiesOfContainerUsingArray()
        {
            var item = new ExternalCacheItem();
            item.Container["Prop1"] = 1;
            item.Container["Prop2"] = "Hello";
            Assert.AreEqual(1, item.Container["Prop1"]);
            Assert.AreEqual("Hello", item.Container["Prop2"]);
        }

        [TestMethod]
        public void TestSetPropertiesUsingArray()
        {
            var item = new ExternalCacheItem();
            item["Prop1"] = 1;
            item["Prop2"] = "Hello";
            Assert.AreEqual(1, item["Prop1"]);
            Assert.AreEqual("Hello", item["Prop2"]);
        }

        [TestMethod]
        public void TestCopyFromAndTo()
        {
            var item = new ExternalCacheItem();
            var obj = new TestClass {Prop1 = "P1", Prop2 = "P2", Prop3 = "P3", Prop4 = -1};
            item.CopyFrom(obj);
            Assert.AreEqual("P1", item.Container.Prop1);
            Assert.AreEqual("P2", item.Container.Prop2);
            Assert.AreEqual("P3", item.Container.Prop3);
            Assert.AreEqual(-1, item.Container.Prop4);
            obj = new TestClass();
            Assert.AreEqual(null, obj.Prop1);
            Assert.AreEqual(null, obj.Prop2);
            Assert.AreEqual(null, obj.Prop3);
            Assert.AreEqual(0, obj.Prop4);
            item.CopyTo(obj);
            Assert.AreEqual("P1", obj.Prop1);
            Assert.AreEqual("P2", obj.Prop2);
            Assert.AreEqual("P3", obj.Prop3);
            Assert.AreEqual(-1, obj.Prop4);
        }
    }
}
