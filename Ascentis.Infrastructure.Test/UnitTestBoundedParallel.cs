using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            boundedParallel.Invoke(() =>
                {
                    Interlocked.Increment(ref cnt);
                }, () =>
                {
                    Interlocked.Increment(ref cnt);
                },
                () =>
                {
                    Interlocked.Increment(ref cnt);
                });
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.SerialRunCount == 0, "SerialRunCount must be zero");
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerial()
        {
            var cnt = 0;
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
            Assert.IsTrue(boundedParallel.SerialRunCount > 0, "SerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForEachCall()
        {
            var items = new[] {1, 2, 3};
            var cnt = 0;
            var boundedParallel = new BoundedParallel(3); 
            var result = boundedParallel.ForEach(items, (item) =>
            {
                Interlocked.Increment(ref cnt);
            });
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.SerialRunCount == 0, "SerialRunCount must be zero");
            Assert.IsTrue(result.IsCompleted);
        }

        [TestMethod]
        public void TestBoundedParallelForEachForceSerial()
        {
            var items = new[] {1, 2, 3};
            var cnt = 0;
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
            Assert.IsTrue(boundedParallel.SerialRunCount > 0, "SerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleForCall()
        {
            var items = new[] {1, 2, 3};
            var sumItems = 0;
            var boundedParallel = new BoundedParallel(3);
            boundedParallel.For(0, 3, (idx) =>
            {
                Interlocked.Add(ref sumItems, items[(int)idx]);
            });
            Assert.AreEqual(6, sumItems);
            Assert.IsTrue(boundedParallel.SerialRunCount == 0, "SerialRunCount must be zero");
        }

        [TestMethod]
        public void TestBoundedParallelForForceSerial()
        {
            var items = new[] {1, 2, 3};
            var sumItems = 0;
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            });

            Assert.AreEqual(24, sumItems);
            Assert.IsTrue(boundedParallel.SerialRunCount > 0, "SerialRunCount should be > 0");
        }

        [TestMethod]
        public void TestBoundedParallelForForceSerialThenParallelAgain()
        {
            var items = new[] {1, 2, 3};
            var sumItems = 0;
            var boundedParallel = new BoundedParallel(2);
            Parallel.Invoke(() =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            });

            Assert.AreEqual(24, sumItems);
            Assert.IsTrue(boundedParallel.SerialRunCount > 0, "SerialRunCount should be > 0");
            boundedParallel.ResetSerialRunCount();

            sumItems = 0;
            Parallel.Invoke(() =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            }, () =>
            {
                boundedParallel.For(0, 3, (idx) =>
                {
                    Thread.Sleep(500);
                    Interlocked.Add(ref sumItems, items[(int)idx]);
                });
            });

            Assert.AreEqual(12, sumItems);
            Assert.IsTrue(boundedParallel.SerialRunCount == 0, "SerialRunCount should be equals to 0");
        }

        [TestMethod]
        public void TestBoundedParallelSimpleInvokeCallThrowsException()
        {
            var cnt = 0;
            var boundedParallel = new BoundedParallel(3);
            Assert.ThrowsException<AggregateException>(() =>
            boundedParallel.Invoke(() =>
                {
                    Interlocked.Increment(ref cnt);
                    throw new Exception("Explosion");
                }, () =>
                {
                    Interlocked.Increment(ref cnt);
                },
                () =>
                {
                    Interlocked.Increment(ref cnt);
                }));
            Assert.AreEqual(3, cnt);
            Assert.IsTrue(boundedParallel.SerialRunCount == 0, "SerialRunCount must be zero");
        }

        [TestMethod]
        public void TestBoundedParallelInvokeForceSerialThrowsException()
        {
            var cnt = 0;
            var boundedParallel = new BoundedParallel(2) {AbortInvocationsOnSerialInvocationException = false};
            Assert.ThrowsException<AggregateException>(() =>
            Parallel.Invoke(() =>
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
            Assert.IsTrue(boundedParallel.SerialRunCount > 0, "SerialRunCount should be > 0");

            boundedParallel.AbortInvocationsOnSerialInvocationException = true;
            cnt = 0;
            Assert.ThrowsException<AggregateException>(() =>
            Parallel.Invoke(() =>
            {
                boundedParallel.Invoke(() =>
                    {
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
                Thread.Sleep(500);
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
            Assert.AreEqual(10, cnt);
            Assert.IsTrue(boundedParallel.SerialRunCount > 0, "SerialRunCount should be > 0");
        }
    }
}
