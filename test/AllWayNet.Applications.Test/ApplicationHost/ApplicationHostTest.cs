namespace AllWayNet.Applications.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using AllWayNet.Logger;
    using System.Threading;

    [TestClass]
    public class ApplicationHostTest
    {
        private object lockObject = new object();

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(lockObject);
            MockHostableProcess.Exception = null;
        }

        [TestCleanup]
        public void End()
        {
            Monitor.Exit(lockObject);
        }

        [TestMethod]
        public void ApplicationHost_Constructor()
        {
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void ApplicationHost_Constructor_IsDesktopApp()
        {
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            Assert.AreEqual(Environment.UserInteractive, target.IsDesktopApp);
        }

        [TestMethod]
        public void ApplicationHost_Constructor_AutoStart_Must_Be_False()
        {
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            Assert.IsFalse(ApplicationHost.AutoStart);
        }

        [TestMethod]
        public void ApplicationHost_Constructor_AutoStart_Must_Be_True_When_Specified_In_Args()
        {
            string[] args = { "aaa", "-autostart" };
            ApplicationHost target = new ApplicationHost(args, null);
            Assert.IsTrue(ApplicationHost.AutoStart);
        }

        [TestMethod]
        public void ApplicationHost_Constructor_AutoStart_Is_Not_Change_From_True()
        {
            string[] args = { "aaa" };
            ApplicationHost.AutoStart = true;
            ApplicationHost target = new ApplicationHost(args, null);
            Assert.IsTrue(ApplicationHost.AutoStart);
        }

        [TestMethod]
        public void ApplicationHost_Constructor_AutoStart_Is_Not_Change_From_True_Passing_Arg()
        {
            string[] args = { "aaa", "-autostart" };
            ApplicationHost.AutoStart = true;
            ApplicationHost target = new ApplicationHost(args, null);
            Assert.IsTrue(ApplicationHost.AutoStart);
        }

        [TestMethod]
        public void ApplicationHost_Run()
        {
            MockHostableProcess.Exception = null;
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            MockHostableProcess hostableProcess = new MockHostableProcess();
            target.Run(hostableProcess);
        }

        [TestMethod]
        public void ApplicationHost_Run_Generic_Exception_Instantiating_HostableProcess()
        {
            MockHostableProcess.Exception = new Exception("test-exception");
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            try
            {
                target.Run<MockHostableProcess>();
            }
            catch (Exception ex)
            {
                Assert.AreSame(MockHostableProcess.Exception, ex.InnerException.InnerException);
                return;
            }

            Assert.Fail("An exception was not raised.");
        }

        [TestMethod]
        public void ApplicationHost_Run_Initializing_Must_Be_Called()
        {
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            bool called = false;
            target.Initializing = () => { called = true; };
            target.Run<MockHostableProcess>();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void ApplicationHost_Run_Initializing_Exception()
        {
            Exception exception = new Exception("test-exception");
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            target.Initializing = () => { throw exception; };

            try
            {
                target.Run<MockHostableProcess>();
            }
            catch (Exception ex)
            {
                Assert.AreSame(MockHostableProcess.Exception, ex.InnerException.InnerException);
                return;
            }

            Assert.Fail("An exception was not raised.");
        }

        [TestMethod]
        public void ApplicationHost_Run_RunHost_Must_Be_Called()
        {
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            bool called = false;
            target.RunHost = (hp) => { called = true; };
            target.Run<MockHostableProcess>();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void ApplicationHost_Run_HostableProcess_Must_Be_Passed_To_RunHost()
        {
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            HostableProcess passedHostableProcess = null;
            target.RunHost = (hp) => { passedHostableProcess = hp; };
            MockHostableProcess hostableProcess = new MockHostableProcess();
            target.Run(hostableProcess);
            Assert.AreSame(hostableProcess, passedHostableProcess);
        }

        [TestMethod]
        public void ApplicationHost_Run_RunHost_Exception()
        {
            Exception exception = new Exception("test-exception");
            string[] args = { };
            ApplicationHost target = new ApplicationHost(args, null);
            target.RunHost = (hp) => { throw exception; };

            try
            {
                target.Run<MockHostableProcess>();
            }
            catch (Exception ex)
            {
                Assert.AreSame(MockHostableProcess.Exception, ex.InnerException.InnerException);
                return;
            }

            Assert.Fail("An exception was not raised.");
        }

        public class MockHostableProcess : HostableProcess
        {
            public static Exception Exception { get; set; }

            public MockHostableProcess()
            {
                if (Exception != null)
                {
                    throw Exception;
                }
            }

            public override void Init()
            {
            }
        }
    }
}
