using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Manual;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestBlockingQueueSourceAdapter
    {
        [TestMethod]
        public void TestBlockingQueueSourceAdapterCreateWithIntArray()
        {
            var data = new int[100];
            for (var i = 0; i < data.Length; i++)
                data[i] = i;
            var adapter = new BlockingQueueSourceAdapter(data);
            var itemNo = 0;
            foreach (var item in adapter.RowsEnumerable)
                Assert.AreEqual(itemNo++, item.Value[0]);
        }

        [TestMethod]
        public void TestBlockingQueueSourceAdapterCreateWithArraysOfIntArray()
        {
            var data = new object[100][];
            for (var i = 0; i < data.Length; i++)
                data[i] = new[]{(object)i};
            var adapter = new BlockingQueueSourceAdapter(data);
            var itemNo = 0;
            foreach (var item in adapter.RowsEnumerable)
                Assert.AreEqual(itemNo++, item.Value[0]);
        }

        [TestMethod]
        public void TestBlockingQueueSourceAdapterCreateWithArraysOfPoolEntryArray()
        {
            var data = new object[100];
            for (var i = 0; i < data.Length; i++)
                data[i] = new PoolEntry<object[]>(new [] {(object)i});
            var adapter = new BlockingQueueSourceAdapter(data);
            var itemNo = 0;
            foreach (var item in adapter.RowsEnumerable)
                Assert.AreEqual(itemNo++, item.Value[0]);
        }
    }
}
