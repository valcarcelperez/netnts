namespace AllWayNet.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Deserializes logger implementers from the configuration file.
    /// </summary>
    public class ApplicationLoggerConfigDeserializer
    {
        /// <summary>
        /// Name used for the node.
        /// </summary>
        public const string LoggerImplementersNodeName = "implementers";

        /// <summary>
        /// Xml built from the XmlReader passed to the constructor.
        /// </summary>
        private XElement xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLoggerConfigDeserializer" /> class.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader that reads from the configuration file.</param>
        public ApplicationLoggerConfigDeserializer(XmlReader reader)
        {
            string xmlText = reader.ReadOuterXml();
            this.xml = XElement.Parse(xmlText);
        }

        /// <summary>
        /// Gets the LoggerImplementers.
        /// </summary>
        public IList<LoggerImplementerConfig> LoggerImplementers { get; private set; }

        /// <summary>
        /// Deserializes logger implementers from the configuration file.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader that reads from the configuration file.</param>
        /// <returns>A collection of LoggerImplementerConfig.</returns>
        public static IList<LoggerImplementerConfig> GetLoggerImplementers(XmlReader reader)
        {
            ApplicationLoggerConfigDeserializer deserializer = new ApplicationLoggerConfigDeserializer(reader);
            deserializer.DeserializeImplementers();
            return deserializer.LoggerImplementers;
        }

        /// <summary>
        /// Deserializes the implementers.
        /// </summary>
        public void DeserializeImplementers()
        {
            this.LoggerImplementers = new List<LoggerImplementerConfig>();
            XElement implementers = this.xml.Element(LoggerImplementersNodeName);
            if (implementers == null)
            {
                return;
            }

            try
            {
                foreach (var implementer in implementers.Elements())
                {
                    if (implementer.Name != LoggerImplementerConfig.NodeName)
                    {
                        string message = string.Format("Unexpected node '{0}'.", implementer.Name);
                        throw new ConfigurationErrorsException(message);
                    }

                    LoggerImplementerConfig implementerConfig = new LoggerImplementerConfig(implementer);

                    if (this.LoggerImplementers.Any(a => a.Name == implementerConfig.Name))
                    {
                        string message = string.Format("Duplicated implementer name. Name : {0}", implementerConfig.Name);
                        throw new ConfigurationErrorsException(message);
                    }

                    this.LoggerImplementers.Add(implementerConfig);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Error while deserializing {0}.", LoggerImplementersNodeName);
                throw new ConfigurationErrorsException(message, ex);
            }
        }
    }
}