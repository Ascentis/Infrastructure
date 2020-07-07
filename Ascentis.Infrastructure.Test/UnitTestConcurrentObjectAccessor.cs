using Ascentis.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestAsyncDisposer
{
    [TestClass]
    public class UnitTestConcurrentObjectAccessor
    {
        [TestMethod]
        public void TestCreate()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject>();
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
        }

        [TestMethod]
        public void TestCreateWithName()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            Assert.AreEqual("Hello", accessor.Reference.Name);
        }

        [TestMethod]
        public void TestExecuteReadLockedFunction()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            var retVal = accessor.ExecuteReadLocked(obj => obj.Name);
            Assert.AreEqual("Hello", retVal);
        }

        [TestMethod]
        public void TestExecuteReadLockedProcedure()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject, TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            string retVal = "";
            accessor.ExecuteReadLocked(obj =>
            {
                retVal = obj.Name;
            });
            Assert.AreEqual("Hello", retVal);
        }

        [TestMethod]
        public void TestSwapNewAndExecuteFunction()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject, TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            var retVal = accessor.SwapNewAndExecute(newObj => { }, obj => obj.Name);
            Assert.AreEqual("Hello", retVal);
        }

        [TestMethod]
        public void TestSwapNewAndExecuteProcedure()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject, TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            string retVal = "";
            accessor.SwapNewAndExecute(obj =>
            {
                retVal = obj.Name;
            });
            Assert.AreEqual("Hello", retVal);
        }

        [TestMethod]
        public void TestSwapNewAndExecuteGatedFunction()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject, TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            var retVal = accessor.SwapNewAndExecute(gate => true, obj => obj.Name);
            Assert.AreEqual("Hello", retVal);
            retVal = accessor.SwapNewAndExecute(gate => false, obj => obj.Name);
            Assert.AreEqual(null, retVal);
        }

        [TestMethod]
        public void TestSwapNewAndExecuteGatedProcedure()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject, TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            string retVal = "";
            accessor.SwapNewAndExecute(gate => true, obj =>
            {
                retVal = obj.Name;
            });
            Assert.AreEqual("Hello", retVal);
            retVal = null;
            accessor.SwapNewAndExecute(gate => false, obj =>
            {
                retVal = obj.Name;
            });
            Assert.AreEqual(null, retVal);
        }

        [TestMethod]
        public void TestSwapNewAndExecuteProcedureWithInit()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject, TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            string retVal = "";
            accessor.SwapNewAndExecute(obj => { obj.Name = "Replaced"; },obj => { retVal = obj.Name; });
            Assert.AreEqual("Hello", retVal);
            Assert.AreEqual("Replaced", accessor.Reference.Name);
        }

        [TestMethod]
        public void TestSwapNewAndExecuteFunctionWithInit()
        {
            var accessor = new ConcurrentObjectAccessor<TestObject, TestObject>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            var retVal = accessor.SwapNewAndExecute(obj => { obj.Name = "Replaced"; }, obj => obj.Name);
            Assert.AreEqual("Hello", retVal);
            Assert.AreEqual("Replaced", accessor.Reference.Name);
        }
    }
}
