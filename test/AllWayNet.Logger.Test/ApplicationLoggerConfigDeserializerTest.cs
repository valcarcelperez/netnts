namespace AllWayNet.Logger.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Xml;

    [TestClass]
    public class ApplicationLoggerConfigDeserializerTest
    {
        private ApplicationLoggerConfigDeserializer target;
        private XmlReader reader = null;
        private MemoryStream stream = null;

        [TestInitialize]
        public void Init()
        {
        }

        [TestCleanup]
        public void Dispose()
        {
            if (this.reader != null)
            {
#if NET40
                this.reader.Close();
#else
                this.reader.Dispose();
#endif
            }

            if (this.stream != null)
            {
                this.stream.Dispose();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void ApplicationLoggerConfigDeserializer_DeserializeImplementers_Invalid_Xml()
        {
            string xmlText = "some invalid text";
            this.SetXmlText(xmlText);
            this.target = new ApplicationLoggerConfigDeserializer(reader);
            this.target.DeserializeImplementers();
        }

        [TestMethod]
        public void ApplicationLoggerConfigDeserializer_DeserializeImplementers_Without_Implementers()
        {
            string xmlText = @"<root/>";
            this.SetXmlText(xmlText);
            this.target = new ApplicationLoggerConfigDeserializer(this.reader);
            this.target.DeserializeImplementers();
            Assert.AreEqual(0, this.target.LoggerImplementers.Count);
        }

        [TestMethod]
        public void ApplicationLoggerConfigDeserializer_DeserializeImplementers_Without_Implementers_Empty_List()
        {
            string xmlText = @"
<root>
    <implementers/>
</root>";
            this.SetXmlText(xmlText);
            this.target = new ApplicationLoggerConfigDeserializer(this.reader);
            this.target.DeserializeImplementers();
            Assert.AreEqual(0, this.target.LoggerImplementers.Count);
        }

        [TestMethod]
        public void ApplicationLoggerConfigDeserializer_DeserializeImplementers_Unexpected_Node()
        {
            string xmlText = @"
<root>
    <implementers>
        <invalid_item/>
    </implementers>
</root>";
            this.SetXmlText(xmlText);
            try
            {
                this.target = new ApplicationLoggerConfigDeserializer(reader);
                this.target.DeserializeImplementers();
                Assert.Fail("An exception was not raised.");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(ConfigurationErrorsException), ex.GetType());
                StringAssert.Contains(ex.ToString(), "invalid_item");
            }
        }

        [TestMethod]
        public void ApplicationLoggerConfigDeserializer_DeserializeImplementers_Good_Nodes()
        {
            string xmlText = @"
<root>
    <implementers>
        <implementer type=""Type1"" name=""Name1"" customAttribute=""1"" />
        <implementer type=""Type2"" name=""Name2"" anotherAttribute=""2"" />
    </implementers>
</root>";
            this.SetXmlText(xmlText);
            this.target = new ApplicationLoggerConfigDeserializer(reader);
            this.target.DeserializeImplementers();
            Assert.AreEqual(2, this.target.LoggerImplementers.Count);
            Assert.AreEqual(@"<implementer type=""Type1"" name=""Name1"" customAttribute=""1"" />", this.target.LoggerImplementers[0].Xml.ToString());
            Assert.AreEqual(@"<implementer type=""Type2"" name=""Name2"" anotherAttribute=""2"" />", this.target.LoggerImplementers[1].Xml.ToString());
        }

        [TestMethod]
        public void ApplicationLoggerConfigDeserializer_DeserializeImplementers_Duplicated_Name()
        {
            string xmlText = @"
<root>
    <implementers>
        <implementer type=""Type1"" name=""Name1"" customAttribute=""1"" />
        <implementer type=""Type2"" name=""Name1"" anotherAttribute=""2"" />
    </implementers>
</root>";
            this.SetXmlText(xmlText);
            try
            {
                this.target = new ApplicationLoggerConfigDeserializer(reader);
                this.target.DeserializeImplementers();
                Assert.Fail("An exception was not raised.");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(ConfigurationErrorsException), ex.GetType());
                StringAssert.Contains(ex.ToString(), "Name1");
            }
        }

        private void SetXmlText(string xmlText)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(xmlText);
            this.stream = new MemoryStream(buffer);
            this.reader = XmlReader.Create(this.stream);
            this.reader.ReadToFollowing("root");
        }
    }
}