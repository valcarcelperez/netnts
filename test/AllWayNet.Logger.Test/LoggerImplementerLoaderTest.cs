namespace AllWayNet.Logger.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// Classe use by LoggerImplementerLoaderTest
    /// </summary>
    public class TestClass
    {
    }

    [TestClass]
    public class LoggerImplementerLoaderTest
    {
        [TestMethod]
        public void LoggerImplementerLoader_Load()
        {
            string xmlText = @"<implementer name=""ImplementerA"" type=""AllWayNet.Logger.Test.MockLoggerProcessor, AllWayNet.Logger.Test"" />";
            XElement xml = XElement.Parse(xmlText);
            LoggerImplementerConfig loggerConfig = new LoggerImplementerConfig(xml);

            LoggerImplementerLoader target = new LoggerImplementerLoader();
            ILoggerProcessor loggerProcessor = target.Load(loggerConfig);
            Assert.IsTrue(loggerProcessor is MockLoggerProcessor);
        }

        [TestMethod]
        public void LoggerImplementerLoader_Load_Invalid_Type()
        {
            string xmlText = @"<implementer name=""ImplementerA"" type=""AllWayNet.Logger.Test.TestClass, AllWayNet.Logger.Test"" />";
            XElement xml = XElement.Parse(xmlText);
            LoggerImplementerConfig loggerConfig = new LoggerImplementerConfig(xml);
            LoggerImplementerLoader target = new LoggerImplementerLoader();

            try
            {
                target.Load(loggerConfig);
                Assert.Fail("An exception was not raised.");
            }
            catch (Exception ex)
            {
                StringAssert.Contains(ex.Message, "ImplementerA");
            }
        }
    }
}