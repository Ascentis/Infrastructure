using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "VariableHidesOuterVariable")]
    public class UnitTestPool
    {
        [TestMethod]
        public void TestPoolBasic()
        {
            var pool = new Pool<object>(2, pool => pool.NewPoolEntry(new object()));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
        }

        [TestMethod]
        public void TestPoolRetrieveTimeout()
        {
            var pool = new Pool<object>(2, pool => pool.NewPoolEntry(new object()));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
            Assert.ThrowsException<TimeoutException>(() => pool.Acquire(1000));
        }

        [TestMethod]
        public void TestPoolSimpleReleaseSemantics()
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
        public void TestPoolReleaseInSeparateThread()
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
        public void TestPoolWithRefCount()
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

        [TestMethod]
        public void TestPoolParallelAcquireAndRelease()
        {
            var pool = new Pool<object>(100000, pool => pool.NewPoolEntry(new object()));
            var items = new ConcurrentBag<PoolEntry<object>>();
            Parallel.Invoke(() =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    items.Add(pool.Acquire(1));
                    if (i != 1000) continue;
                    pool.MaxCapacity += 1;
                    pool.MaxCapacity -= 1;
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    items.Add(pool.Acquire(1));
                    if (i != 1000) continue;
                    pool.MaxCapacity += 1;
                    pool.MaxCapacity -= 1;
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    items.Add(pool.Acquire(1));
                    if (i != 1000) continue;
                    pool.MaxCapacity += 1;
                    pool.MaxCapacity -= 1;
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    items.Add(pool.Acquire(1));
                    if (i != 1000) continue;
                    pool.MaxCapacity += 1;
                    pool.MaxCapacity -= 1;
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    items.Add(pool.Acquire(1));
                    if (i != 1000) continue;
                    pool.MaxCapacity += 1;
                    pool.MaxCapacity -= 1;
                }
            });
            Assert.ThrowsException<TimeoutException>(() => pool.Acquire(1));
            Assert.AreEqual(pool.MaxCapacity, items.Count);
            Parallel.Invoke(() =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    Assert.IsTrue(items.TryTake(out var item));
                    pool.Release(item);
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    Assert.IsTrue(items.TryTake(out var item));
                    pool.Release(item);
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    Assert.IsTrue(items.TryTake(out var item));
                    pool.Release(item);
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    Assert.IsTrue(items.TryTake(out var item));
                    pool.Release(item);
                }
            }, () =>
            {
                for (var i = 0; i < 20000; i++)
                {
                    Assert.IsTrue(items.TryTake(out var item));
                    pool.Release(item);
                }
            });
            Assert.AreEqual(0, items.Count);
            pool.Acquire(1);
        }

        [TestMethod]
        public void TestPoolWithRefCountAndRetainSemantics()
        {
            var pool = new Pool<object>(10, pool => pool.NewPoolEntry(new object(), 2));
            var obj1 = pool.Acquire();
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire();
            Assert.IsNotNull(obj2);
            pool.Release(obj2);
            var obj3 = pool.Acquire();
            Assert.AreNotEqual(obj3, obj2);
            obj2.Retain();
            pool.Release(obj2);
            var obj4 = pool.Acquire();
            Assert.AreNotEqual(obj4, obj2);
            obj2.Pool.Release(obj2);
            obj4 = pool.Acquire();
            Assert.AreEqual(obj4, obj2);
        }

        [TestMethod]
        public void TestPoolChangeMaxCapacity()
        {
            var pool = new Pool<object>(3, pool => pool.NewPoolEntry(new object()));
            var obj1 = pool.Acquire(1);
            Assert.IsNotNull(obj1);
            var obj2 = pool.Acquire(1);
            Assert.IsNotNull(obj2);
            Assert.AreNotEqual(obj1, obj2);
            pool.MaxCapacity = 4;
            var obj3 = pool.Acquire(1);
            Assert.IsNotNull(obj3);
            var obj4 = pool.Acquire(1);
            Assert.IsNotNull(obj4);
            Assert.ThrowsException<TimeoutException>(() => pool.Acquire(1));
            pool.MaxCapacity = 3;
            pool.Release(obj4);
            pool.Release(obj3);
            pool.Release(obj2);
            pool.Release(obj1);
            obj1 = pool.Acquire(1);
            pool.Acquire(1);
            pool.Acquire(1);
            Assert.ThrowsException<TimeoutException>(() => pool.Acquire(1));
            pool.Release(obj1);
            pool.Acquire(1);
        }
    }
}
