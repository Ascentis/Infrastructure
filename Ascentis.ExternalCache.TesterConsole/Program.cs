using System;
using Ascentis.ExternalCache.TesterConsole;

namespace Ascentis.Infrastructure
{
    class Program
    {
        static void Main()
        {
            const int count = 10000;
            var externalCache = new ExternalCache();
            externalCache.Select("PerformanceTestCache");
            externalCache.Clear();

            var initialTickCount = Environment.TickCount;
            Console.WriteLine($@"Start:{initialTickCount}");
            for (var i = 0; i < count; i++)
                externalCache.Add($"Item{i}", TextResource.SampleXML);
            for (var i = 0; i < count; i++)
                externalCache.Get($"Item{i}");
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (insert/retrieves per second): {(count / (((float) (Environment.TickCount - initialTickCount)) / 1000))}");
            var externalCacheManager = new ExternalCacheManager();
            externalCacheManager.ClearAllCaches();

            Console.WriteLine(@"--- Executing serializing complex object ---");
            initialTickCount = Environment.TickCount;
            Console.WriteLine($@"Start:{initialTickCount}");
            var obj = new Dynamo {["Prop1"] = TextResource.SampleXML};
            for (var i = 0; i < count; i++)
                externalCache.Add($"Item{i}", obj);
            for (var i = 0; i < count; i++)
                externalCache.Get($"Item{i}");
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (insert/retrieves per second): {(count / (((float) (Environment.TickCount - initialTickCount)) / 1000))}");
            externalCacheManager.ClearAllCaches();

            Console.WriteLine(@"Press any key to finish");
            Console.ReadLine();
        }
    }
}
