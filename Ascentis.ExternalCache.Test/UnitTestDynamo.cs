using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestDynamo
    {
        [TestMethod]
        public void TestCreateDynamo()
        {
            var item = new Dynamo(); 
            Assert.IsNotNull(item);
        }

        [TestMethod]
        public void TestSetPropertiesOfContainer()
        {
            dynamic item = new Dynamo();
            item.Prop1 = 1;
            item.Prop2 = "Hello";
            Assert.AreEqual(1, item.Prop1);
            Assert.AreEqual("Hello", item.Prop2);
        }

        [TestMethod]
        public void TestSetPropertiesOfContainerUsingArray()
        {
            var item = new Dynamo();
            item["Prop1"] = 1;
            item["Prop2"] = "Hello";
            Assert.AreEqual(1, item["Prop1"]);
            Assert.AreEqual("Hello", item["Prop2"]);
        }

        [TestMethod]
        public void TestSetPropertiesUsingArray()
        {
            var item = new Dynamo();
            item["Prop1"] = 1;
            item["Prop2"] = "Hello";
            Assert.AreEqual(1, item["Prop1"]);
            Assert.AreEqual("Hello", item["Prop2"]);
        }

        [TestMethod]
        public void TestCopyFromAndTo()
        {
            dynamic item = new Dynamo();
            var obj = new TestClass {Prop1 = "P1", Prop2 = "P2", Prop3 = "P3", Prop4 = -1};
            item.CopyFrom(obj);
            Assert.AreEqual("P1", item.Prop1);
            Assert.AreEqual("P2", item.Prop2);
            Assert.AreEqual("P3", item.Prop3);
            Assert.AreEqual(-1, item.Prop4);
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
            dynamic item2 = new Dynamo();
            item.CopyTo(item2);
            Assert.AreEqual(4, item2.Count);
            Assert.AreEqual("P1", item2.Prop1);
            Assert.AreEqual("P2", item2.Prop2);
            Assert.AreEqual("P3", item2.Prop3);
            Assert.AreEqual(-1, item2.Prop4);
            dynamic item3 = new Dynamo();

            item3.CopyFrom(item2);
            Assert.AreEqual(4, item3.Count);
            Assert.AreEqual("P1", item3.Prop1);
            Assert.AreEqual("P2", item3.Prop2);
            Assert.AreEqual("P3", item3.Prop3);
            Assert.AreEqual(-1, item3.Prop4);
        }
    }
}
