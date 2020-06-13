using System;
using Ascentis.ExternalCache.TesterConsole;
using Utf8Json;

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

            Console.WriteLine(@"--- Executing serializing a string ---");
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

            Console.WriteLine(@"--- Executing serializing JSON as byte[] ---");
            initialTickCount = Environment.TickCount;
            Console.WriteLine($@"Start:{initialTickCount}");
            dynamic obj2 = new Dynamo { ["Prop1"] = TextResource.SampleXML };
            var jsonObj = JsonSerializer.Serialize(obj2); 
            for (var i = 0; i < count; i++)
                externalCache.Add($"Item{i}", jsonObj);
            for (var i = 0; i < count; i++)
                externalCache.Get($"Item{i}");
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (insert/retrieves per second): {(count / (((float)(Environment.TickCount - initialTickCount)) / 1000))}");
            externalCacheManager.ClearAllCaches();

            Console.WriteLine(@"Press any key to finish");
            Console.ReadLine();
        }
    }
}
