using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ascentis.Infrastructure
{
    public class ConcurrentBagSlim<T> where T : class
    {
        struct SlotRec
        {
            public T data;
            public int stale;
        }

        public int DefaultBagSlimSize = 100;
        private SlotRec _slots0 = new SlotRec();
        private SlotRec _slots1;

        private volatile int _concurrentThreadsCount;
        private int _opCount;
        /*public int Size
        {
            get => _slots.Length;
            set => _slots = new T[value];
        }*/

        public ConcurrentBagSlim()
        {
            //Size = DefaultBagSlimSize;
        }

        public void Add(T item)
        {

            while (true)
            {
                var localConcurrentThreadCount = Interlocked.Increment(ref _concurrentThreadsCount);
                if (localConcurrentThreadCount <= 4)
                    break;
                Interlocked.Decrement(ref _concurrentThreadsCount);
                Thread.Yield();
            }

            while (true)
            {
                var slotNumber = Interlocked.Increment(ref _opCount) % 2;
                switch (slotNumber)
                {
                    case 0:
                        if (Interlocked.CompareExchange(ref _slots0.stale, 1, 0) == 1)
                            continue;
                        _slots0.data = item;
                        break;
                    case 1:
                        if (Interlocked.CompareExchange(ref _slots1.stale, 1, 0) == 1)
                            continue;
                        _slots1.data = item;
                        break;
                }

                break;
            }
            Interlocked.Decrement(ref _concurrentThreadsCount);
        }

        public bool TryTake(out T result)
        {
            result = default;
            return true;
        }
    }
}
