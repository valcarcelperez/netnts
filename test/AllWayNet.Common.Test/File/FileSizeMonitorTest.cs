namespace AllWayNet.Common.Test.File
{
    using AllWayNet.Common.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    [TestClass]
    public class FileSizeMonitorTest
    {
        private FileSizeMonitor target = null;
        private string filename;
        private static string testDirectory;
        private const string testFileExtension = "test";
        private byte[] oneKByteOfData = new byte[1024];

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void MainInit(TestContext testContext)
        {
            testDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSizeMonitorTest");
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }

            Directory.CreateDirectory(testDirectory);
        }

        [TestInitialize]
        public void Init()
        {
            string name = string.Format("{0}.{1}", this.TestContext.TestName, testFileExtension);
            this.filename = Path.Combine(testDirectory, name);
            this.DeleteFile(this.filename);
        }

        [TestCleanup]
        public void Dispose()
        {
            if (this.target != null)
            {
                this.target.Dispose();
            }

            this.DeleteFile(this.filename);
        }

        [TestMethod]
        public void FileSizeMonitor_Constructor()
        {
            this.target = new FileSizeMonitor(100, this.filename, 100);
        }

        [TestMethod]
        public void FileSizeMonitor_MaxSize_Event()
        {
            int maxSizeCount = 0;
            EventHandler maxSizeHandler = delegate(object sender, EventArgs e)
            {
                Interlocked.Increment(ref maxSizeCount);
            };

            List<Exception> exceptionList = new List<Exception>();
            EventHandler<ErrorEventArgs> errorHandler = delegate(object sender, ErrorEventArgs e)
            {
                exceptionList.Add(e.GetException());
            };

            using(FileStream fs = new FileStream(this.filename, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                this.target = new FileSizeMonitor(100, this.filename, 2048);
                this.target.MaxSize += maxSizeHandler;
                this.target.Error += errorHandler;
                this.target.Start();

                Assert.AreEqual(0, exceptionList.Count);
                Assert.AreEqual(0, maxSizeCount);
                
                fs.Write(this.oneKByteOfData, 0, this.oneKByteOfData.Length);
                fs.Flush(true);
                Thread.Sleep(120);
                Assert.AreEqual(0, exceptionList.Count);
                Assert.AreEqual(0, maxSizeCount);

                fs.Write(this.oneKByteOfData, 0, this.oneKByteOfData.Length);
                fs.Flush(true);
                Thread.Sleep(100);
                Assert.AreEqual(0, exceptionList.Count);
                Assert.AreEqual(0, maxSizeCount);

                fs.Write(this.oneKByteOfData, 0, this.oneKByteOfData.Length);
                fs.Flush(true);
                Thread.Sleep(100);
                Assert.AreEqual(0, exceptionList.Count);
                Assert.AreEqual(1, maxSizeCount);
            }
        }

        [TestMethod]
        public void FileSizeMonitor_Error_Event()
        {
            List<Exception> exceptionList = new List<Exception>();
            EventHandler<ErrorEventArgs> errorHandler = delegate(object sender, ErrorEventArgs e)
            {
                exceptionList.Add(e.GetException());
            };

            this.target = new FileSizeMonitor(100, this.filename + ".invalid", 2048);
            this.target.Error += errorHandler;
            this.target.Start();

            Assert.AreEqual(0, exceptionList.Count);
            Thread.Sleep(120);
            Assert.AreEqual(1, exceptionList.Count);
            Thread.Sleep(100);
            Assert.AreEqual(2, exceptionList.Count);
        }

        [TestMethod]
        public void FileSizeMonitor_Dispose_Stops_The_Process()
        {
            int maxSizeCount = 0;
            EventHandler maxSizeHandler = delegate(object sender, EventArgs e)
            {
                Interlocked.Increment(ref maxSizeCount);
            };

            using (FileStream fs = new FileStream(this.filename, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                this.target = new FileSizeMonitor(100, this.filename, 2048);
                this.target.MaxSize += maxSizeHandler;
                this.target.Start();
                Assert.AreEqual(0, maxSizeCount);

                fs.Write(this.oneKByteOfData, 0, this.oneKByteOfData.Length);
                fs.Flush(true);
                Thread.Sleep(120);
                Assert.AreEqual(0, maxSizeCount);

                fs.Write(this.oneKByteOfData, 0, this.oneKByteOfData.Length);
                fs.Flush(true);
                Thread.Sleep(100);
                Assert.AreEqual(0, maxSizeCount);

                this.target.Dispose();

                fs.Write(this.oneKByteOfData, 0, this.oneKByteOfData.Length);
                fs.Flush(true);
                Thread.Sleep(100);
                Assert.AreEqual(0, maxSizeCount);
            }
        }

        private void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }
}
