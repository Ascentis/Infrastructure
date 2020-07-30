using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TesterClass
    {
        public readonly int A;
        public TesterClass(int a)
        {
            A = a;
        }

        public TesterClass(int a, int b)
        {
            A = a + b;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TesterClass2
    {
        public readonly int A;
        public TesterClass2(int a)
        {
            A = a;
        }

        public TesterClass2(int a, int b)
        {
            A = a + b;
        }
    }

    [TestClass]
    public class UnitTestGenericObjectBuilder
    {
        [TestMethod]
        public void TestObjectConstructor()
        {
            var sourceQueue = new Queue<string>();
            sourceQueue.Enqueue("hello");
            var newQueue = GenericObjectBuilder.Build<Queue<string>>(sourceQueue);
            var firstItem = sourceQueue.Dequeue();
            var secondItem = newQueue.Dequeue();
            Assert.AreEqual(firstItem, secondItem);
        }

        [TestMethod]
        public void TestCreateMultipleObjects()
        {
            GenericObjectBuilder.ClearConstructorDelegatesCache();
            var sourceQueue = new Queue<string>();
            sourceQueue.Enqueue("hello");
            var newQueue = GenericObjectBuilder.Build<Queue<string>>(sourceQueue);
            var thirdQueue = GenericObjectBuilder.Build<Queue<string>>(sourceQueue);
            var firstItem = sourceQueue.Dequeue();
            var secondItem = newQueue.Dequeue();
            var thirdItem = thirdQueue.Dequeue();
            Assert.AreEqual(firstItem, secondItem);
            Assert.AreEqual(firstItem, thirdItem);
            Assert.AreEqual(1, GenericObjectBuilder.CachedConstructorDelegatesCount);
        }

        [TestMethod]
        public void TestObjectConstructorWithTwoTypes()
        {
            var sourceQueue = new Queue<string>();
            sourceQueue.Enqueue("hello");
            var newQueue = GenericObjectBuilder.Build<Queue<string>>(sourceQueue);
            var firstItem = sourceQueue.Dequeue();
            var secondItem = newQueue.Dequeue();
            var sourceArray = new List<string> {"hello"};
            var newArray = GenericObjectBuilder.Build<List<string>>(sourceArray);
            var firstArrayItem = sourceArray[0];
            var secondArrayItem = newArray[0];
            Assert.AreEqual(firstItem, secondItem);
            Assert.AreEqual(firstArrayItem, secondArrayItem);
        }

        [TestMethod]
        public void TestObjectConstructorFailsPassingNullParam()
        {
            GenericObjectBuilder.ClearConstructorDelegatesCache();
            Assert.ThrowsException<ArgumentException>(() => GenericObjectBuilder.Build<Queue<string>>(new object[]{null}));
        }

        [TestMethod]
        public void TestObjectConstructorSucceedsPassingNullParam()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => GenericObjectBuilder.Build<Queue<string>>(new[] {typeof(Queue<string>)}, new object[]{null}));
            Assert.AreEqual("collection", e.ParamName);
        }

        [TestMethod]
        public void TestObjectConstructorsSameClass()
        {
            GenericObjectBuilder.ClearConstructorDelegatesCache();
            var sourceArray = new List<string> {"hello"};
            GenericObjectBuilder.Build<List<string>>(sourceArray);
            GenericObjectBuilder.Build<List<string>>(1);
            Assert.AreEqual(2, GenericObjectBuilder.CachedConstructorDelegatesCount);
        }

        [TestMethod]
        public void TestTesterClassesWithMultipleConstructors()
        {
            GenericObjectBuilder.ClearConstructorDelegatesCache();
            var obj1 = GenericObjectBuilder.Build<TesterClass>(10);
            var obj2 = GenericObjectBuilder.Build<TesterClass>(20);
            Assert.AreEqual(10, obj1.A);
            Assert.AreEqual(20, obj2.A);
            Assert.AreEqual(1, GenericObjectBuilder.CachedConstructorDelegatesCount);
            var obj3 = GenericObjectBuilder.Build<TesterClass2>(10);
            var obj4 = GenericObjectBuilder.Build<TesterClass2>(20);
            Assert.AreEqual(10, obj3.A);
            Assert.AreEqual(20, obj4.A);
            Assert.AreEqual(2, GenericObjectBuilder.CachedConstructorDelegatesCount);
            obj1 = GenericObjectBuilder.Build<TesterClass>(10, 20);
            obj2 = GenericObjectBuilder.Build<TesterClass>(20, 30);
            Assert.AreEqual(30, obj1.A);
            Assert.AreEqual(50, obj2.A);
            Assert.AreEqual(3, GenericObjectBuilder.CachedConstructorDelegatesCount);
            obj3 = GenericObjectBuilder.Build<TesterClass2>(10, 20);
            obj4 = GenericObjectBuilder.Build<TesterClass2>(20, 30);
            Assert.AreEqual(30, obj3.A);
            Assert.AreEqual(50, obj4.A);
            Assert.AreEqual(4, GenericObjectBuilder.CachedConstructorDelegatesCount);
        }

        [TestMethod]
        public void TestTesterPerformanceUsingActivator()
        {
            for (var i = 0; i < 1024 * 1024; i++)
            {
                Activator.CreateInstance(typeof(TesterClass), 10);
                Activator.CreateInstance(typeof(TesterClass), 20);
            }
        }

        [TestMethod]
        public void TestTesterPerformanceUsingGenericObjectBuilder()
        {
            var paramTypes = new[] {typeof(int)};
            var builder = GenericObjectBuilder.Builder<TesterClass>(paramTypes);
            for (var i = 0; i < 1024 * 1024; i++)
            {
                builder(10);
                builder(20);
            }
        }

        [TestMethod]
        public void TestTesterPerformanceUsingReflection()
        {
            var constructorInfo = typeof(TesterClass).GetConstructor(new[] { typeof(int) });
            Assert.IsNotNull(constructorInfo);
            for (var i = 0; i < 1024 * 1024; i++)
            {
                constructorInfo.Invoke(new object [] {10});
                constructorInfo.Invoke(new object [] {20});
            }
        }

        [TestMethod]
        public void TestTesterPerformanceUsingDirectConstruction()
        {
            for (var i = 0; i < 1024 * 1024; i++)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new TesterClass(10);
                // ReSharper disable once ObjectCreationAsStatement
                new TesterClass(20);
            }
        }
    }
}
