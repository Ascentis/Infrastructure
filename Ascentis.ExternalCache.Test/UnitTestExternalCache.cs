using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestExternalCache
    {
        [TestMethod]
        public void TestCreateExternalCache()
        {
            var externalCache = new ExternalCache();
            Assert.IsNotNull(externalCache);
        }
    }
}
