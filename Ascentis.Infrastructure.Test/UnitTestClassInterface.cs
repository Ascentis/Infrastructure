using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    public class TheTesterClass
    {
        public static int Value;

        public static void TestMethod(int value)
        {
            Value = value;
        }
    }

    public class TesterClassInterface<TClass> : ClassInterface<TClass>
    {
        public delegate void TestMethodDelegate(int value);
        public TestMethodDelegate TestMethod { get; protected set; }
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
        }
    }
}
