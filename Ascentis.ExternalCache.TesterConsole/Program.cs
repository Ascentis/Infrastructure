using System;
using System.Text;
using Ascentis.ExternalCache.TesterConsole;

namespace Ascentis.Infrastructure
{
    class Program
    {
        static void Main()
        {
            const int count = 15000;
            var data = TextResource.SampleXML;
            //var data = "Ping";
            var externalCache = new ComPlusCache();
            //var externalCache = new MemoryCache("Test");
            //externalCache.Select("PerformanceTestCache");
            externalCache.Clear();

            Console.WriteLine(@"--- Executing serializing a string ---");
            var initialTickCount = Environment.TickCount;
            Console.WriteLine($@"Start:{initialTickCount}");
            for (var i = 0; i < count; i++)
                externalCache.Add($"Item{i}", data);
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (insert per second): {(count / (((float)(Environment.TickCount - initialTickCount)) / 1000))}");
            Console.WriteLine($@"Average roundtrip time (microseconds): {((((float)(Environment.TickCount - initialTickCount)) / count * 1000))}");

            initialTickCount = Environment.TickCount;
            // ReSharper disable once NotAccessedVariable
            string s;
            for (var i = 0; i < count; i++)
                // ReSharper disable once RedundantAssignment
                s = (string)externalCache.Get($"Item{i}");
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (retrieves per second): {(count / (((float) (Environment.TickCount - initialTickCount)) / 1000))}");
            Console.WriteLine($@"Average roundtrip time (microseconds): {((((float)(Environment.TickCount - initialTickCount)) / count * 1000))}");
            var externalCacheManager = new ComPlusCacheManager();
            externalCacheManager.ClearAllCaches();
            externalCache.Clear();

            Console.WriteLine(@"--- Executing serializing a byte[] ---");
            var arr = Encoding.UTF8.GetBytes (data); 
            Console.WriteLine($@"Byte array size: {arr.Length}");
            initialTickCount = Environment.TickCount;
            Console.WriteLine($@"Start:{initialTickCount}");
            for (var i = 0; i < count; i++)
                externalCache.Add($"Item{i}", arr);
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (insert per second): {(count / (((float)(Environment.TickCount - initialTickCount)) / 1000))}");
            Console.WriteLine($@"Average roundtrip time (microseconds): {((((float)(Environment.TickCount - initialTickCount)) / count * 1000))}");

            initialTickCount = Environment.TickCount;
            byte[] ba = {};
            for (var i = 0; i < count; i++)
                ba = (byte[])externalCache.Get($"Item{i}");
            Console.WriteLine($@"Byte array read size: {ba.Length}");
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (retrieves per second): {(count / (((float)(Environment.TickCount - initialTickCount)) / 1000))}");
            Console.WriteLine($@"Average roundtrip time (microseconds): {((((float)(Environment.TickCount - initialTickCount)) / count * 1000))}");
            externalCacheManager = new ComPlusCacheManager();
            externalCacheManager.ClearAllCaches();
            externalCache.Clear();

            Console.WriteLine(@"--- Executing serializing complex object ---");
            initialTickCount = Environment.TickCount;
            Console.WriteLine($@"Start:{initialTickCount}");
            var obj = new Dynamo {["Prop1"] = data};
            for (var i = 0; i < count; i++)
                externalCache.Add($"Item{i}", obj);
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (insert per second): {(count / (((float)(Environment.TickCount - initialTickCount)) / 1000))}");
            Console.WriteLine($@"Average roundtrip time (microseconds): {((((float)(Environment.TickCount - initialTickCount)) / count * 1000))}");

            initialTickCount = Environment.TickCount;
            for (var i = 0; i < count; i++)
                // ReSharper disable once RedundantAssignment
                obj = (Dynamo)externalCache.Get($"Item{i}");
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (retrieves per second): {(count / (((float) (Environment.TickCount - initialTickCount)) / 1000))}");
            Console.WriteLine($@"Average roundtrip time (microseconds): {((((float)(Environment.TickCount - initialTickCount)) / count * 1000))}");
            externalCacheManager.ClearAllCaches();
            externalCache.Clear();

            /*
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
            
            Console.WriteLine(@"--- Executing serializing with MessagePack as byte[] ---");
            initialTickCount = Environment.TickCount;
            Console.WriteLine($@"Start:{initialTickCount}");
            dynamic obj3 = new Dynamo { ["Prop1"] = TextResource.SampleXML };
            jsonObj = MessagePack.MessagePackSerializer.Serialize(obj3, ContractlessStandardResolver.Options);
            for (var i = 0; i < count; i++)
                externalCache.Add($"Item{i}", jsonObj);
            for (var i = 0; i < count; i++)
                externalCache.Get($"Item{i}");
            Console.WriteLine($@"Finish:{Environment.TickCount}");
            Console.WriteLine($@"Speed (insert/retrieves per second): {(count / (((float)(Environment.TickCount - initialTickCount)) / 1000))}");
            externalCacheManager.ClearAllCaches();
            */

            Console.WriteLine(@"Press any key to finish");
            Console.ReadLine();
        }
    }
}
