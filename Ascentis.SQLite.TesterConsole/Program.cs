using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading;
using Ascentis.Infrastructure;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Utils;

namespace Ascentis.SQLite.TesterConsole
{
    class BizObject
    {
        public int IntValue { get; set; }
    }

    class Program
    {
        delegate int GetIntDelegate(BizObject obj);
        static void Main()
        {
            try
            {
                var obj = new BizObject();
                var pool = new Pool<BizObject>(int.MaxValue, p => new PoolEntry<BizObject>(obj));

                var stopwatch = new Stopwatch();
                var slot = Thread.AllocateDataSlot();
                Thread.SetData(slot, new object[]{obj});
                BizObject v = obj;
                stopwatch.Start();
                for (var i = 0; i < 1000000; i++)
                {
                    var entry = pool.Acquire();
                    v = entry.Value;
                    pool.Release(entry);
                    //var arr = (object[]) Thread.GetData(slot);
                    //var arr = new object[] {obj};
                    //v = (BizObject)arr[0];
                }
                stopwatch.Stop();
                
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                Console.WriteLine(v.IntValue);

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
