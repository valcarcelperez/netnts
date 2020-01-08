namespace AllWayNet.Logger.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Linq;

    [TestClass]
    public class LoggerTest
    {
        public class LogCounters
        {
            public long ErrorCount { get; set; }
            public long FailureAuditCount { get; set; }
            public long InformationCount { get; set; }
            public long SuccessAuditCount { get; set; }
            public long WarningCount { get; set; }
        }

        private Logger target;

        private static LogItemTest.MockCustomLogItem customLogItem = new LogItemTest.MockCustomLogItem();

        private MockLoggerProcessor loggerProcessor1;
        private MockLoggerProcessor loggerProcessor2;
        private LogItem logItem1 = new LogItem(LoggerTest.logType1, LoggerTest.logDescription1, customLogItem);
        private LogItem logItem2 = new LogItem(LoggerTest.logType2, LoggerTest.logDescription2);
        private static EventLogEntryType logType1 = EventLogEntryType.Information;
        private static EventLogEntryType logType2 = EventLogEntryType.Error;
        private static string logDescription1 = "log item 1";
        private static string logDescription2 = "log item 2";
        private List<Exception> loggerExceptions;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.loggerProcessor1 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));
            this.loggerProcessor2 = new MockLoggerProcessor(string.Format("Log Processor2 - {0}", this.TestContext.TestName));
            this.loggerExceptions = new List<Exception>();

            this.target = new Logger(this.TestContext.TestName);
            this.target.AddLoggerProcessor(this.loggerProcessor1);
            this.target.AddLoggerProcessor(this.loggerProcessor2);
            this.target.LoggerError += target_LoggerError;
        }

        [TestMethod]
        public void Logger_Constructor()
        {
            string loggerName = this.TestContext.TestName;
            Logger target = new Logger(loggerName);
            Assert.AreEqual(loggerName, target.Name);
        }

        [TestMethod]
        public void Logger_Log_Passing_LogItem()
        {
            this.target.Log(this.logItem1);
            this.target.Log(this.logItem2);

            Assert.AreEqual(2, this.loggerProcessor1.LogItems.Count);
            Assert.AreEqual(2, this.loggerProcessor2.LogItems.Count);

            Assert.AreEqual(this.logItem1, this.loggerProcessor1.LogItems[0]);
            Assert.AreEqual(this.logItem2, this.loggerProcessor1.LogItems[1]);
        }

        [TestMethod]
        public void Logger_Log()
        {
            this.target.Log(LoggerTest.logType1, LoggerTest.logDescription1, LoggerTest.customLogItem);
            this.target.Log(LoggerTest.logType2, LoggerTest.logDescription2);

            Assert.AreEqual(2, this.loggerProcessor1.LogItems.Count);
            Assert.AreEqual(2, this.loggerProcessor2.LogItems.Count);

            Assert.AreEqual(LoggerTest.logType1, this.loggerProcessor1.LogItems[0].LogType);
            Assert.AreEqual(LoggerTest.logDescription1, this.loggerProcessor1.LogItems[0].Description);
            Assert.AreEqual(LoggerTest.customLogItem, this.loggerProcessor1.LogItems[0].CustomLogItem);

            Assert.AreEqual(LoggerTest.logType2, this.loggerProcessor1.LogItems[1].LogType);
            Assert.AreEqual(LoggerTest.logDescription2, this.loggerProcessor1.LogItems[1].Description);
        }

        [TestMethod]
        public void Logger_Log_Error()
        {
            this.target.Error(LoggerTest.logDescription1, LoggerTest.customLogItem);
            this.target.Error(LoggerTest.logDescription2);
            this.CompareResults(EventLogEntryType.Error);
        }

        [TestMethod]
        public void Logger_Log_Info()
        {
            this.target.Info(LoggerTest.logDescription1, LoggerTest.customLogItem);
            this.target.Info(LoggerTest.logDescription2);
            this.CompareResults(EventLogEntryType.Information);
        }

        [TestMethod]
        public void Logger_Log_FailureAudit()
        {
            this.target.FailureAudit(LoggerTest.logDescription1, LoggerTest.customLogItem);
            this.target.FailureAudit(LoggerTest.logDescription2);
            this.CompareResults(EventLogEntryType.FailureAudit);
        }

        [TestMethod]
        public void Logger_Log_SuccessAudit()
        {
            this.target.SuccessAudit(LoggerTest.logDescription1, LoggerTest.customLogItem);
            this.target.SuccessAudit(LoggerTest.logDescription2);
            this.CompareResults(EventLogEntryType.SuccessAudit);
        }

        [TestMethod]
        public void Logger_Log_Warning()
        {
            this.target.Warning(LoggerTest.logDescription1, LoggerTest.customLogItem);
            this.target.Warning(LoggerTest.logDescription2);
            this.CompareResults(EventLogEntryType.Warning);
        }

        [TestMethod]
        public void Logger_Error_Event_One_Logger_Fails()
        {
            this.target.Warning(LoggerTest.logDescription1);
            Assert.AreEqual(0, this.loggerExceptions.Count);

            this.loggerProcessor1.Exception = new Exception("exception in loggerProcessor1");
            AggregateException aggregateException;

            this.target.Warning(LoggerTest.logDescription1);
            Assert.AreEqual(1, this.loggerExceptions.Count);
            aggregateException = (AggregateException)this.loggerExceptions[0];
            Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
            Assert.AreEqual(this.loggerProcessor1.Exception, aggregateException.InnerExceptions[0]);

            this.target.Info(LoggerTest.logDescription1);
            Assert.AreEqual(2, this.loggerExceptions.Count);
            aggregateException = (AggregateException)this.loggerExceptions[1];
            Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
            Assert.AreEqual(this.loggerProcessor1.Exception, aggregateException.InnerExceptions[0]);
        }

        [TestMethod]
        public void Logger_Error_Event_Two_Loggers_Fail()
        {
            this.target.Warning(LoggerTest.logDescription1);
            Assert.AreEqual(0, this.loggerExceptions.Count);

            this.loggerProcessor1.Exception = new Exception("exception in loggerProcessor1");
            this.loggerProcessor2.Exception = new Exception("exception in loggerProcessor2");
            AggregateException aggregateException;

            this.target.Warning(LoggerTest.logDescription1);
            Assert.AreEqual(1, this.loggerExceptions.Count);
            aggregateException = (AggregateException)this.loggerExceptions[0];
            Assert.AreEqual(2, aggregateException.InnerExceptions.Count);
            Assert.AreEqual(this.loggerProcessor1.Exception, aggregateException.InnerExceptions[0]);
            Assert.AreEqual(this.loggerProcessor2.Exception, aggregateException.InnerExceptions[1]);
        }

        [TestMethod]
        public void Logger_Error_Event_from_LggerProcessor()
        {
            this.target.Warning(LoggerTest.logDescription1);
            Assert.AreEqual(0, this.loggerExceptions.Count);

            Exception exception = new Exception("exception in loggerProcessor1");

            this.loggerProcessor1.RaiseErrorEvent(exception);
            Assert.AreEqual(1, this.loggerExceptions.Count);
            Assert.AreEqual(exception, this.loggerExceptions[0]);

            this.loggerProcessor1.RaiseErrorEvent(exception);
            Assert.AreEqual(2, this.loggerExceptions.Count);
            Assert.AreEqual(exception, this.loggerExceptions[1]);
        }

        [TestMethod]
        public void Logger_Counters()
        {
            LogCounters expectedCounters = new LogCounters();
            this.CompareCounters(expectedCounters);

            this.target.Info(LoggerTest.logDescription1);
            expectedCounters.InformationCount = 1;
            this.CompareCounters(expectedCounters);

            this.target.Info(LoggerTest.logDescription1);
            expectedCounters.InformationCount = 2;
            this.CompareCounters(expectedCounters);

            this.target.Warning(LoggerTest.logDescription1);
            expectedCounters.WarningCount = 1;
            this.CompareCounters(expectedCounters);

            this.target.Warning(LoggerTest.logDescription1);
            expectedCounters.WarningCount = 2;
            this.CompareCounters(expectedCounters);

            this.target.Error(LoggerTest.logDescription1);
            expectedCounters.ErrorCount = 1;
            this.CompareCounters(expectedCounters);

            this.target.Error(LoggerTest.logDescription1);
            expectedCounters.ErrorCount = 2;
            this.CompareCounters(expectedCounters);

            this.target.FailureAudit(LoggerTest.logDescription1);
            expectedCounters.FailureAuditCount = 1;
            this.CompareCounters(expectedCounters);

            this.target.FailureAudit(LoggerTest.logDescription1);
            expectedCounters.FailureAuditCount = 2;
            this.CompareCounters(expectedCounters);

            this.target.SuccessAudit(LoggerTest.logDescription1);
            expectedCounters.SuccessAuditCount = 1;
            this.CompareCounters(expectedCounters);

            this.target.SuccessAudit(LoggerTest.logDescription1);
            expectedCounters.SuccessAuditCount = 2;
            this.CompareCounters(expectedCounters);
        }

        [TestMethod]
        public void Logger_AddLoggerProcessor()
        {
            Logger target = new Logger(this.TestContext.TestName);
            loggerProcessor1 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));
            loggerProcessor2 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));
            Assert.AreEqual(0, loggerProcessor1.LogItems.Count);
            Assert.AreEqual(0, loggerProcessor2.LogItems.Count);

            target.AddLoggerProcessor(loggerProcessor1);
            target.Log(this.logItem1);
            Assert.AreEqual(1, loggerProcessor1.LogItems.Count);
            Assert.AreEqual(0, loggerProcessor2.LogItems.Count);

            target.AddLoggerProcessor(loggerProcessor2);
            target.Log(this.logItem1);
            Assert.AreEqual(2, loggerProcessor1.LogItems.Count);
            Assert.AreEqual(1, loggerProcessor2.LogItems.Count);
        }

        [TestMethod]
        public void Logger_AddLoggerProcessor_Must_Subscribe_To_Error_Event()
        {
            Logger target = new Logger(this.TestContext.TestName);
            loggerProcessor1 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));
            Assert.IsFalse(loggerProcessor1.HasErrorEventSubscribers);
            
            target.AddLoggerProcessor(loggerProcessor1);
            Assert.IsTrue(loggerProcessor1.HasErrorEventSubscribers);
        }

        [TestMethod]
        public void Logger_RemoveLoggerProcessor()
        {
            Logger target = new Logger(this.TestContext.TestName);
            loggerProcessor1 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));
            loggerProcessor2 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));

            target.AddLoggerProcessor(loggerProcessor1);
            target.AddLoggerProcessor(loggerProcessor2);

            target.Log(this.logItem1);
            Assert.AreEqual(1, loggerProcessor1.LogItems.Count);
            Assert.AreEqual(1, loggerProcessor2.LogItems.Count);

            target.RemoveLoggerProcessor(loggerProcessor1);
            target.Log(this.logItem1);
            Assert.AreEqual(1, loggerProcessor1.LogItems.Count);
            Assert.AreEqual(2, loggerProcessor2.LogItems.Count);

            target.RemoveLoggerProcessor(loggerProcessor2);
            target.Log(this.logItem1);
            Assert.AreEqual(1, loggerProcessor1.LogItems.Count);
            Assert.AreEqual(2, loggerProcessor2.LogItems.Count);
        }

        [TestMethod]
        public void Logger_RemoveLoggerProcessor_Must_Unsubscribe_From_Error_Event()
        {
            Logger target = new Logger(this.TestContext.TestName);
            loggerProcessor1 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));
            target.AddLoggerProcessor(loggerProcessor1);         
            Assert.IsTrue(loggerProcessor1.HasErrorEventSubscribers);
            target.RemoveLoggerProcessor(loggerProcessor1);         
            Assert.IsFalse(loggerProcessor1.HasErrorEventSubscribers);
        }

        [TestMethod]
        public void Logger_DisposeLoggerProcessors()
        {
            Logger target = new Logger(this.TestContext.TestName);
            loggerProcessor1 = new MockLoggerProcessor(string.Format("Log Processor1 - {0}", this.TestContext.TestName));
            loggerProcessor2 = new MockLoggerProcessor(string.Format("Log Processor2 - {0}", this.TestContext.TestName));
            target.AddLoggerProcessor(loggerProcessor1);
            target.AddLoggerProcessor(loggerProcessor2);
            target.DisposeLoggerProcessors();

            Assert.AreEqual(1, loggerProcessor1.DisposeCount);
            Assert.AreEqual(1, loggerProcessor2.DisposeCount);
        }

        private void CompareResults(EventLogEntryType expectedLogType)
        {
            Assert.AreEqual(2, this.loggerProcessor1.LogItems.Count);
            Assert.AreEqual(2, this.loggerProcessor2.LogItems.Count);

            Assert.AreEqual(expectedLogType, this.loggerProcessor1.LogItems[0].LogType);
            Assert.AreEqual(LoggerTest.logDescription1, this.loggerProcessor1.LogItems[0].Description);
            Assert.AreEqual(LoggerTest.customLogItem, this.loggerProcessor1.LogItems[0].CustomLogItem);

            Assert.AreEqual(expectedLogType, this.loggerProcessor1.LogItems[1].LogType);
            Assert.AreEqual(LoggerTest.logDescription2, this.loggerProcessor1.LogItems[1].Description);
        }

        private void CompareCounters(LogCounters expectedCounters)
        {
            Assert.AreEqual(expectedCounters.ErrorCount, this.target.ErrorCount);
            Assert.AreEqual(expectedCounters.FailureAuditCount, this.target.FailureAuditCount);
            Assert.AreEqual(expectedCounters.InformationCount, this.target.InformationCount);
            Assert.AreEqual(expectedCounters.SuccessAuditCount, this.target.SuccessAuditCount);
            Assert.AreEqual(expectedCounters.WarningCount, this.target.WarningCount);
        }

        void target_LoggerError(object sender, ErrorEventArgs e)
        {
            this.loggerExceptions.Add(e.GetException());
        }
    }
}