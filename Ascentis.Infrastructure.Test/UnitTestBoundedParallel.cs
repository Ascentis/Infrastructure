using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable AccessToDisposedClosure
// ReSharper disable once CheckNamespace

namespace Ascentis.Infrastructure.Test
{
    [TestClass]
    public class UnitTestBoundedParallel
    {
        [TestMethod]
        public void TestCreateBoundedParallel()
        {
            var boundedParallel = new BoundedParallel();
            Assert.IsNotNull(boundedParallel);
        }

        // ReSharper disable once IdentifierTypo
        private static void BoundedParallelInvoke(BoundedParallel boundedParallel, int withSleep, ref int pcnt, ParallelOptions parOptions = null)
        {
            parOptions ??= new ParallelOptions();
            var cnt = 0;
            boundedParallel.Invoke(parOptions, () =>
            {
                if (withSleep > 0)
                    Thread.Sleep(withSleep);
                Interlocked.Increment(ref cnt);
            }, () =>
            {
                Interlocked.Increment(ref cnt);
            }, () =>
            {
                Interlocked.Increment(ref cnt);
            });
            Interlocked.Add(ref pcnt, cnt);
        }

        [TestMethod]
        public void TestBoundedParallelSimpleInvokeCall()
        {
            var cnt = 0;
            var boundedParallel = new BoundedParallel(3);
            BoundedParallelInvoke(boundedParallel, 0, ref cnt);
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
            Assert.IsTrue(boundedParallel.Stats.TotalParallelRunCount != 0, "TotalParallelRunCount must not be zero");
            boundedParallel.ResetAllStats();
            Assert.AreEqual(0, boundedParallel.Stats.TotalParallelRunCount);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        public void TestBoundedParallelArgumentsCheck()
        {
            var boundedParallel = new BoundedParallel();
            ParallelOptions nullParallelOptions = null;
            Assert.ThrowsException<ArgumentNullException>(() => boundedParallel.Invoke(nullParallelOptions, delegate { }));
            Assert.ThrowsException<ArgumentNullException>(() => boundedParallel.Invoke(new ParallelOptions(), null));
            Assert.ThrowsException<ArgumentException>(() => boundedParallel.Invoke(new ParallelOptions(), new Action[] {null}));
            Assert.ThrowsException<ArgumentNullException>(() => boundedParallel.For(0, 1, nullParallelOptions, delegate { }));
            Assert.ThrowsException<ArgumentNullException>(() => boundedParallel.For(0, 1, new ParallelOptions(), null));
            IEnumerable<int> nullEnumerable = null;
            Assert.ThrowsException<ArgumentNullException>(() => boundedParallel.ForEach(nullEnumerable, new ParallelOptions(), delegate { }));
            Assert.ThrowsException<ArgumentNullException>(() => boundedParallel.ForEach(new List<int>(), null, delegate { }));
            Assert.ThrowsException<ArgumentNullException>(() => boundedParallel.ForEach(new List<int>(), new ParallelOptions(), null));
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForEachWithZeroItems()
        {
            var regularParallelResult = Parallel.ForEach(new List<int>(), (n) => { });
            var boundedParallel = new BoundedParallel(3);
            var boundedResult = boundedParallel.ForEach(new List<int>(), (n) => { });
            Assert.AreEqual(1, boundedParallel.Stats.TotalSerialRunCount);
            Assert.AreEqual(0, boundedParallel.Stats.TotalParallelRunCount);
            Assert.AreEqual(regularParallelResult.IsCompleted, boundedResult.IsCompleted);
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForWithZeroItems()
        {
            var regularParallelResult = Parallel.For(0, 0, (n) => { });
            var boundedParallel = new BoundedParallel(3);
            var boundedResult = boundedParallel.For(0, 0, (n) => { });
            Assert.AreEqual(1, boundedParallel.Stats.TotalSerialRunCount);
            Assert.AreEqual(0, boundedParallel.Stats.TotalParallelRunCount);
            Assert.AreEqual(regularParallelResult.IsCompleted, boundedResult.IsCompleted);
        }

        [TestMethod]
        public void TestBoundedParallelSimpleInvokeWithNoActions()
        {
            var boundedParallel = new BoundedParallel(3);
            var actions = new Action[0];
            boundedParallel.Invoke(actions);
            Assert.AreEqual(1, boundedParallel.Stats.TotalSerialRunCount);
            Assert.AreEqual(0, boundedParallel.Stats.TotalParallelRunCount);
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerial()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            });
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialAndThenSomeParallel()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                BoundedParallelInvoke(boundedParallel, 1000, ref cnt); // Parallel
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 1000, ref cnt); // Parallel
            }, () =>
            {
                Thread.Sleep(500);
                BoundedParallelInvoke(boundedParallel, 2000, ref cnt); // One action serial and two in Parallel
            });
            Assert.AreEqual(9, cnt);
            Assert.AreEqual(1, boundedParallel.Stats.TotalSerialRunCount);
            Assert.AreEqual(3, boundedParallel.Stats.TotalParallelRunCount);
            Assert.AreEqual(8, boundedParallel.Stats.TotalParallelsThreadConsumed);
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialWithThreadLimiter()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(-1, 2);
            Parallel.Invoke(() =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt);
            });
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialWithThreadLimiterForcingAllowance()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            using var waitEvent1 = new ManualResetEvent(false);
            using var waitEvent2 = new ManualResetEvent(false);
            var boundedParallel = new BoundedParallel(-1, 5);
            Parallel.Invoke(() =>
            {
                boundedParallel.Invoke(() =>
                    {
                        waitEvent1.Set();
                        waitEvent2.WaitOne();
                        Thread.Sleep(1500);
                        Interlocked.Increment(ref cnt);
                    },
                    () =>
                    {
                        Interlocked.Increment(ref cnt);
                    }, () =>
                    {
                        Interlocked.Increment(ref cnt);
                    });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        waitEvent1.WaitOne();
                        waitEvent2.Set();
                        Thread.Sleep(1500);
                        Interlocked.Increment(ref cnt);
                    },
                    () =>
                    {
                        Interlocked.Increment(ref cnt);
                    }, () =>
                    {
                        Interlocked.Increment(ref cnt);
                    });
            }, () =>
            {
                waitEvent1.WaitOne();
                waitEvent2.WaitOne();
                BoundedParallelInvoke(boundedParallel, 1000, ref cnt);
            }, () =>
            {
                waitEvent1.WaitOne();
                waitEvent2.WaitOne();
                BoundedParallelInvoke(boundedParallel, 1000, ref cnt);
            });
            Assert.AreEqual(12, cnt);
            Assert.AreEqual(2, boundedParallel.Stats.TotalSerialRunCount);
            Assert.AreEqual(2, boundedParallel.Stats.TotalParallelRunCount);
            Assert.AreEqual(5, boundedParallel.Stats.TotalParallelsThreadConsumed);
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialWithThreadLimiterAndParallelOptions()
        {
            var cnt = 0;
            var parOptions = new ParallelOptions {MaxDegreeOfParallelism = 2};
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(-1, 4);
            Parallel.Invoke(() =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt, parOptions);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt, parOptions);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt, parOptions);
            }, () =>
            {
                BoundedParallelInvoke(boundedParallel, 500, ref cnt, parOptions);
            });
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalParallelRunCount > 0, "TotalParallelRunCount should be higher than 0");
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be higher than 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForEachCall()
        {
            var items = new[] {1, 2, 3};
            var cnt = 0;
            var boundedParallel = new BoundedParallel(3);
            var result = boundedParallel.ForEach(items, (item) => { Interlocked.Increment(ref cnt); });
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
            Assert.IsTrue(result.IsCompleted);
        }

        [TestMethod]
        public void TestBoundedParallelForEachForceSerial()
        {
            var items = new[] {1, 2, 3};
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                boundedParallel.ForEach(items, (item) =>
                {
                    Thread.Sleep(1500);
                    Interlocked.Increment(ref cnt);
                });
            }, () =>
            {
                boundedParallel.ForEach(items, (item) =>
                {
                    Thread.Sleep(1500);
                    Interlocked.Increment(ref cnt);
                });
            }, () =>
            {
                Thread.Sleep(500);
                var result1 = boundedParallel.ForEach(items, (item) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Increment(ref cnt);
                });
                Assert.IsTrue(result1.IsCompleted);
            }, () =>
            {
                Thread.Sleep(500);
                var result2 = boundedParallel.ForEach(items, (item) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Increment(ref cnt);
                });
                Assert.IsTrue(result2.IsCompleted);
            });

            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForCall()
        {
            var items = new[] {1, 2, 3};
            var sumItems = 0;
            var boundedParallel = new BoundedParallel(3);
            boundedParallel.For(0, 3, (idx) =>
            {
                Interlocked.Add(ref sumItems, items[idx]);
            });
            Assert.AreEqual(6, sumItems);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
        }

        [TestMethod]
        public void TestBoundedParallelForForceSerial()
        {
            var items = new[] {1, 2, 3};
            var sumItems = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            });

            Assert.AreEqual(24, sumItems);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void TestBoundedParallelForForceSerialThenParallelAgain()
        {
            var items = new[] {1, 2, 3};
            var sumItems = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            });

            Assert.AreEqual(24, sumItems);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
            boundedParallel.Stats.ResetTotalSerialRunCount();

            sumItems = 0;
            Parallel.Invoke(() =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[idx]);
                });
            });

            Assert.AreEqual(12, sumItems);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount == 0, "TotalSerialRunCount should be equals to 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleInvokeCallThrowsException()
        {
            var cnt = 0;
            var boundedParallel = new BoundedParallel(3);
            var e = Assert.ThrowsException<AggregateException>(() =>
                boundedParallel.Invoke(() =>
                    {
                        Interlocked.Increment(ref cnt);
                        throw new Exception("Explosion");
                    }, () =>
                    {
                        Interlocked.Increment(ref cnt);
                        throw new Exception("Explosion");
                    }, () =>
                    {
                        Interlocked.Increment(ref cnt);
                    }));
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
            Assert.AreEqual(2, e.InnerExceptions.Count);
        }

        [TestMethod]
        public void TestBoundedParallelStressTest1BoundedSucceedsContainingThreads()
        {
            Assert.IsTrue(ThreadPool.SetMinThreads(25, 10));
            var cnt = 0;
            var boundedParallel = new BoundedParallel(4, 5);
            var actions = new Action[20000];
            for (var i = 0; i < actions.Length; i++)
                actions[i] = () =>
                {
                    Interlocked.Increment(ref cnt);
                    for (var j = 0; j < 5000; j++)
                        Thread.Sleep(0);
                };
            boundedParallel.Invoke(actions);
            Assert.AreEqual(actions.Length, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount == 0, "TotalSerialRunCount must == 0");
            Assert.IsTrue(Process.GetCurrentProcess().Threads.Count < 50, "WorkerThreads should be < 50");
        }

        [TestMethod]
        public void TestBoundedParallelStressTest2UnboundedFailsContainingThreads()
        {
            Assert.IsTrue(ThreadPool.SetMinThreads(25, 10));
            var cnt = 0;
            var actions = new Action[20000];
            for (var i = 0; i < actions.Length; i++)
                actions[i] = () =>
                {
                    Interlocked.Increment(ref cnt);
                    for (var j = 0; j < 5000; j++)
                        Thread.Sleep(0);
                };
            Parallel.Invoke(actions);
            Assert.AreEqual(actions.Length, cnt);
            Assert.IsTrue(Process.GetCurrentProcess().Threads.Count > 40, "WorkerThreads should be > 40");
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void TestBoundedParallelInvokeForceSerialThrowsException()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2) {AbortOnSerialInvocationException = false};
            Assert.ThrowsException<AggregateException>(() =>
                Parallel.Invoke(() =>
                {
                    BoundedParallelInvoke(boundedParallel, 500, ref cnt);
                }, () =>
                {
                    BoundedParallelInvoke(boundedParallel, 500, ref cnt);
                }, () =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                            throw new Exception();
                        },
                        () =>
                        {
                            Interlocked.Increment(ref cnt);
                        }, () =>
                        {
                            Interlocked.Increment(ref cnt);
                        });
                }, () =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () =>
                        {
                            Interlocked.Increment(ref cnt);
                        }, () =>
                        {
                            Interlocked.Increment(ref cnt);
                        });
                }));
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");

            using var startedThread1Event = new ManualResetEvent(false);
            using var startedThread2Event = new ManualResetEvent(false);
            boundedParallel.AbortOnSerialInvocationException = true;
            cnt = 0;
            Assert.ThrowsException<AggregateException>(() =>
                Parallel.Invoke(() =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            startedThread1Event.Set();
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () =>
                        {
                            Interlocked.Increment(ref cnt);
                        }, () =>
                        {
                            Interlocked.Increment(ref cnt);
                        });
                }, () =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            startedThread2Event.Set();
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () =>
                        {
                            Interlocked.Increment(ref cnt);
                        }, () =>
                        {
                            Interlocked.Increment(ref cnt);
                        });
                }, () =>
                {
                    startedThread1Event.WaitOne();
                    startedThread2Event.WaitOne();
                    boundedParallel.Invoke(() =>
                        {
                            Interlocked.Increment(ref cnt);
                            throw new Exception();
                        },
                        () =>
                        {
                            Interlocked.Increment(ref cnt);
                        }, () =>
                        {
                            Interlocked.Increment(ref cnt);
                        });
                }, () =>
                {
                    startedThread1Event.WaitOne();
                    startedThread2Event.WaitOne();
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () =>
                        {
                            Interlocked.Increment(ref cnt);
                        }, () =>
                        {
                            Interlocked.Increment(ref cnt);
                        });
                }));
            Assert.AreEqual(10, cnt);
            Assert.IsTrue(boundedParallel.Stats.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        #region Private Method Tests

        private delegate int GetAllowedThreadCountDelegate(int currentConcurrentThreadCount, int requestedThreadCount);
        [TestMethod]
        public void TestPrivateGetAllowedThreadCount()
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2) {AbortOnSerialInvocationException = false};
            var methodInfo = boundedParallel.GetType()
                .GetMethod("GetAllowedThreadCount", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(methodInfo);
            var getAllowedThreadCount =
                (GetAllowedThreadCountDelegate) Delegate.CreateDelegate(typeof(GetAllowedThreadCountDelegate),
                    boundedParallel, methodInfo);

            boundedParallel.MaxParallelThreads = 3;
            Assert.AreEqual(2, getAllowedThreadCount(4, 2)); // Could fit one thread of the two requested, which makes no sense so returns requestedThreadCount
            Assert.AreEqual(2, getAllowedThreadCount(3, 2)); // Exact fit of the two requests
            Assert.AreEqual(2,
                getAllowedThreadCount(5, 2)); // Could not fit even 1 thread, returned requestedThreadCount
            boundedParallel.MaxParallelThreads = BoundedParallel.Unlimited;
            Assert.AreEqual(2, getAllowedThreadCount(8, 2)); // Could fit both thread. Using unlimited ThreadLimit
        }

        private delegate bool TryParallelDelegate(BoundedParallel.ParallelInvokeDelegate bodyParallelCall, int threadCount);
        [TestMethod]
        public void TestPrivateTryParallelDelegate()
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2) {AbortOnSerialInvocationException = false};
            var methodInfo = boundedParallel.GetType().GetMethod("TryParallel",
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] {typeof(BoundedParallel.ParallelInvokeDelegate), typeof(int)}, null);
            Assert.IsNotNull(methodInfo);
            var tryParallel = (TryParallelDelegate) Delegate.CreateDelegate(typeof(TryParallelDelegate), boundedParallel, methodInfo);

            Assert.AreEqual(0, boundedParallel.ConcurrentInvocationsCount);
            Assert.AreEqual(0, boundedParallel.ConcurrentThreadsCount);
            var delegateCalled = false;
            var retVal = tryParallel(count =>
            {
                Assert.AreEqual(1, boundedParallel.ConcurrentInvocationsCount);
                Assert.AreEqual(2, boundedParallel.ConcurrentThreadsCount);
                Parallel.For(0, 1, idx => delegateCalled = true);
            }, 2);
            Assert.IsTrue(retVal);
            Assert.IsTrue(delegateCalled);
            Assert.AreEqual(0, boundedParallel.ConcurrentInvocationsCount);
            Assert.AreEqual(0, boundedParallel.ConcurrentThreadsCount);

            boundedParallel.MaxParallelInvocations = 1;
            var innerDelegateCalled = false;
            var retValOuter = tryParallel(countOuter =>
            {
                delegateCalled = true;
                retVal = tryParallel(count =>
                {
                    innerDelegateCalled = true;
                }, 2);
            }, 2);
            Assert.IsTrue(retValOuter);
            Assert.IsFalse(retVal);
            Assert.IsTrue(delegateCalled);
            Assert.IsFalse(innerDelegateCalled);
            Assert.AreEqual(0, boundedParallel.ConcurrentInvocationsCount);
            Assert.AreEqual(0, boundedParallel.ConcurrentThreadsCount);
        }

        private delegate int MaxDegreeOfParallelismDelegate(ParallelOptions parallelOptions, int itemCount);
        [TestMethod]
        public void TestPrivateMaxDegreeOfParallelism()
        {
            var methodInfo = typeof(BoundedParallel).GetMethod("MaxDegreeOfParallelism", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(methodInfo);
            var maxDegreeOfParallelism = (MaxDegreeOfParallelismDelegate) Delegate.CreateDelegate(typeof(MaxDegreeOfParallelismDelegate), methodInfo);
            Assert.AreEqual(2, maxDegreeOfParallelism(new ParallelOptions() {MaxDegreeOfParallelism = BoundedParallel.Unlimited}, 2));
            Assert.AreEqual(1, maxDegreeOfParallelism(new ParallelOptions() {MaxDegreeOfParallelism = 1}, 2));
            Assert.AreEqual(2, maxDegreeOfParallelism(new ParallelOptions() {MaxDegreeOfParallelism = 3}, 2));
        }

        #endregion
    }
}
