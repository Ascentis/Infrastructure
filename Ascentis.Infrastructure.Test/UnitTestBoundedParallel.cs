using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable AccessToDisposedClosure

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure.Test
{
    /// <summary>
    /// Summary description for BoundedParallel
    /// </summary>
    [TestClass]
    public class UnitTestBoundedParallel
    {
        [TestMethod]
        public void TestCreateBoundedParallel()
        {
            var boundedParallel = new BoundedParallel();
            Assert.IsNotNull(boundedParallel);
        }

        [TestMethod]
        public void TestBoundedParallelSimpleInvokeCall()
        {
            var cnt = 0;
            var boundedParallel = new BoundedParallel(3);
            boundedParallel.Invoke(() => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); },
                () => { Interlocked.Increment(ref cnt); });
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerial()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            });
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialWithThreadLimiter()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(-1, 2);
            Parallel.Invoke(() =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            });
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialWithThreadLimiterForcingAllowance()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            using var waitEvent1 = new ManualResetEvent(false);
            using var waitEvent2 = new ManualResetEvent(false);
            var boundedParallel = new BoundedParallel(-1, 4);
            Parallel.Invoke(() =>
            {
                boundedParallel.Invoke(() =>
                    {
                        waitEvent1.Set();
                        waitEvent2.WaitOne();
                        Thread.Sleep(1500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(() =>
                    {
                        waitEvent1.WaitOne();
                        waitEvent2.Set();
                        Thread.Sleep(1500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                waitEvent1.WaitOne();
                waitEvent2.WaitOne();
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(1000);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                waitEvent1.WaitOne();
                waitEvent2.WaitOne();
                boundedParallel.Invoke(() =>
                    {
                        Thread.Sleep(1000);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            });
            Assert.AreEqual(12, cnt);
            Assert.AreEqual(2, boundedParallel.TotalSerialRunCount);
            Assert.AreEqual(4, boundedParallel.TotalParallelsThreadConsumed);
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialWithThreadLimiterAndParallelOptions()
        {
            var cnt = 0;
            var parOptions = new ParallelOptions {MaxDegreeOfParallelism = 1};
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(-1, 4);
            Parallel.Invoke(() =>
            {
                boundedParallel.Invoke(parOptions, () =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(parOptions, () =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(parOptions, () =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            }, () =>
            {
                boundedParallel.Invoke(parOptions, () =>
                    {
                        Thread.Sleep(500);
                        Interlocked.Increment(ref cnt);
                    },
                    () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
            });
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount == 0, "TotalSerialRunCount should be equals to 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForEachCall()
        {
            var items = new[] {1, 2, 3};
            var cnt = 0;
            var boundedParallel = new BoundedParallel(3);
            var result = boundedParallel.ForEach(items, (item) => { Interlocked.Increment(ref cnt); });
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
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
            Assert.IsTrue(boundedParallel.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForCall()
        {
            var items = new[] {1, 2, 3};
            var sumItems = 0;
            var boundedParallel = new BoundedParallel(3);
            boundedParallel.For(0, 3, (idx) => { Interlocked.Add(ref sumItems, items[(int) idx]); });
            Assert.AreEqual(6, sumItems);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
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
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            });

            Assert.AreEqual(24, sumItems);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
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
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            });

            Assert.AreEqual(24, sumItems);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
            boundedParallel.ResetTotalSerialRunCount();

            sumItems = 0;
            Parallel.Invoke(() =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int) idx]);
                });
            });

            Assert.AreEqual(12, sumItems);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount == 0, "TotalSerialRunCount should be equals to 0");
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
            Assert.IsTrue(boundedParallel.TotalSerialRunCount == 0, "TotalSerialRunCount must be zero");
            Assert.AreEqual(2, e.InnerExceptions.Count);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void TestBoundedParallelInvokeForceSerialThrowsException()
        {
            var cnt = 0;
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2) {AbortInvocationsOnSerialInvocationException = false};
            Assert.ThrowsException<AggregateException>(() =>
                Parallel.Invoke(() =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }, () =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }, () =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                            throw new Exception();
                        },
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }, () =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }));
            Assert.AreEqual(12, cnt);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");

            using var startedThread1Event = new ManualResetEvent(false);
            using var startedThread2Event = new ManualResetEvent(false);
            boundedParallel.AbortInvocationsOnSerialInvocationException = true;
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
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }, () =>
                {
                    boundedParallel.Invoke(() =>
                        {
                            startedThread2Event.Set();
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }, () =>
                {
                    startedThread1Event.WaitOne();
                    startedThread2Event.WaitOne();
                    boundedParallel.Invoke(() =>
                        {
                            Interlocked.Increment(ref cnt);
                            throw new Exception();
                        },
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }, () =>
                {
                    startedThread1Event.WaitOne();
                    startedThread2Event.WaitOne();
                    boundedParallel.Invoke(() =>
                        {
                            Thread.Sleep(500);
                            Interlocked.Increment(ref cnt);
                        },
                        () => { Interlocked.Increment(ref cnt); }, () => { Interlocked.Increment(ref cnt); });
                }));
            Assert.AreEqual(10, cnt);
            Assert.IsTrue(boundedParallel.TotalSerialRunCount > 0, "TotalSerialRunCount should be > 0");
        }

        private delegate int GetAllowedThreadCountDelegate(int currentConcurrentThreadCount, int requestedThreadCount);
        [TestMethod]
        public void TestPrivateGetAllowedThreadCount()
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2) {AbortInvocationsOnSerialInvocationException = false};
            var methodInfo = boundedParallel.GetType()
                .GetMethod("GetAllowedThreadCount", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(methodInfo);
            var getAllowedThreadCount =
                (GetAllowedThreadCountDelegate) Delegate.CreateDelegate(typeof(GetAllowedThreadCountDelegate),
                    boundedParallel, methodInfo);

            boundedParallel.MaxParallelThreads = 3;
            Assert.AreEqual(1, getAllowedThreadCount(4, 2)); // Could fit one thread of the two requested
            Assert.AreEqual(2, getAllowedThreadCount(3, 2)); // Exact fit of the two requests
            Assert.AreEqual(2,
                getAllowedThreadCount(5, 2)); // Could not fit even 1 thread, returned requestedThreadCount
            boundedParallel.MaxParallelThreads = BoundedParallel.Unlimited;
            Assert.AreEqual(2, getAllowedThreadCount(8, 2)); // Could fit both thread. Using unlimited ThreadLimit
        }

        private delegate bool TryParallelDelegate(BoundedParallel.ParallelLoopDelegate bodyParallelCall, out ParallelLoopResult parallelLoopResult, int threadCount);
        [TestMethod]
        public void TestPrivateTryParallelDelegate()
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            var boundedParallel = new BoundedParallel(2) {AbortInvocationsOnSerialInvocationException = false};
            var methodInfo = boundedParallel.GetType().GetMethod("TryParallel",
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] {typeof(BoundedParallel.ParallelLoopDelegate), typeof(ParallelLoopResult).MakeByRefType(), typeof(int)}, null);
            Assert.IsNotNull(methodInfo);
            var tryParallel = (TryParallelDelegate) Delegate.CreateDelegate(typeof(TryParallelDelegate), boundedParallel, methodInfo);

            Assert.AreEqual(0, boundedParallel.ConcurrentInvocationsCount);
            Assert.AreEqual(0, boundedParallel.ConcurrentThreadsCount);
            var delegateCalled = false;
            var retVal = tryParallel(count =>
            {
                Assert.AreEqual(1, boundedParallel.ConcurrentInvocationsCount);
                Assert.AreEqual(2, boundedParallel.ConcurrentThreadsCount);
                return Parallel.For(0, 1, idx => delegateCalled = true);
            }, out var parallelLoopResult, 2);
            Assert.IsTrue(retVal);
            Assert.IsTrue(delegateCalled);
            Assert.IsTrue(parallelLoopResult.IsCompleted);
            Assert.IsNull(parallelLoopResult.LowestBreakIteration);
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
                    return new System.Threading.Tasks.ParallelLoopResult();
                }, out parallelLoopResult, 2);
                return new System.Threading.Tasks.ParallelLoopResult();
            }, out var parallelLoopResultOuter, 2);
            Assert.IsTrue(retValOuter);
            Assert.IsFalse(retVal);
            Assert.IsTrue(delegateCalled);
            Assert.IsFalse(innerDelegateCalled);
            Assert.IsFalse(parallelLoopResultOuter.IsCompleted);
            Assert.IsNull(parallelLoopResultOuter.LowestBreakIteration);
            Assert.IsFalse(parallelLoopResult.IsCompleted);
            Assert.IsNull(parallelLoopResult.LowestBreakIteration);
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
    }
}
