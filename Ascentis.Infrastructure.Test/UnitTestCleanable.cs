using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestCleanable
    {
        [TestMethod]
        public void TestCleanable()
        {
            var cleaned = false;
            {
                using var cleanable = new Cleanable<int>(1, value => cleaned = true);
                Assert.AreEqual(1, cleanable.Value);
                Assert.IsFalse(cleaned);
            }
            Assert.IsTrue(cleaned);
        }
    }
}
