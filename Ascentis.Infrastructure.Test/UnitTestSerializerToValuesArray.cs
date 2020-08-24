using System;
using Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestSerializerToValuesArray
    {
        private class BizObj
        {
            public int AInt { get; set; }
            public short AShort { get; set; }
            public string AStr { get; set; }
            public decimal ADec { get; set; }
            public bool ABool { get; set; }
            public DateTime ADateTime { get; set; }
            public double ADouble { get; set; }
            public float AFloat { get; set; }
            public DateTimeOffset ADateTimeOffset { get; set; }
        }

        [TestMethod]
        public void TestBasicSerialization()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float) 1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var values = Serializer<BizObj>.ToValues(obj);
            Assert.AreEqual(9, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.AStr, values[2]);
            Assert.AreEqual(obj.ADec, values[3]);
            Assert.AreEqual(obj.ABool, values[4]);
            Assert.AreEqual(obj.ADateTime, values[5]);
            Assert.AreEqual(obj.ADouble, values[6]);
            Assert.AreEqual(obj.AFloat, values[7]);
            Assert.AreEqual(obj.ADateTimeOffset, values[8]);
        }

        [TestMethod]
        public void TestRestrictingFields()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var fieldsToSerialize = Serializer<BizObj>.BuildFieldEnabledArray();
            fieldsToSerialize[Serializer<BizObj>.IndexOfProperty("AStr")] = false;
            fieldsToSerialize[Serializer<BizObj>.IndexOfProperty("AFloat")] = false;
            var values = Serializer<BizObj>.ToValues(obj, fieldsToSerialize);
            Assert.AreEqual(7, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.ADec, values[2]);
            Assert.AreEqual(obj.ABool, values[3]);
            Assert.AreEqual(obj.ADateTime, values[4]);
            Assert.AreEqual(obj.ADouble, values[5]);
            Assert.AreEqual(obj.ADateTimeOffset, values[6]);
        }

        [TestMethod]
        public void TestSerializationWithSerializerInstance()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var serializer = new Serializer<BizObj>();
            var values = serializer.ToValues(obj);
            Assert.AreEqual(9, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.AStr, values[2]);
            Assert.AreEqual(obj.ADec, values[3]);
            Assert.AreEqual(obj.ABool, values[4]);
            Assert.AreEqual(obj.ADateTime, values[5]);
            Assert.AreEqual(obj.ADouble, values[6]);
            Assert.AreEqual(obj.AFloat, values[7]);
            Assert.AreEqual(obj.ADateTimeOffset, values[8]);
            values = new object[9];
            serializer.ToValues(obj, values);
            Assert.AreEqual(9, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.AStr, values[2]);
            Assert.AreEqual(obj.ADec, values[3]);
            Assert.AreEqual(obj.ABool, values[4]);
            Assert.AreEqual(obj.ADateTime, values[5]);
            Assert.AreEqual(obj.ADouble, values[6]);
            Assert.AreEqual(obj.AFloat, values[7]);
            Assert.AreEqual(obj.ADateTimeOffset, values[8]);
        }

        [TestMethod]
        public void TestRestrictingFieldsWithSerializerInstance()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var fieldsToSerialize = Serializer<BizObj>.BuildFieldEnabledArray();
            fieldsToSerialize[Serializer<BizObj>.IndexOfProperty("AStr")] = false;
            fieldsToSerialize[Serializer<BizObj>.IndexOfProperty("AFloat")] = false;
            var serializer = new Serializer<BizObj> {OnOffsEnabled = fieldsToSerialize};
            var values = serializer.ToValues(obj);
            Assert.AreEqual(7, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.ADec, values[2]);
            Assert.AreEqual(obj.ABool, values[3]);
            Assert.AreEqual(obj.ADateTime, values[4]);
            Assert.AreEqual(obj.ADouble, values[5]);
            Assert.AreEqual(obj.ADateTimeOffset, values[6]);
        }

        [TestMethod]
        public void TestBuildingSerializerMap()
        {
            var map = new []
            {
                "AShort",
                "AInt",
                "ADouble",
                "ADec",
                "AFloat",
                "ADateTimeOffset",
                "AStr",
                "ABool",
                "ADateTime"
            };
            var indexMap = Serializer<BizObj>.BuildFieldMap(map);
            Assert.AreEqual(1, indexMap[0]);
            Assert.AreEqual(0, indexMap[1]);
            Assert.AreEqual(6, indexMap[2]);
            Assert.AreEqual(3, indexMap[3]);
            Assert.AreEqual(7, indexMap[4]);
            Assert.AreEqual(8, indexMap[5]);
            Assert.AreEqual(2, indexMap[6]);
            Assert.AreEqual(4, indexMap[7]);
            Assert.AreEqual(5, indexMap[8]);
        }

        [TestMethod]
        public void TestBuildingSerializerMapWithGaps()
        {
            var map = new []
            {
                "AShort",
                "AInt",
                "ADouble",
                "AFloat",
                "ADateTimeOffset",
                "ABool",
                "ADateTime"
            };
            var indexMap = Serializer<BizObj>.BuildFieldMap(map);
            Assert.AreEqual(1, indexMap[0]);
            Assert.AreEqual(0, indexMap[1]);
            Assert.AreEqual(-1, indexMap[2]);
            Assert.AreEqual(-1, indexMap[3]);
            Assert.AreEqual(5, indexMap[4]);
            Assert.AreEqual(6, indexMap[5]);
            Assert.AreEqual(2, indexMap[6]);
            Assert.AreEqual(3, indexMap[7]);
            Assert.AreEqual(4, indexMap[8]);
        }

        [TestMethod] public void TestBuildingSerializerMapColumnDoesNotExistsFails()
        {
            var map = new []
            {
                "AShort",
                "AInt",
                "ADouble",
                "AFloat",
                "ADateTimeOffsetx",
                "ABool",
                "ADateTime"
            };
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                Serializer<BizObj>.BuildFieldMap(map);
            });
        }

        [TestMethod]
        public void TestBuildingSerializerWithMap()
        {
            var map = new[]
            {
                "AShort",
                "AInt",
                "ADouble",
                "AFloat",
                "ADateTimeOffset",
                "ABool",
                "ADateTime"
            };
            var indexMap = Serializer<BizObj>.BuildFieldMap(map);
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var values = Serializer<BizObj>.ToValues(obj, null, indexMap);
            Assert.AreEqual(obj.AShort, values[0]);
            Assert.AreEqual(obj.AInt, values[1]);
            Assert.AreEqual(obj.ADouble, values[2]);
            Assert.AreEqual(obj.AFloat, values[3]);
            Assert.AreEqual(obj.ADateTimeOffset, values[4]);
            Assert.AreEqual(obj.ABool, values[5]);
            Assert.AreEqual(obj.ADateTime, values[6]);
        }

        [TestMethod]
        public void TestSerializationUsingObject()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var values = Serializer<object>.ToValues(obj);
            Assert.AreEqual(9, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.AStr, values[2]);
            Assert.AreEqual(obj.ADec, values[3]);
            Assert.AreEqual(obj.ABool, values[4]);
            Assert.AreEqual(obj.ADateTime, values[5]);
            Assert.AreEqual(obj.ADouble, values[6]);
            Assert.AreEqual(obj.AFloat, values[7]);
            Assert.AreEqual(obj.ADateTimeOffset, values[8]);
        }

        [TestMethod]
        public void TestSerializationUsingObjectAndSerializerInstance()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var serializer = new Serializer<object>();
            var values = serializer.ToValues(obj);
            Assert.AreEqual(9, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.AStr, values[2]);
            Assert.AreEqual(obj.ADec, values[3]);
            Assert.AreEqual(obj.ABool, values[4]);
            Assert.AreEqual(obj.ADateTime, values[5]);
            Assert.AreEqual(obj.ADouble, values[6]);
            Assert.AreEqual(obj.AFloat, values[7]);
            Assert.AreEqual(obj.ADateTimeOffset, values[8]);
        }

        [TestMethod]
        public void TestRestrictingFieldsWithSerializerInstanceAndBaseObjectSerializer()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var fieldsToSerialize = Serializer<object>.BuildFieldEnabledArray(obj);
            fieldsToSerialize[Serializer<object>.IndexOfProperty(obj, "AStr")] = false;
            fieldsToSerialize[Serializer<object>.IndexOfProperty(obj, "AFloat")] = false;
            var serializer = new Serializer<object> { OnOffsEnabled = fieldsToSerialize };
            var values = serializer.ToValues(obj);
            Assert.AreEqual(7, values.Length);
            Assert.AreEqual(obj.AInt, values[0]);
            Assert.AreEqual(obj.AShort, values[1]);
            Assert.AreEqual(obj.ADec, values[2]);
            Assert.AreEqual(obj.ABool, values[3]);
            Assert.AreEqual(obj.ADateTime, values[4]);
            Assert.AreEqual(obj.ADouble, values[5]);
            Assert.AreEqual(obj.ADateTimeOffset, values[6]);
        }

        [TestMethod]
        public void TestBuildingSerializerWithMapAndSerializerInstanceOfObject()
        {
            var map = new[]
            {
                "AShort",
                "AInt",
                "ADouble",
                "AFloat",
                "ADateTimeOffset",
                "ABool",
                "ADateTime"
            };
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var indexMap = Serializer<object>.BuildFieldMap(obj, map);
            var fieldsToSerialize = Serializer<object>.BuildFieldEnabledArray(obj);
            fieldsToSerialize[Serializer<object>.IndexOfProperty(obj, "AInt")] = false;
            var serializer = new Serializer<object>
            {
                FieldMap = indexMap, 
                OnOffsEnabled = fieldsToSerialize
            };
            Assert.ThrowsException<InvalidOperationException>(() => serializer.ToValues(obj));
        }

        [TestMethod]
        public void TestSerializationPerformance()
        {
            var obj = new BizObj
            {
                AInt = 10,
                AShort = 9,
                AStr = "hello",
                ADec = 10.2M,
                ABool = true,
                ADateTime = DateTime.MaxValue,
                ADouble = 1.23,
                AFloat = (float)1.5,
                ADateTimeOffset = DateTimeOffset.MaxValue
            };
            var serializer = new Serializer<BizObj>();
            var values = new object[serializer.FieldCount];
            Assert.AreEqual(9, serializer.GetFieldCount(obj));
            for (var i = 0; i < 1000000; i++)
                Serializer<BizObj>.ToValues(obj, values);
        }
    }
}
