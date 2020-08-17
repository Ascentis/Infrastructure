using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class UnitTestObjectExtensions
    {
        private class TesterClass
        {
            public short AShort { get; set; }
            public short? AShortNullable { get; set; }
            public int AInt { get; set; }
            public int? AIntNullable { get; set; }
            public long ALong{ get; set; }
            public long? ALongNullable { get; set; }
            public ushort AUShort { get; set; }
            public ushort? AUShortNullable { get; set; }
            public uint AUInt { get; set; }
            public uint? AUIntNullable { get; set; }
            public ulong AULong { get; set; }
            public ulong? AULongNullable { get; set; }
        }

        [TestMethod]
        public void TestAutoCastPropSetter_Short()
        {
            const string propName = "AShort";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual(1, obj.AShort);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, 1 + short.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, short.MinValue - 1));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (long)1 + short.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (long)short.MinValue - 1));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (double)1 + short.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (double)short.MinValue - 1));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (float)1 + short.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (float)short.MinValue - 1));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1 + short.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)short.MinValue - 1));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (ushort)(1 + short.MaxValue)));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (uint)(1 + short.MaxValue)));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (ulong)(1 + short.MaxValue)));
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual(1, obj.AShort);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual(1, obj.AShort);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_ShortNullable()
        {
            const string propName = "AShortNullable";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual((short?)1, obj.AShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual((short?)1, obj.AShortNullable);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_Int()
        {
            const string propName = "AInt";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual(1, obj.AInt);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (long)1 + int.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (long)int.MinValue - 1));
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (double)1 + int.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (double)int.MinValue - 1));
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1 + int.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)int.MinValue - 1));
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (ulong)(1 + (long)int.MaxValue)));
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual(1, obj.AInt);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual(1, obj.AInt);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_IntNullable()
        {
            const string propName = "AIntNullable";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual(1, obj.AIntNullable);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual(1, obj.AIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual(1, obj.AIntNullable);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_Long()
        {
            const string propName = "ALong";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual(1, obj.ALong);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1 + long.MaxValue));
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)long.MinValue - 1));
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, 1 + (ulong)long.MaxValue));
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual(1, obj.ALong);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual(1, obj.ALong);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_LongNullable()
        {
            const string propName = "ALongNullable";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual(1, obj.ALongNullable);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual(1, obj.ALongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual(1, obj.ALongNullable);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_UShort()
        {
            const string propName = "AUShort";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual(1, obj.AUShort);
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (long)1 + ushort.MaxValue));
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (double)1 + ushort.MaxValue));
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (float)1 + ushort.MaxValue));
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1 + ushort.MaxValue));
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (uint)(1 + ushort.MaxValue)));
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (ulong)(1 + ushort.MaxValue)));
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual(1, obj.AUShort);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual(1, obj.AUShort);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_UShortNullable()
        {
            const string propName = "AUShortNullable";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual((ushort?)1, obj.AUShortNullable);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_UInt()
        {
            const string propName = "AUInt";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (long)1 + uint.MaxValue));
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (double)1 + uint.MaxValue));
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1 + uint.MaxValue));
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (ulong)(1 + (long)uint.MaxValue)));
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual((uint)1, obj.AUInt);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual((uint)1, obj.AUInt);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_UIntNullable()
        {
            const string propName = "AUIntNullable";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual((uint)1, obj.AUIntNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual((uint)1, obj.AUIntNullable);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_ULong()
        {
            const string propName = "AULong";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.ThrowsException<OverflowException>(() => ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1 + ulong.MaxValue));
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual((ulong)1, obj.AULong);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual((ulong)1, obj.AULong);
        }

        [TestMethod]
        public void TestAutoCastPropSetter_ULongNullable()
        {
            const string propName = "AULongNullable";
            var obj = new TesterClass();
            ObjectExtensions.SetPropertyValue(propName, obj, (short)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, 1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (long)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (double)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (float)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (decimal)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (sbyte)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (byte)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ushort)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (uint)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, (ulong)1);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, true);
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, "1");
            Assert.AreEqual((ulong)1, obj.AULongNullable);
            ObjectExtensions.SetPropertyValue(propName, obj, '1');
            Assert.AreEqual((ulong)1, obj.AULongNullable);
        }

        [TestMethod]
        public void TestArgsCheckSettingProperty()
        {
            var obj = new TesterClass();
            Assert.ThrowsException<ArgumentNullException>(() => obj.SetPropertyValue("NotExists", null));
            Assert.ThrowsException<ArgumentNullException>(() => (null as TesterClass).SetPropertyValue("AInt", 1));
        }
    }
}
