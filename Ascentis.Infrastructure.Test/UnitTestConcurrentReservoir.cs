using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestConcurrentReservoir
    {
        [TestMethod]
        public void TestMethodConcurrentCircularFactoryBasic()
        {
            var factory = new ConcurrentReservoir<object>(2, () => new object());
            var obj1 = factory.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = factory.Acquire();
            Assert.IsNotNull(obj2);
        }

        [TestMethod]
        public void TestMethodConcurrentCircularFactoryRetrieveTimeout()
        {
            var factory = new ConcurrentReservoir<object>(2, () => new object());
            var obj1 = factory.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = factory.Acquire();
            Assert.IsNotNull(obj2);
            Assert.ThrowsException<TimeoutException>(() => factory.Acquire(1000));
        }

        [TestMethod]
        public void TestMethodConcurrentCircularFactorySimpleReleaseSemantics()
        {
            var factory = new ConcurrentReservoir<object>(2, () => new object());
            var obj1 = factory.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = factory.Acquire();
            Assert.IsNotNull(obj2);
            factory.Release(obj1);
            obj2 = factory.Acquire(1000);
            Assert.IsNotNull(obj2);
            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void TestMethodConcurrentCircularFactoryReleaseInSeparateThread()
        {
            var factory = new ConcurrentReservoir<object>(2, () => new object());
            var obj1 = factory.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = factory.Acquire();
            var obj3 = obj2;
            Assert.IsNotNull(obj2);
            ThreadPool.QueueUserWorkItem(state => factory.Release(obj1)); 
            obj2 = factory.Acquire(1000);
            Assert.IsNotNull(obj2);
            Assert.AreEqual(obj1, obj2);
            factory.Release(obj3);
            obj2 = factory.Acquire();
            Assert.IsNotNull(obj2);
            Assert.AreEqual(obj3, obj2);
        }
    }
}
