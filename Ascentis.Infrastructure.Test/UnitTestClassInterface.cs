using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    public class TheTesterClass
    {
        public static int Value { get; set; }

        public static void TestMethodInBase(int value)
        {
            Value = value + 2;
        }

        public static void TestMethod(int value)
        {
            Value = value;
        }
    }

    public class TheDescendant : TheTesterClass
    {
        public new static void TestMethod(int value)
        {
            Value = value + 1;
        }
    }

    public class TheTesterClass2
    {
        public static void TestMethod() { }
    }

    public class TesterClassInterface<TClass> : ClassInterface<TClass>
    {
        public delegate void TestMethodDelegate(int value);
        public TestMethodDelegate TestMethod { get; protected set; }
        public TestMethodDelegate TestMethodInBase { get; protected set; }
    }

    [TestClass]
    public class UnitTestClassInterface
    {
        [TestMethod]
        public void TestClassInterface()
        {
            var classInterface = new TesterClassInterface<TheTesterClass>();
            classInterface.TestMethod(10);
            Assert.AreEqual(10, TheTesterClass.Value);
            classInterface.TestMethodInBase(10);
            Assert.AreEqual(12, TheTesterClass.Value);
        }

        [TestMethod]
        public void TestClassInterfaceBadMethodPrototype()
        {
            // ReSharper disable once NotAccessedVariable
            ClassInterface classInterface;
            Assert.ThrowsException<ArgumentException>(() => classInterface = new TesterClassInterface<TheTesterClass2>());
        }

        [TestMethod]
        public void TestClassInterfaceWithParent()
        {
            var classInterface = new TesterClassInterface<TheDescendant>();
            classInterface.TestMethod(10); // call "overridden" TestMethod()
            Assert.AreEqual(11, TheTesterClass.Value);
            classInterface.TestMethodInBase(10);
            Assert.AreEqual(12, TheTesterClass.Value);
        }
    }
}
