namespace AllWayNet.Logger.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Xml.Linq;

    public class LoggerImplementerBase
    {
        public event EventHandler<ErrorEventArgs> Error;
        public string Name { get; protected set; }

        public void RaiseError(ErrorEventArgs e)
        {
            Error(this, e);
        }
    }

    public class LoggerImplementer01 : LoggerImplementerBase, ILoggerProcessor
    {
        public static IList<LogItem> Logs = new List<LogItem>();
        public static int SetConfigCount;
        public static int DisposeCount;
        public static string XmlConfig;

        public void Log(LogItem log)
        {
            Logs.Add(log);
        }

        public void Prepare(XElement xml)
        {
            SetConfigCount++;
            XmlConfig = xml.ToString();
        }

        public void Dispose()
        {
            DisposeCount++;
        }
    }

    public class LoggerImplementer02 : LoggerImplementerBase, ILoggerProcessor
    {
        public static IList<LogItem> Logs = new List<LogItem>();
        public static int SetConfigCount;
        public static int DisposeCount;
        public static string XmlConfig;

        public void Log(LogItem log)
        {
            Logs.Add(log);
        }

        public void Prepare(System.Xml.Linq.XElement xml)
        {
            SetConfigCount++;
            XmlConfig = xml.ToString();
        }

        public void Dispose()
        {
            DisposeCount++;
        }
    }

    [TestClass]
    public class ApplicationLoggerTest
    {
        private string descriptionWithoutParameters = "a description";
        private MockLoggerProcessor loggerProcessor;
        private static LogItemTest.MockCustomLogItem customLogItem = new LogItemTest.MockCustomLogItem();
        private static Exception exception = new Exception("an exception");

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.loggerProcessor = new MockLoggerProcessor(string.Format("Log Processor - {0}", this.TestContext.TestName));
            ApplicationLogger.Initialize(false, null, null);
            ApplicationLogger.AddLoggerProcessor(this.loggerProcessor);
        }

        [TestCleanup]
        public void Dispose()
        {
            ApplicationLogger.RemoveLoggerProcessor(this.loggerProcessor);
        }

        [TestMethod]
        public void ApplicationLogger_Log()
        {
            EventLogEntryType type = EventLogEntryType.SuccessAudit;
            ApplicationLogger.Log(type, this.descriptionWithoutParameters);
            this.CompareResults(type);
        }

        [TestMethod]
        public void ApplicationLogger_ScheduleLog()
        {
            EventLogEntryType type = EventLogEntryType.SuccessAudit;
            ApplicationLogger.ScheduleLog(type, this.descriptionWithoutParameters);
            Thread.Sleep(500);

            this.CompareResults(type);
        }

        [TestMethod]
        public void ApplicationLogger_Log_With_CustomLogItem()
        {
            EventLogEntryType type = EventLogEntryType.SuccessAudit;
            ApplicationLogger.Log(type, customLogItem, this.descriptionWithoutParameters);
            this.CompareResults(type, customLogItem);
        }

        [TestMethod]
        public void ApplicationLogger_ScheduleLog_With_CustomLogItem()
        {
            EventLogEntryType type = EventLogEntryType.SuccessAudit;
            ApplicationLogger.ScheduleLog(type, customLogItem, this.descriptionWithoutParameters);
            Thread.Sleep(500);

            this.CompareResults(type, customLogItem);
        }

        [TestMethod]
        public void ApplicationLogger_LogInfo()
        {
            ApplicationLogger.LogInfo(this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Information);
        }

        [TestMethod]
        public void ApplicationLogger_LogInfo_With_CustomLogItem()
        {
            ApplicationLogger.LogInfo(customLogItem, this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Information, customLogItem);
        }

        [TestMethod]
        public void ApplicationLogger_LogWarning()
        {
            ApplicationLogger.LogWarning(this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Warning);
        }

        [TestMethod]
        public void ApplicationLogger_LogWarning_With_CustomLogItem()
        {
            ApplicationLogger.LogWarning(customLogItem, this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Warning, customLogItem);
        }

        [TestMethod]
        public void ApplicationLogger_LogWarning_With_Exception()
        {
            ApplicationLogger.LogWarning(exception, this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Warning, exception);
        }

        [TestMethod]
        public void ApplicationLogger_LogError()
        {
            ApplicationLogger.LogError(this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Error);
        }

        [TestMethod]
        public void ApplicationLogger_LogError_With_CustomLogItem()
        {
            ApplicationLogger.LogError(customLogItem, this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Error, customLogItem);
        }

        [TestMethod]
        public void ApplicationLogger_LogError_With_Exception()
        {
            ApplicationLogger.LogError(exception, this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.Error, exception);
        }

        [TestMethod]
        public void ApplicationLogger_LogFailureAudit()
        {
            ApplicationLogger.LogFailureAudit(this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.FailureAudit);
        }

        [TestMethod]
        public void ApplicationLogger_LogFailureAudit_With_CustomLogItem()
        {
            ApplicationLogger.LogFailureAudit(customLogItem, this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.FailureAudit, customLogItem);
        }

        [TestMethod]
        public void ApplicationLogger_LogSuccessAudit()
        {
            ApplicationLogger.LogSuccessAudit(this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.SuccessAudit);
        }

        [TestMethod]
        public void ApplicationLogger_LogSuccessAudit_With_CustomLogItem()
        {
            ApplicationLogger.LogSuccessAudit(customLogItem, this.descriptionWithoutParameters);
            this.CompareResults(EventLogEntryType.SuccessAudit, customLogItem);
        }

        [TestMethod]
        public void ApplicationLogger_AddLoggersProcessorsFromConfig_And_Dispose()
        {
            ApplicationLogger.AddLoggerProcessorsFromConfig();

            string message = "text";
            string xmlConfig01 = @"<implementer name=""loggerA"" type=""AllWayNet.Logger.Test.LoggerImplementer01, AllWayNet.Logger.Test"" />";
            string xmlConfig02 = @"<implementer name=""loggerB"" type=""AllWayNet.Logger.Test.LoggerImplementer02, AllWayNet.Logger.Test"" />";
            Assert.AreEqual(xmlConfig01, LoggerImplementer01.XmlConfig);
            Assert.AreEqual(0, LoggerImplementer01.DisposeCount);
            Assert.AreEqual(xmlConfig02, LoggerImplementer02.XmlConfig);
            Assert.AreEqual(0, LoggerImplementer02.DisposeCount);

            ApplicationLogger.LogError(message);
            Assert.AreEqual(1, LoggerImplementer01.Logs.Count);
            Assert.AreEqual(message, LoggerImplementer01.Logs[0].Description);
            Assert.AreEqual(1, LoggerImplementer02.Logs.Count);
            Assert.AreEqual(message, LoggerImplementer02.Logs[0].Description);

            ApplicationLogger.LogError(message);
            Assert.AreEqual(2, LoggerImplementer01.Logs.Count);
            Assert.AreEqual(2, LoggerImplementer02.Logs.Count);

            ApplicationLogger.DisposeLoggerProcessors();
            Assert.AreEqual(1, LoggerImplementer01.DisposeCount);
            Assert.AreEqual(1, LoggerImplementer02.DisposeCount);

            ApplicationLogger.LogError(message);
            Assert.AreEqual(2, LoggerImplementer01.Logs.Count);
            Assert.AreEqual(2, LoggerImplementer02.Logs.Count);
        }

        private void CompareResults(EventLogEntryType expectedType, ICustomLogItem customLogItem = null)
        {
            Assert.AreEqual(1, this.loggerProcessor.LogItems.Count);
            Assert.AreEqual(expectedType, this.loggerProcessor.LogItems[0].LogType);
            Assert.AreEqual(this.descriptionWithoutParameters, this.loggerProcessor.LogItems[0].Description);
            Assert.AreEqual(customLogItem, this.loggerProcessor.LogItems[0].CustomLogItem);
        }

        private void CompareResults(EventLogEntryType expectedType, Exception exception)
        {
            Assert.AreEqual(1, this.loggerProcessor.LogItems.Count);
            Assert.AreEqual(expectedType, this.loggerProcessor.LogItems[0].LogType);
            StringAssert.Contains(this.loggerProcessor.LogItems[0].Description, exception.Message);
        }
    }
}