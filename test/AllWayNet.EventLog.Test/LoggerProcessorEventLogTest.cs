namespace AllWayNet.Logger.EventLogger.Test
{
    using AllWayNet.EventLog;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics;
    using System.Threading;
    using System.Xml.Linq;

    public class MockCustomLogItem : ICustomLogItem
    {
        private string toStringValue;
        public MockCustomLogItem(string toStringValue)
        {
            this.toStringValue = toStringValue;
        }

        public ICustomLogItem Clone()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return this.toStringValue;
        }
    }

    [TestClass]
    public class LoggerProcessorEventLogTest
    {
        private LoggerProcessorEventLog target;

        private string expectedName = "AName";
        private string expectedSource;
        private string expectedLogName = "ALogName";
        private string expectedDateTimeFormat = "yyyy-MM-dd HH:mm:ss:ffffff";
        private string expectedTemplate = "ATemplate";

        private string xmlConfigTemplate = @"<node name=""{0}"" source=""{1}"" logName=""{2}"" dateTimeFormat=""{3}"">
    <template>{4}</template>
</node>";

        private XElement xml;

        private EventLogEntry eventLogEntry;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.expectedSource = this.TestContext.TestName;
            this.xml = this.BuildConfig(this.expectedName, this.expectedSource, this.expectedLogName, this.expectedDateTimeFormat, this.expectedTemplate);
            RemoveLog(this.expectedSource);
            this.target = new LoggerProcessorEventLog();
        }

        [TestCleanup]
        public void Dispose()
        {
            RemoveLog(this.expectedSource);
        }

        [TestMethod]
        public void LoggerProcessorEventLog_Constructor()
        {
            LoggerProcessorEventLog target = new LoggerProcessorEventLog();
        }

        [TestMethod]
        public void LoggerProcessorEventLog_Prepare()
        {
            this.target.Prepare(this.xml);
            Assert.AreEqual(expectedName, this.target.Name);
            Assert.AreEqual(expectedSource, this.target.Source);
            Assert.AreEqual(expectedLogName, this.target.LogName);
            Assert.AreEqual(expectedDateTimeFormat, this.target.DateTimeFormat);
            Assert.AreEqual(expectedTemplate, this.target.Template);
        }

        [TestMethod]
        public void LoggerProcessorEventLog_Install()
        {
            Assert.IsFalse(EventLog.SourceExists(expectedSource));
            this.target.Install(this.xml);
            Thread.Sleep(1000);
            Assert.IsTrue(EventLog.SourceExists(expectedSource));
        }

        [TestMethod]
        public void LoggerProcessorEventLog_Uninstall()
        {
            this.CreateLog(this.expectedSource, this.expectedLogName);
            Assert.IsTrue(EventLog.SourceExists(expectedSource));
            this.target.Uninstall(this.xml);
            Assert.IsFalse(EventLog.SourceExists(expectedSource));
        }

        [TestMethod]
        public void LoggerProcessorEventLog_Log()
        {
            this.CreateLog(expectedSource, expectedLogName);
            Assert.IsTrue(EventLog.SourceExists(expectedSource));

            EventLog eventLog = new EventLog();
            eventLog.Source = this.expectedSource;
            eventLog.EntryWritten += eventLog_EntryWritten;
            eventLog.EnableRaisingEvents = true;
            this.eventLogEntry = null;

            string description = "a description";
            LogItem log = new LogItem(EventLogEntryType.Error, description);

            this.expectedTemplate = "-#DateTime-#ThreadId-#Description-";
            this.xml = this.BuildConfig(this.expectedName, this.expectedSource, this.expectedLogName, this.expectedDateTimeFormat, this.expectedTemplate);
            this.target.Prepare(this.xml);
            this.target.Log(log);

            Thread.Sleep(1000);
            Assert.IsNotNull(this.eventLogEntry);
            Assert.AreEqual(EventLogEntryType.Error, this.eventLogEntry.EntryType);

            string expectedMessage = string.Format("-{0}-{1}-{2}-", log.DateTime.ToString(this.expectedDateTimeFormat), log.ThreadId, log.Description);
            Assert.AreEqual(expectedMessage, this.eventLogEntry.Message);
        }

        [TestMethod]
        public void LoggerProcessorEventLog_Log_With_CustomLogItem()
        {
            this.CreateLog(expectedSource, expectedLogName);
            Assert.IsTrue(EventLog.SourceExists(expectedSource));

            EventLog eventLog = new EventLog();
            eventLog.Source = this.expectedSource;
            eventLog.EntryWritten += eventLog_EntryWritten;
            eventLog.EnableRaisingEvents = true;
            this.eventLogEntry = null;

            string description = "a description";
            ICustomLogItem customLogItem = new MockCustomLogItem("ACustomLogItem");
            LogItem log = new LogItem(EventLogEntryType.Error, description, customLogItem);

            this.expectedTemplate = "-#DateTime-#ThreadId-#Description-#CustomLogItem-";
            this.xml = this.BuildConfig(this.expectedName, this.expectedSource, this.expectedLogName, this.expectedDateTimeFormat, this.expectedTemplate);
            this.target.Prepare(this.xml);
            this.target.Log(log);

            Thread.Sleep(1000);
            Assert.IsNotNull(this.eventLogEntry);
            Assert.AreEqual(EventLogEntryType.Error, this.eventLogEntry.EntryType);

            string expectedMessage = string.Format("-{0}-{1}-{2}-{3}-", log.DateTime.ToString(this.target.DateTimeFormat), log.ThreadId, log.Description, "ACustomLogItem");
            Assert.AreEqual(expectedMessage, this.eventLogEntry.Message);
        }

        void eventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            this.eventLogEntry = e.Entry;
        }

        private XElement BuildConfig(string name, string source, string logName, string dateTimeFormat, string template)
        {
            string xmlText = string.Format(this.xmlConfigTemplate, name, source, logName, dateTimeFormat, template);
            return XElement.Parse(xmlText);
        }

        private void RemoveLog(string source)
        {
            if (EventLog.SourceExists(source))
            {
                EventLog.DeleteEventSource(source);
            }
        }

        private void CreateLog(string source, string logName)
        {
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, logName);
                Thread.Sleep(1000);
            }
        }
    }
}