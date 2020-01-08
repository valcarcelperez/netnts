namespace AllWayNet.Common.Test.Threading
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading.Tasks;
    using AllWayNet.Common.Threading;

    [TestClass]
    public class ConcurrentCounterTest
    {
        private ConcurrentCounter target;

        [TestMethod]
        public void ProtectedCounter_Constructor()
        {
            this.target = new ConcurrentCounter();
            Assert.IsNotNull(this.target);
            Assert.AreEqual(0, this.target.Value);
        }

        [TestMethod]
        public void ProtectedCounter_Zero()
        {
            this.target = ConcurrentCounter.Zero;
            Assert.IsNotNull(this.target);
            Assert.AreEqual(0, this.target.Value);
        }

        [TestMethod]
        public void ProtectedCounter_Inc()
        {
            this.target = new ConcurrentCounter();
            this.target.Inc();
            Assert.AreEqual(1, this.target.Value);
            this.target.Inc();
            Assert.AreEqual(2, this.target.Value);
            this.target.Inc(20);
            Assert.AreEqual(22, this.target.Value);
        }

        [TestMethod]
        public void ProtectedCounter_Inc_Is_Thread_Safe()
        {
            this.target = new ConcurrentCounter();

            Task[] taskList = new Task[10000];
            Action action = () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    this.target.Inc();
                }

                for (int i = 0; i < 10; i++)
                {
                    this.target.Inc(10);
                }
            };

            for(int i = 0; i < taskList.Length; i++)
            {
                taskList[i] = Task.Factory.StartNew(action);
            }

            Task.WaitAll(taskList);

            Assert.AreEqual(1100000, this.target.Value);
        }
    }
}
