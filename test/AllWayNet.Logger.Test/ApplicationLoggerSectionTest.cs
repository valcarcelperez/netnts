namespace AllWayNet.Logger.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Configuration;

    /// <summary>
    /// This test is responsible for testing that the applicationLogger section can be loaded from the config file.
    /// The test ApplicationLoggerConfigDeserializerTest is responsible of testing how the section is deserialized.
    /// </summary>
    [TestClass]
    public class ApplicationLoggerSectionTest
    {
        [TestMethod]
        public void ApplicationLoggerSection_Config_File_Must_Be_Present()
        {
            string value = ConfigurationManager.AppSettings["TestValue"];
            Assert.IsFalse(string.IsNullOrEmpty(value), "App.Config not found.");
        }

        [TestMethod]
        public void ApplicationLoggerSection_DeserializeImplementers_Section_Found()
        {
            ApplicationLoggerSection target = (ApplicationLoggerSection)ConfigurationManager.GetSection("applicationLoggerTest01");
            Assert.IsNotNull(target);
            Assert.IsNotNull(target.LoggerImplementers);
            Assert.AreEqual(2, target.LoggerImplementers.Count);
        }
    }
}