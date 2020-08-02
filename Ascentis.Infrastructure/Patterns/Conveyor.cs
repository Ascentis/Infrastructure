using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class Conveyor<T>
    {
        public delegate void ProcessPacketDelegate(T packet);
        private readonly ManualResetEventSlim _dataAvailable;
        private readonly ConcurrentQueue<T> _packetsQueue;
        private readonly ProcessPacketDelegate _processPacketDelegate;
        private volatile bool _stopped;

        public Conveyor(ProcessPacketDelegate processPacketDelegate)
        {
            ArgsChecker.CheckForNull<NullReferenceException>(processPacketDelegate, nameof(processPacketDelegate));
            _dataAvailable = new ManualResetEventSlim(false);
            _packetsQueue = new ConcurrentQueue<T>();
            _processPacketDelegate = processPacketDelegate;
        }

        public void Start()
        {
            _stopped = false;
            var writerThread = new Thread(() =>
            {
                while (true)
                {
                    T packet;
                    do
                    {
                        if (_packetsQueue.TryDequeue(out packet))
                            break;
                        _dataAvailable.Wait();
                        _dataAvailable.Reset();
                    } while (true);

                    if (_stopped)
                        break;

                    _processPacketDelegate(packet);
                }
            });
            writerThread.Start();
        }

        public void InsertPacket(T packet)
        {
            _packetsQueue.Enqueue(packet);
            _dataAvailable.Set();
        }

        public void Stop()
        {
            _stopped = true;
            _dataAvailable.Set();
        }
    }
}
