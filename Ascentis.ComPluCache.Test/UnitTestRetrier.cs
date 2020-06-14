using System;
using Ascentis.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.ComPluCache.Test
{
    public class TheClass
    {

    }

    [TestClass]
    public class UnitTestRetrier
    {
        public UnitTestRetrier()
        {
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethodCreateRetrier()
        {
            var retrier = new Retrier<TheClass>(new TheClass());
            Assert.IsNotNull(retrier);
        }

        [TestMethod]
        public void TestMethodRetryProcedureOnException()
        {
            var retrier = new Retrier<TheClass>(new TheClass());
            var counter = 0;
            retrier.Retriable(delegate
            {
                if(counter++ == 0) 
                    throw new Exception();
            });  
            Assert.AreEqual(2, counter);
        }

        [TestMethod]
        public void TestMethodRetryFunctionOnException()
        {
            var retrier = new Retrier<TheClass>(new TheClass());
            var counter = 0;
            var retVal = retrier.Retriable(delegate
            {
                if (counter++ == 0)
                    throw new Exception();
                return counter;
            });
            Assert.AreEqual(2, counter);
            Assert.AreEqual(2, retVal);
        }

        [TestMethod]
        public void TestMethodRetryProcedureWithCustomCanRetryOnException()
        {
            var retrier = new Retrier<TheClass>(new TheClass(), (e, _counter) => e is Exception && _counter <= 2);
            var counter = 0;
            retrier.Retriable(delegate
            {
                if (counter++ <= 2)
                    throw new Exception();
            });
            Assert.AreEqual(4, counter);
            counter = 0;
            Assert.ThrowsException<Exception>(delegate
            {
                retrier.Retriable(delegate
                {
                    if (counter++ <= 3)
                        throw new Exception();
                });
            });
        }
    }
}
