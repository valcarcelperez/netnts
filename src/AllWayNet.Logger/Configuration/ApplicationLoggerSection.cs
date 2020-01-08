namespace AllWayNet.Logger
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Defines the ConfigurationSection for the ApplicationLogger. 
    /// </summary>
    public class ApplicationLoggerSection : ConfigurationSection
    {
        /// <summary>
        /// Section name.
        /// </summary>
        public const string SectionName = "applicationLogger";
        
        /// <summary>
        /// Gets the LoggerImplementers.
        /// </summary>
        public IList<LoggerImplementerConfig> LoggerImplementers { get; private set; }

        /// <summary>
        /// Converts the value of this instance to a System.String.
        /// </summary>
        /// <returns>Actual configuration</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Application Logger Configuration ({0})\r\n", SectionName);

            if (this.LoggerImplementers != null)
            {
                if (this.LoggerImplementers.Count == 0)
                {
                    sb.Append("Implementers: none");
                }
                else
                {
                    sb.Append("Implementers:\r\n");
                    foreach (LoggerImplementerConfig implementer in this.LoggerImplementers)
                    {
                        sb.Append(implementer.ToString());
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reads XML from the configuration file.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader that reads from the configuration file.</param>
        /// <param name="serializeCollectionKey">True to serialize only the collection key properties; otherwise, false.</param>
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            this.DeserializeImplementers(reader);
        }

        /// <summary>
        /// Deserializes log implementers.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader that reads from the configuration file.</param>
        private void DeserializeImplementers(XmlReader reader)
        {
            this.LoggerImplementers = ApplicationLoggerConfigDeserializer.GetLoggerImplementers(reader);
        }
    }
}