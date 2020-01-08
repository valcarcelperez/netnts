namespace AllWayNet.Common.Test.Threading
{
    using AllWayNet.Common.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    [TestClass]
    public class TimerExTest
    {
        private TimerEx target;
        private int cancelableActionCount = 0;
        private int canceled = 0;

        [TestInitialize]
        public void Init()
        {
        }

        [TestCleanup]
        public void Dispose()
        {
            if (this.target != null)
            {
                this.target.Dispose();
            }
        }

        [TestMethod]
        public void TimerEx_Constructor_With_Action()
        {
            Action action = () => {};
            this.target = new TimerEx(100, action);
        }

        [TestMethod]
        public void TimerEx_Constructor_With_Cancelable_Action()
        {
            Action<CancellationToken> cancellableAction = (t) => {};
            this.target = new TimerEx(100, cancellableAction);
        }

        [TestMethod]
        public void TimerEx_Start()
        {
            int actionCount = 0;
            Action action = () =>
                {
                    Interlocked.Increment(ref actionCount);
                };

            this.target = new TimerEx(50, action);
            Assert.AreEqual(0, actionCount);
            this.target.Start();
            Thread.Sleep(250);
            Assert.IsTrue(actionCount > 0);
        }

        [TestMethod]
        public void TimerEx_Stop()
        {
            int actionCount = 0;
            Action action = () =>
            {
                Interlocked.Increment(ref actionCount);
            };

            this.target = new TimerEx(50, action);
            this.target.Start();
            Thread.Sleep(250);
            bool stoped = this.target.Stop();
            Assert.IsTrue(stoped);
            Assert.IsTrue(actionCount > 0);
            int afterStoped = actionCount;
            Thread.Sleep(250);
            Assert.AreEqual(afterStoped, actionCount);
        }

        [TestMethod]
        public void TimerEx_Stop_Before_Executing_The_First_Time()
        {
            int actionCount = 0;
            Action action = () =>
            {
                Interlocked.Increment(ref actionCount);
            };

            this.target = new TimerEx(2000, action);
            this.target.Start();
            Thread.Sleep(100);
            bool stoped = this.target.Stop();
            Assert.IsTrue(stoped);
            Assert.AreEqual(0, actionCount);
        }

        [TestMethod]
        public void TimerEx_Interval()
        {
            int actionCount = 0;
            bool firstTime = true;
            bool secondTime = false;
            Stopwatch sw = new Stopwatch();
            long elapsed = 0;
            Action action = () =>
            {
                if (firstTime)
                {
                    sw.Start();
                    firstTime = false;
                    secondTime = true;
                    return;
                }

                if (secondTime)
                {
                    elapsed = sw.ElapsedMilliseconds;
                    sw.Stop();
                    secondTime = false;
                }                
            };

            int interval = 250;
            this.target = new TimerEx(250, action);
            Assert.AreEqual(0, actionCount);
            this.target.Start();
            Thread.Sleep(600);

            Assert.IsFalse(firstTime);
            Assert.IsFalse(secondTime);
            Assert.IsTrue(this.AreComparable(interval, elapsed, .10), string.Format("elapsed : {0}", elapsed));
        }

        [TestMethod]
        public void TimerEx_Stop_Returns_False_When_Action_Does_Not_Stop_On_Time()
        {
            Action action = () =>
            {
                Thread.Sleep(2000);
            };

            this.target = new TimerEx(100, action);
            this.target.Start();
            Thread.Sleep(500);
            bool stoped = this.target.Stop(500);
            Assert.IsFalse(stoped);
        }

        [TestMethod]
        public void TimerEx_Reentrancy_Must_Be_Avoided()
        {
            int actionCount = 0;
            Action action = () =>
            {
                Interlocked.Increment(ref actionCount);
                Thread.Sleep(1000);
            };

            this.target = new TimerEx(50, action);
            this.target.Start();
            Thread.Sleep(800);
            this.target.Stop();
            Assert.AreEqual(1, actionCount);
            
        }

        [TestMethod]
        public void TimerEx_Cancellation_Signaled()
        {
            this.target = new TimerEx(100, this.CancelableAction);
            this.target.Start();
            Thread.Sleep(150);
            this.target.Stop();
            Thread.Sleep(150);
            Assert.AreEqual(1, this.cancelableActionCount);
            Assert.AreEqual(1, this.canceled);
        }

        [TestMethod]
        public void TimerEx_Error_Event()
        {
            Exception exception = new Exception("test exception");
            Action action = () =>
            {
                Thread.Sleep(5);
                throw exception;
            };

            List<Exception> exceptionList = new List<Exception>();
            EventHandler<ErrorEventArgs> errorHandler = delegate(object sender, ErrorEventArgs e)
            {
                exceptionList.Add(e.GetException());
            };

            this.target = new TimerEx(100, action);
            this.target.Error += errorHandler;
            this.target.Start();
            Thread.Sleep(250);
            this.target.Stop();
            Assert.AreEqual(2, exceptionList.Count);
        }

        [TestMethod]
        public void TimerEx_Dispose_Must_Stop_The_Timer()
        {
            object lockObject = new object();
            int actionCount = 0;
            Action action = () =>
            {
                lock (lockObject)
                {
                    actionCount++;
                }
            };

            this.target = new TimerEx(50, action);
            this.target.Start();
            Thread.Sleep(130);
            this.target.Dispose();
            this.target = null;
            Assert.IsTrue(actionCount > 0, "Action must be called when timer is running.");
            int expected;
            lock (lockObject)
            {
                expected = actionCount;
            }

            Thread.Sleep(130);
            lock (lockObject)
            {
                Assert.AreEqual(expected, actionCount, "Action must not be called after the timer is disposed.");
            }
        }

        private void CancelableAction(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref this.cancelableActionCount);
            int count = 0;

            while (count < 10)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Interlocked.Increment(ref this.canceled);
                    return;
                }

                Thread.Sleep(50);
                count++;
            }
        }

        private bool AreComparable(long a, long b, double tolerancePercent)
        {
            double bPercent = (double)b / a;
            return Math.Abs(1 - bPercent) <= tolerancePercent;
        }
    }
}
