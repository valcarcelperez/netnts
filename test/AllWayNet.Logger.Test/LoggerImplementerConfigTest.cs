namespace AllWayNet.Logger.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Xml.Linq;

    [TestClass]
    public class LoggerImplementerConfigTest
    {
        [TestMethod]
        public void LoggerImplementerConfig_Constructor()
        {
            string xmlText = @"
<implementer name=""name1"" type=""type1"" anotherAttribute=""1"" />";
            XElement xml = XElement.Parse(xmlText);
            LoggerImplementerConfig target = new LoggerImplementerConfig(xml);
            Assert.AreEqual(xml, target.Xml);
            Assert.AreEqual("name1", target.Name);
            Assert.AreEqual("type1", target.Type);
        }

        [TestMethod]
        public void LoggerImplementerConfig_Constructor_Exception_When_Name_Is_Missing()
        {
            string xmlText = @"
<implementer type=""type1"" anotherAttribute=""1"" />";
            XElement xml = XElement.Parse(xmlText);
            try
            {
                new LoggerImplementerConfig(xml);
                Assert.Fail("An exception was not raised.");
            }
            catch (Exception ex)
            {
                StringAssert.Contains(ex.ToString(), "name");
            }
        }

        [TestMethod]
        public void LoggerImplementerConfig_Constructor_Exception_When_Type_Is_Missing()
        {
            string xmlText = @"
<implementer name=""name1"" anotherAttribute=""1"" />";
            XElement xml = XElement.Parse(xmlText);
            try
            {
                new LoggerImplementerConfig(xml);
                Assert.Fail("An exception was not raised.");
            }
            catch (Exception ex)
            {
                StringAssert.Contains(ex.ToString(), "type");
            }
        }
    }
}