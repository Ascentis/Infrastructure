using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    internal class Tester
    {
        public static int ConstructorCallCount;
        public int A;

        public Tester()
        {
            ConstructorCallCount++;
        }

        public Tester(int a)
        {
            A = a;
            ConstructorCallCount++;
        }
    }

    [TestClass]
    public class UnitTestAutoInit
    {
        [TestMethod]
        public void TestAutoInit()
        {
            Tester ref1 = null;
            Tester ref2 = null;
            Assert.IsNotNull(AutoInit.Ref(ref ref1));
            Assert.IsNotNull(AutoInit.Ref(ref ref1));
            Assert.AreEqual(0, ref1.A);
            Assert.IsNotNull(AutoInit.Ref(ref ref2, 10));
            Assert.IsNotNull(AutoInit.Ref(ref ref2, 10));
            Assert.AreEqual(10, ref2.A);
            Assert.AreEqual(2, Tester.ConstructorCallCount);
        }
    }
}
