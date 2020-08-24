using System.Collections.Generic;
using Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestBitArrayAsDictionaryKey
    {
        [TestMethod]
        public void TestOnOffArrayAsDictionaryKey()
        {
            var dict = new Dictionary<IOnOffArray, int>(new OnOffArrayEqualityComparer());
            var k1 = new OnOffArray(3) {[0] = true, [1] = false, [2] = true};
            var k2 = new OnOffArray(2) {[0] = true, [1] = false};
            var k3 = new OnOffArray(2) {[0] = true, [1] = false};
            dict.Add(k1, 1);
            dict.Add(k2, 2);
            Assert.IsTrue(dict.ContainsKey(k3));
        }
    }
}
