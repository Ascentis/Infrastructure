using Ascentis.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.ComPluCache.Test
{
    [TestClass]
    public class UnitTestComPlusCache
    {
        [TestMethod]
        public void TestCreate()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache())
            {
                Assert.IsNotNull(comPlusCache);
            }
        }

        [TestMethod]
        public void TestTrim()
        {
            using (var comPlusCache = new Infrastructure.ComPlusCache())
            {
                Assert.AreEqual(0, comPlusCache.Trim(100));
            }
        }
    }
}
