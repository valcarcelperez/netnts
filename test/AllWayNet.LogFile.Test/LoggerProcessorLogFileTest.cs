namespace AllWayNet.LogFile.Test
{
    using AllWayNet.Logger;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Xml.Linq;

    [TestClass]
    public class LoggerProcessorLogFileTest
    {
        private LoggerProcessorLogFile target;
        private string fileName;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.fileName = string.Format("{0}.log", this.TestContext.TestName);
            this.fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.fileName);
            if (File.Exists(this.fileName))
            {
                File.Delete(this.fileName);
            }

            this.target = new LoggerProcessorLogFile();
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
        public void LoggerProcessorLogFile_Constructor()
        {
            LoggerProcessorLogFile target = new LoggerProcessorLogFile();
            target.Dispose();
        }

        [TestMethod]
        public void LoggerProcessorLogFile_Prepare()
        {
            XElement xml = new XElement("node");
            this.AddAttribute(xml, "name", this.TestContext.TestName);
            this.AddAttribute(xml, "filename", this.fileName);
            
            string dateTimeFormat = "yyyy-MM-dd";
            this.AddAttribute(xml, "dateTimeFormat", dateTimeFormat);

            int maxLogSizeKB = 10;
            this.AddAttribute(xml, "maxLogSizeKB", maxLogSizeKB);

            TimeSpan maxLogAge = TimeSpan.Parse("7.00:00:00");
            this.AddAttribute(xml, "maxLogAge", maxLogAge.ToString());

            bool isXmlTemplate = true;
            this.AddAttribute(xml, "isXmlTemplate", isXmlTemplate);
            
            XElement template = new XElement("template");
            xml.Add(template);
            
            this.target.Prepare(xml);

            Assert.AreEqual(this.TestContext.TestName, this.target.Name);
            Assert.AreEqual(this.fileName, this.target.Filename);
            Assert.AreEqual(dateTimeFormat, this.target.DateTimeFormat);
            Assert.AreEqual(maxLogSizeKB, this.target.MaxLogSizeKB);
            Assert.AreEqual(maxLogAge, this.target.MaxLogAge);
            Assert.AreEqual(isXmlTemplate, this.target.IsXmlTemplate);
        }

        [TestMethod]
        public void LoggerProcessorLogFile_Prepare_Name_Attribute_Must_Be_Required()
        {
            XElement xml = new XElement("node");
            this.AddAttribute(xml, "filename", this.fileName);
            XElement template = new XElement("template");
            xml.Add(template);

            try
            {
                this.target.Prepare(xml);
                Assert.Fail("An exception was not raised.");
            }
            catch (ConfigurationErrorsException e)
            {
                StringAssert.Contains(e.Message, "Attribute : name.");
            }
        }

        [TestMethod]
        public void LoggerProcessorLogFile_Prepare_Template_Element_Must_Be_Required()
        {
            XElement xml = new XElement("node");
            this.AddAttribute(xml, "name", this.TestContext.TestName);
            this.AddAttribute(xml, "filename", this.fileName);

            try
            {
                this.target.Prepare(xml);
                Assert.Fail("An exception was not raised.");
            }
            catch (ConfigurationErrorsException e)
            {
                StringAssert.Contains(e.Message, "Element : template.");
            }
        }

        [TestMethod]
        public void LoggerProcessorLogFile_Prepare_Directory_Must_Be_Created()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.TestContext.TestName);
            this.fileName = Path.Combine(directory, "test-file.log");
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            XElement xml = new XElement("node");
            this.AddAttribute(xml, "name", this.TestContext.TestName);
            this.AddAttribute(xml, "filename", this.fileName);
            XElement template = new XElement("template");
            xml.Add(template);

            Assert.IsFalse(Directory.Exists(directory));
            this.target.Prepare(xml);
            Assert.IsTrue(Directory.Exists(directory));
        }

        [TestMethod]
        public void LoggerProcessorLogFile_Prepare_File_Must_Be_Created()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.TestContext.TestName);
            this.fileName = Path.Combine(directory, "test-file.log");
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            XElement xml = new XElement("node");
            this.AddAttribute(xml, "name", this.TestContext.TestName);
            this.AddAttribute(xml, "filename", this.fileName);
            XElement template = new XElement("template");
            xml.Add(template);

            Assert.IsFalse(File.Exists(this.fileName));
            this.target.Prepare(xml);
            Assert.IsTrue(File.Exists(this.fileName));
        }

        [TestMethod]
        public void LoggerProcessorLogFile_Log()
        {
            string description = "a description";
            LogItem log = new LogItem(EventLogEntryType.Error, description);
            string dateTimeFormat = "yyyy-MM-dd HH:mm:ss:ffffff";
            string expectedMessage = string.Format("-{0}-{1}-{2}-", log.DateTime.ToString(dateTimeFormat), log.ThreadId, log.Description);
            
            this.PrepareLog(this.target, dateTimeFormat);
            this.target.Log(log);
            Thread.Sleep(1000);

            string fileContent = this.ReadFile(this.fileName);
            Assert.AreEqual(expectedMessage, fileContent);
        }

        [TestMethod]
        public void LoggerProcessorLogFile_Log_Is_Added_To_File()
        {
            string oldText = "old text in the file\n";
            File.WriteAllText(this.fileName, oldText);
            LoggerProcessorLogFile target = new LoggerProcessorLogFile();

            string description = "a description";
            LogItem log = new LogItem(EventLogEntryType.Error, description);
            string dateTimeFormat = "yyyy-MM-dd HH:mm:ss:ffffff";
            string expectedMessage = string.Format("{3}-{0}-{1}-{2}-", log.DateTime.ToString(dateTimeFormat), log.ThreadId, log.Description, oldText);

            this.PrepareLog(target, dateTimeFormat);
            target.Log(log);
            Thread.Sleep(1000);

            string fileContent = this.ReadFile(this.fileName);
            Assert.AreEqual(expectedMessage, fileContent);
        }

        private void PrepareLog(LoggerProcessorLogFile logger, string dateTimeFormat)
        {
            string template = "-#DateTime-#ThreadId-#Description-";
            XElement xml = new XElement("node");
            this.AddAttribute(xml, "name", this.TestContext.TestName);
            this.AddAttribute(xml, "filename", this.fileName);
            this.AddAttribute(xml, "dateTimeFormat", dateTimeFormat);
            XElement templateNode = new XElement("template", template);
            xml.Add(templateNode);
            logger.Prepare(xml);
        }

        private void AddAttribute(XElement xml, XName name, object value)
        {
            XAttribute attribute = new XAttribute(name, value);
            xml.Add(attribute);
        }

        private string ReadFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
                return Encoding.ASCII.GetString(buffer);
            }
        }
    }
}
