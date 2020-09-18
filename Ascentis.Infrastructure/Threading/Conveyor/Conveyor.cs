using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class Conveyor<T>
    {
        private enum ThreadStatus {NotStarted, Started, Stopped}
        public delegate void ProcessPacketDelegate(T packet, object context);
        private readonly ManualResetEventSlim _dataAvailable;
        private readonly ConcurrentQueue<T> _packetsQueue;
        private readonly ProcessPacketDelegate _processPacketDelegate;
        private volatile ThreadStatus _threadStatus;
        private Thread _workerThread;
        private Exception _exception;
        private readonly object _context;

        public Conveyor(ProcessPacketDelegate processPacketDelegate, object context = null)
        {
            _dataAvailable = new ManualResetEventSlim(false);
            _packetsQueue = new ConcurrentQueue<T>();
            _processPacketDelegate = processPacketDelegate ?? throw new ArgumentNullException(nameof(processPacketDelegate));
            _threadStatus = ThreadStatus.NotStarted;
            _context = context;
        }

        public void Start()
        {
            if (_threadStatus == ThreadStatus.Started)
                throw new InvalidOperationException("Worker thread already started");
            _exception = null;
            _threadStatus = ThreadStatus.Started;
            _workerThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        if (_packetsQueue.TryDequeue(out var packet))
                        {
                            _processPacketDelegate(packet, _context);
                            continue;
                        }

                        if (_threadStatus == ThreadStatus.Stopped)
                            return;

                        _dataAvailable.Wait();
                        _dataAvailable.Reset();
                    }
                }
                catch (Exception e)
                {
                    _exception = e;
                }
            });
            _workerThread.Start();
        }

        private void CheckExceptionState()
        {
            if (_exception != null)
                throw new ConveyorException(_exception);
        }

        private void CheckThreadState()
        {
            if (_threadStatus != ThreadStatus.Started)
                throw new InvalidOperationException("Worker thread not started");
            CheckExceptionState();
        }

        public void InsertPacket(T packet)
        {
            CheckThreadState();
            _packetsQueue.Enqueue(packet);
            _dataAvailable.Set();
        }

        public void Stop()
        {
            CheckThreadState();
            _threadStatus = ThreadStatus.Stopped;
            _dataAvailable.Set();
            _workerThread = null;
        }

        public void StopAndWait()
        {
            var workerThread = _workerThread;
            Stop();
            workerThread.Join();
            CheckExceptionState();
        }
    }
}
