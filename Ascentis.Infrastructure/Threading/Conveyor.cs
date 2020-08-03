﻿using System;
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
        private Exception _exception;

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
                        T packet;
                        while (true)
                        {
                            if (_packetsQueue.TryDequeue(out packet))
                                break;
                            if (_threadStatus == ThreadStatus.Stopped)
                                return;
                            _dataAvailable.Wait();
                            _dataAvailable.Reset();
                        }

                        _processPacketDelegate(packet);
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
