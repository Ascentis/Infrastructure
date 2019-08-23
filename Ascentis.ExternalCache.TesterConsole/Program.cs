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
            externalCache.Select("PerformanceTestCache");
            externalCache.Clear();
            var initialTickCount = Environment.TickCount;
            Console.WriteLine("Start:" + initialTickCount);
            for (var i = 0; i < 1000; i++)
            {
                var externalCacheItem = new ExternalCacheItem();
                externalCacheItem.Container.Prop1 = "Hello" + i;
                externalCache.Add("Item" + i, "hello");
            }
            for (var i = 0; i < 1000; i++)
            {
                var externalCacheItem = externalCache.Get("Item" + i);
                //string s = externalCacheItem.Container.Prop1;
            }
            Console.WriteLine("Finish:" + Environment.TickCount);
            Console.WriteLine("Speed (insert/retrieves per second): " + (1000 / (((float)(Environment.TickCount - initialTickCount)) / 1000)));
            var externalCacheManager = new ExternalCacheManager();
            externalCacheManager.ClearAllCaches();
            Console.WriteLine("Press any key to finish");
            Console.ReadLine();
        }
    }
}
