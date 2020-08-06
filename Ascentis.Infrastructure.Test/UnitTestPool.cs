using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestPool
    {
        [TestMethod]
        public void TestMethodPoolBasic()
        {
            var pool = new Pool<object>(2, pool => pool.NewPoolEntry(new object()));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
        }

        [TestMethod]
        public void TestMethodPoolRetrieveTimeout()
        {
            var pool = new Pool<object>(2, pool => pool.NewPoolEntry(new object()));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
            Assert.ThrowsException<TimeoutException>(() => pool.Acquire(1000));
        }

        [TestMethod]
        public void TestMethodPoolSimpleReleaseSemantics()
        {
            var pool = new Pool<object>(2, pool => pool.NewPoolEntry(new object()));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
            pool.Release(obj1);
            obj2 = pool.Acquire(1000);
            Assert.IsNotNull(obj2);
            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void TestMethodPoolReleaseInSeparateThread()
        {
            var pool = new Pool<object>(2, pool => pool.NewPoolEntry(new object()));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            var obj3 = obj2;
            Assert.IsNotNull(obj2);
            ThreadPool.QueueUserWorkItem(state => pool.Release(obj1)); 
            obj2 = pool.Acquire(1000);
            Assert.IsNotNull(obj2);
            Assert.AreEqual(obj1, obj2);
            pool.Release(obj3);
            obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
            Assert.AreEqual(obj3, obj2);
        }

        [TestMethod]
        public void TestMethodPoolWithRefCount()
        {
            var pool = new Pool<object>(10, pool => pool.NewPoolEntry(new object(), 2));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
            pool.Release(obj2);
            var obj3 = pool.Acquire();
            Assert.AreNotEqual(obj3, obj2);
            pool.Release(obj2);
            var obj4 = pool.Acquire();
            Assert.AreEqual(obj4, obj2);
        }
    }
}
