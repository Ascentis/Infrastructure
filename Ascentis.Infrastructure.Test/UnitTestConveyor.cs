using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestConveyor
    {
        [TestMethod]
        public void TestMethodConveyorBasic()
        {
            var sum = 0;
            var conveyor = new Conveyor<int>((packet, context) =>
            {
                sum += packet;
            });
            conveyor.Start();
            conveyor.InsertPacket(1);
            conveyor.InsertPacket(2);
            conveyor.StopAndWait();
            Assert.AreEqual(3, sum);
        }

        [TestMethod]
        public void TestMethodConveyorBadUsageThrowExceptions()
        {
            var sum = 0;
            var conveyor = new Conveyor<int>((packet, context) =>
            {
                sum += packet;
            });
            Assert.ThrowsException<InvalidOperationException>(() => conveyor.Stop());
            Assert.ThrowsException<InvalidOperationException>(() => conveyor.StopAndWait());
            conveyor.Start();
            Assert.ThrowsException<InvalidOperationException>(() => conveyor.Start());
            conveyor.StopAndWait();
            Assert.ThrowsException<InvalidOperationException>(() => conveyor.Stop());
            conveyor.Start();
            conveyor.StopAndWait();
            Assert.ThrowsException<InvalidOperationException>(() => conveyor.StopAndWait());
            Assert.AreEqual(0, sum);
        }

        [TestMethod]
        public void TestMethodConveyorThrowExceptionWithin()
        {
            var sum = 0;
            var conveyor = new Conveyor<int>((packet, context) =>
            {
                sum += packet;
                if (sum >= 3)
                    throw new Exception("Blow up");
            });
            conveyor.Start();
            conveyor.InsertPacket(1);
            conveyor.InsertPacket(2);
            conveyor.InsertPacket(0);
            Thread.Sleep(100);
            Assert.ThrowsException<ConveyorException>(() => conveyor.InsertPacket(0));
            Assert.AreEqual(3, sum);
        }

        [TestMethod]
        public void TestMethodConveyorStopOnExceptedState()
        {
            var sum = 0;
            var conveyor = new Conveyor<int>((packet, context) =>
            {
                sum += packet;
                if (sum >= 3)
                    throw new Exception("Blow up");
            });
            conveyor.Start();
            conveyor.InsertPacket(1);
            conveyor.InsertPacket(2);
            conveyor.InsertPacket(0);
            Thread.Sleep(100);
            Assert.ThrowsException<ConveyorException>(() => conveyor.Stop());
            Assert.AreEqual(3, sum);
        }

        [TestMethod]
        public void TestMethodConveyorStopAndWaitOnExceptedState()
        {
            var sum = 0;
            var conveyor = new Conveyor<int>((packet, context) =>
            {
                sum += packet;
                if (sum >= 3)
                    throw new Exception("Blow up");
            });
            conveyor.Start();
            conveyor.InsertPacket(1);
            conveyor.InsertPacket(2);
            conveyor.InsertPacket(0);
            Thread.Sleep(100);
            Assert.ThrowsException<ConveyorException>(() => conveyor.StopAndWait());
            Assert.AreEqual(3, sum);
        }
    }
}
