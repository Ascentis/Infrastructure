using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascentis.Infrastructure
{
    class Program
    {
        static void Main(string[] args)
        {
            var externalCache = new ExternalCache();
            externalCache.Clear();
            var externalCacheItem = new ExternalCacheItem();
            externalCacheItem.Container.Prop1 = "Hello";
            externalCache.Add("Item1", externalCacheItem);
            ExternalCacheItem item = (ExternalCacheItem) externalCache.Get("Item1");
            Console.WriteLine(item != null ? item.Container.Prop1 : "item is null");
            Console.WriteLine("Press any key to finish");
            Console.ReadLine();
        }
    }
}
