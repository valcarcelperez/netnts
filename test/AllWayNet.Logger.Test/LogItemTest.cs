namespace AllWayNet.Logger.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Threading;

    [TestClass]
    public class LogItemTest
    {
        private const string Template = "\r\n--------------------\r\nDateTime: #DateTime, ThreadId: #ThreadId, Type: #Type\r\nDescription:\r\n#Description\r\n#CustomLogItem";
        private const string XmpTemplate =@"<logItem dateTime=""#DateTime"" threadId=""#ThreadId"" type=""#Type"" /><description>#Description</description>#CustomLogItem</logItem>";

        [TestMethod]
        public void LogItem_Constructor()
        {
            EventLogEntryType logType = EventLogEntryType.Error;
            string description = "a description";
            ICustomLogItem customLogItem = new MockCustomLogItem { PropertyA = 1, PropertyB = "a value" };
            DateTime beforeCall = DateTime.Now;
            LogItem target = new LogItem(logType, description, customLogItem);
            DateTime afterCall = DateTime.Now;

            Assert.AreEqual(logType, target.LogType);
            Assert.AreEqual(description, target.Description);
            Assert.AreEqual(customLogItem, target.CustomLogItem);
            Assert.IsTrue(target.DateTime >= beforeCall);
            Assert.IsTrue(target.DateTime <= afterCall);
            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, target.ThreadId);
        }

        public void LogItem_Constructor_Passing_DateTime_And_ThreadId()
        {
            EventLogEntryType logType = EventLogEntryType.FailureAudit;
            string description = "a description";
            ICustomLogItem customLogItem = new MockCustomLogItem { PropertyA = 1, PropertyB = "a value" };
            DateTime dateTime = DateTime.UtcNow;
            int threadId = 101;
            LogItem target = new LogItem(logType, description, customLogItem, dateTime, threadId);

            Assert.AreEqual(logType, target.LogType);
            Assert.AreEqual(description, target.Description);
            Assert.AreEqual(customLogItem, target.CustomLogItem);
            Assert.AreEqual(dateTime, target.DateTime);
            Assert.AreEqual(threadId, target.ThreadId);
        }

        [TestMethod]
        public void LogItem_Constructor_Passing_Null_CustomLogItem()
        {
            EventLogEntryType logType = EventLogEntryType.Error;
            string description = "a description";
            ICustomLogItem customLogItem = null;
            LogItem target = new LogItem(logType, description, customLogItem);

            Assert.AreEqual(customLogItem, target.CustomLogItem);
        }

        [TestMethod]
        public void LogItem_Constructor_Using_Default_CustomLogItem()
        {
            EventLogEntryType logType = EventLogEntryType.Error;
            string description = "a description";
            LogItem target = new LogItem(logType, description);

            Assert.IsNull(target.CustomLogItem);
        }

        [TestMethod]
        public void LogItem_Clone()
        {
            EventLogEntryType logType = EventLogEntryType.Error;
            string description = "a description";
            MockCustomLogItem customLogItem = new MockCustomLogItem { PropertyA = 1, PropertyB = "a value" };
            LogItem target = this.CreateTarget(logType, description, customLogItem);
            Thread.Sleep(25);
            LogItem clone = target.Clone();

            Assert.AreEqual(logType, clone.LogType);
            Assert.AreEqual(description, clone.Description);
            Assert.AreEqual(target.DateTime, clone.DateTime);
            Assert.AreEqual(target.ThreadId, clone.ThreadId);
            Assert.AreEqual(1, customLogItem.CloneCount);
        }

        [TestMethod]
        public void LogItem_Clone_Item_With_Null_CustomLogItem()
        {
            EventLogEntryType logType = EventLogEntryType.Error;
            string description = "a description";
            LogItem target = this.CreateTarget(logType, description);
            Thread.Sleep(25);
            LogItem clone = target.Clone();

            Assert.AreEqual(logType, clone.LogType);
            Assert.AreEqual(description, clone.Description);
            Assert.AreEqual(target.DateTime, clone.DateTime);
            Assert.AreEqual(target.ThreadId, clone.ThreadId);
            Assert.IsNull(clone.CustomLogItem);
        }

        [TestMethod]
        public void LogItem_BuildText_Non_Xml_Template()
        {
            MockCustomLogItem customLogItem = new MockCustomLogItem { PropertyA = 1, PropertyB = "a value" };
            DateTime dateTime = new DateTime(2000, 1, 2, 10, 11, 12, 120);
            int threadId = 101;
            LogItem target = this.CreateTarget(EventLogEntryType.SuccessAudit, "a-description", customLogItem, dateTime, threadId);
            string expected = "\r\n--------------------\r\nDateTime: 2000-01-02 10:11:12.120, ThreadId: 101, Type: SuccessAudit\r\nDescription:\r\na-description\r\nMockCustomLogItem: PropertyA: 1, PropertyB: a value";
            string actual = target.BuildText(Template);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LogItem_BuildText_Non_Xml_Template_Null_CustomLogItem()
        {
            DateTime dateTime = new DateTime(2000, 1, 2, 10, 11, 12, 120);
            int threadId = 101;
            LogItem target = this.CreateTarget(EventLogEntryType.SuccessAudit, "a-description", null, dateTime, threadId);
            string expected = "\r\n--------------------\r\nDateTime: 2000-01-02 10:11:12.120, ThreadId: 101, Type: SuccessAudit\r\nDescription:\r\na-description\r\n";
            string actual = target.BuildText(Template);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LogItem_BuildText_Xml_Template()
        {
            MockXmlCustomLogItem customLogItem = new MockXmlCustomLogItem { PropertyA = 1, PropertyB = "a value" };
            DateTime dateTime = new DateTime(2000, 1, 2, 10, 11, 12, 120);
            int threadId = 101;
            LogItem target = this.CreateTarget(EventLogEntryType.SuccessAudit, "a-description", customLogItem, dateTime, threadId);
            string expected = @"<logItem dateTime=""2000-01-02 10:11:12.120"" threadId=""101"" type=""SuccessAudit"" /><description>a-description</description><mockXmlCustomLogItem propertyA=""1"" propertyB=""a value""/></logItem>";
            string actual = target.BuildText(XmpTemplate, LogItem.DefaultDateTimeFormat, true);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LogItem_BuildText_Xml_Template_Null_CustomLogItem()
        {
            DateTime dateTime = new DateTime(2000, 1, 2, 10, 11, 12, 120);
            int threadId = 101;
            LogItem target = this.CreateTarget(EventLogEntryType.SuccessAudit, "a-description", null, dateTime, threadId);
            string expected = @"<logItem dateTime=""2000-01-02 10:11:12.120"" threadId=""101"" type=""SuccessAudit"" /><description>a-description</description></logItem>";
            string actual = target.BuildText(XmpTemplate, LogItem.DefaultDateTimeFormat, true);
            Assert.AreEqual(expected, actual);
        }

        private LogItem CreateTarget(EventLogEntryType logType, string description, ICustomLogItem customLogItem)
        {
            return new LogItem(logType, description, customLogItem);
        }

        private LogItem CreateTarget(EventLogEntryType logType, string description)
        {
            return new LogItem(logType, description);
        }

        private LogItem CreateTarget(EventLogEntryType logType, string description, ICustomLogItem customLogItem, DateTime dateTime, int threadId)
        {
            return new LogItem(logType, description, customLogItem, dateTime, threadId);
        }

        public class MockCustomLogItem : ICustomLogItem
        {
            public int CloneCount;

            public int PropertyA { get; set; }
            public string PropertyB { get; set; }

            public ICustomLogItem Clone()
            {
                this.CloneCount++;
                return new MockCustomLogItem { PropertyA = this.PropertyA, PropertyB = this.PropertyB };
            }

            public override string ToString()
            {
                return string.Format("MockCustomLogItem: PropertyA: {0}, PropertyB: {1}", this.PropertyA, this.PropertyB);
            }
        }

        public class MockXmlCustomLogItem : MockCustomLogItem
        {
            public override string ToString()
            {
                return string.Format("<mockXmlCustomLogItem propertyA=\"{0}\" propertyB=\"{1}\"/>", this.PropertyA, this.PropertyB);
            }
        }
    }
}