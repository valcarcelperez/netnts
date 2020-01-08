namespace AllWayNet.Common.Test.File
{
    using AllWayNet.Common.File;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    [TestClass]
    public class ExpiredFileDeleterTest
    {
        private static string testDirectory;
        private string testSubDirectory;
        private ExpiredFileDeleter target = null;
        private const string searchPattern = "*.log";
        private const string data = "some text";
        private string file1 = "file1.log";
        private string file2 = "file2.log";

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void MainInit(TestContext testContext)
        {
            testDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExpiredFileDeleterTest");
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }

            Directory.CreateDirectory(testDirectory);
        }

        [TestInitialize]
        public void Init()
        {
            this.testSubDirectory = Path.Combine(testDirectory, this.TestContext.TestName);
            Directory.CreateDirectory(this.testSubDirectory);

            this.file1 = Path.Combine(this.testSubDirectory, "file1.log");
            this.file2 = Path.Combine(this.testSubDirectory, "file2.log");
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
        public void ExpiredFileDeleter_Constructor()
        {
            TimeSpan interval = new TimeSpan(0, 0, 0, 0, 250);
            TimeSpan maxAge = new TimeSpan(0, 0, 0, 0, 500);

            this.target = new ExpiredFileDeleter(interval, maxAge, this.testSubDirectory, searchPattern);
            Assert.AreEqual(interval, this.target.Interval);
            Assert.AreEqual(this.testSubDirectory, this.target.Directory);
            Assert.AreEqual(searchPattern, this.target.SearchPattern);
            Assert.AreEqual(maxAge, this.target.MaxAge);
        }

        [TestMethod]
        public void ExpiredFileDeleter_Start()
        {
            // 0 seconds
            this.CreateFilesAndStart();
            string[] currentFiles;

            Thread.Sleep(1000);
            // 11 secs. file1 is 11 secs old. file2 is 1 sec old.
            currentFiles = Directory.GetFiles(this.testSubDirectory).OrderBy(a => a).ToArray();
            Assert.IsFalse(currentFiles.Any(a => a == this.file1), "file1 must be deleted");
            Assert.IsTrue(currentFiles.Any(a => a == this.file2), "file2 must be in the directory");

            Thread.Sleep(1000);
            // 12 secs. file2 is 2 secs old.
            currentFiles = Directory.GetFiles(this.testSubDirectory).OrderBy(a => a).ToArray();
            Assert.IsTrue(currentFiles.Any(a => a == this.file2), "file2 must be in the directory");

            Thread.Sleep(3000);
            // 15 secs. file2 is 5 secs old.
            currentFiles = Directory.GetFiles(this.testSubDirectory).OrderBy(a => a).ToArray();
            Assert.IsFalse(currentFiles.Any(a => a == this.file2), "file2 must be deleted");
        }

        [TestMethod]
        public void ExpiredFileDeleter_Stop()
        {
            // 0 seconds
            this.CreateFilesAndStart();

            string[] currentFiles;

            Thread.Sleep(1000);
            // 11 secs. file1 is 11 secs old. file2 is 1 sec old.
            currentFiles = Directory.GetFiles(this.testSubDirectory).OrderBy(a => a).ToArray();
            Assert.IsFalse(currentFiles.Any(a => a == this.file1), "file1 must be deleted");
            Assert.IsTrue(currentFiles.Any(a => a == this.file2), "file2 must be in the directory");

            this.target.Stop();

            Thread.Sleep(4000);
            // 15 secs. file2 is 5 secs old.
            currentFiles = Directory.GetFiles(this.testSubDirectory).OrderBy(a => a).ToArray();
            Assert.IsTrue(currentFiles.Any(a => a == this.file2), "file2 must be in the ditrectory");
        }

        [TestMethod]
        public void ExpiredFileDeleter_Error_Event()
        {
            List<Exception> exceptionList = new List<Exception>();
            EventHandler<ErrorEventArgs> errorHandler = delegate(object sender, ErrorEventArgs e)
            {
                exceptionList.Add(e.GetException());
            };

            TimeSpan interval = new TimeSpan(0, 0, 0, 0, 100);
            TimeSpan maxAge = new TimeSpan(0, 0, 0, 0, 1000);
            this.target = new ExpiredFileDeleter(interval, maxAge, this.testSubDirectory + "invalid", searchPattern);
            this.target.Error += errorHandler;
            this.target.Start();

            Assert.AreEqual(0, exceptionList.Count);
            Thread.Sleep(120);
            Assert.AreEqual(1, exceptionList.Count);
            Thread.Sleep(100);
            Assert.AreEqual(2, exceptionList.Count);
        }

        [TestMethod]
        public void ExpiredFileDeleter_Dispose_Stops_The_Process()
        {
            // 0 seconds
            this.CreateFilesAndStart();

            string[] currentFiles;

            Thread.Sleep(1000);
            // 11 secs. file1 is 11 secs old. file2 is 1 sec old.
            currentFiles = Directory.GetFiles(this.testSubDirectory).OrderBy(a => a).ToArray();
            Assert.IsFalse(currentFiles.Any(a => a == this.file1), "file1 must be deleted");
            Assert.IsTrue(currentFiles.Any(a => a == this.file2), "file2 must be in the directory");

            this.target.Dispose();

            Thread.Sleep(4000);
            // 15 secs. file2 is 5 secs old.
            currentFiles = Directory.GetFiles(this.testSubDirectory).OrderBy(a => a).ToArray();
            Assert.IsTrue(currentFiles.Any(a => a == this.file2), "file2 must be in the directory");
        }

        private void CreateFile(string filename)
        {
            File.WriteAllText(filename, "some text");
        }

        private void CreateFilesAndStart()
        {
            File.WriteAllText(this.file1, data);
            File.SetLastWriteTimeUtc(this.file1, DateTime.UtcNow);

            Thread.Sleep(10000);
            File.WriteAllText(this.file2, data);
            File.SetLastWriteTimeUtc(this.file2, DateTime.UtcNow);

            Assert.AreEqual(2, Directory.GetFiles(this.testSubDirectory).Length);

            TimeSpan interval = new TimeSpan(0, 0, 0, 0, 500);
            TimeSpan maxAge = new TimeSpan(0, 0, 0, 4);
            this.target = new ExpiredFileDeleter(interval, maxAge, this.testSubDirectory, searchPattern);
            this.target.Start();
        }
    }
}
