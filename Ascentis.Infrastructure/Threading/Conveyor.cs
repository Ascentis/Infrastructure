using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class Conveyor<T>
    {
        private enum ThreadStatus {NotStarted, Started, Stopped}
        public delegate void ProcessPacketDelegate(T packet);
        private readonly ManualResetEventSlim _dataAvailable;
        private readonly ConcurrentQueue<T> _packetsQueue;
        private readonly ProcessPacketDelegate _processPacketDelegate;
        private volatile ThreadStatus _threadStatus;
        private Thread _workerThread;

        public Conveyor(ProcessPacketDelegate processPacketDelegate)
        {
            ArgsChecker.CheckForNull<NullReferenceException>(processPacketDelegate, nameof(processPacketDelegate));
            _dataAvailable = new ManualResetEventSlim(false);
            _packetsQueue = new ConcurrentQueue<T>();
            _processPacketDelegate = processPacketDelegate;
            _threadStatus = ThreadStatus.NotStarted;
        }

        public void Start()
        {
            _threadStatus = ThreadStatus.Started;
            _workerThread = new Thread(() =>
            {
                while (true)
                {
                    T packet;
                    while (true)
                    {
                        if (_packetsQueue.TryDequeue(out packet))
                            break;
                        _dataAvailable.Wait();
                        _dataAvailable.Reset();
                        if (_threadStatus == ThreadStatus.Stopped)
                            return;
                    }

                    _processPacketDelegate(packet);
                }
            });
            _workerThread.Start();
        }

        private void CheckThreadStarted()
        {
            if (_threadStatus == ThreadStatus.Stopped)
                throw new InvalidOperationException("Worker thread not started");
        }

        public void InsertPacket(T packet)
        {
            CheckThreadStarted();
            _packetsQueue.Enqueue(packet);
            _dataAvailable.Set();
        }

        public void Stop()
        {
            CheckThreadStarted();
            _threadStatus = ThreadStatus.Stopped;
            _dataAvailable.Set();
            _workerThread = null;
        }

        public void StopAndWait()
        {
            var workerThread = _workerThread;
            Stop();
            workerThread.Join();
        }
    }
}
