using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestResettable
    {
        [TestMethod]
        public void TestResettable()
        {
            var cleaned = false;
            {
                using var resettable = new Resettable<int>(1, value => cleaned = true);
                Assert.AreEqual(1, resettable.Value);
                Assert.IsFalse(cleaned);
            }
            Assert.IsTrue(cleaned);
        }
    }
}
