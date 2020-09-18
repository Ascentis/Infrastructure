using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestArgsChecker
    {
        [TestMethod]
        public void TestMethodCheck()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ArgsChecker.Check<ArgumentOutOfRangeException>(false, "count", "message"));
        }
    }
}
