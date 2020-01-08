namespace AllWayNet.Logger
{
    using System.Configuration;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// Configuration for a Logger Implementer.
    /// </summary>
    public class LoggerImplementerConfig
    {
        /// <summary>
        /// Node name.
        /// </summary>
        public const string NodeName = "implementer";
        
        /// <summary>
        /// Attribute type.
        /// </summary>
        public const string AttributeImplementerType = "type";
        
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeImplementerName = "name";

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerImplementerConfig" /> class.
        /// </summary>
        /// <param name="xml">A XElement.</param>
        public LoggerImplementerConfig(XElement xml)
        {
            this.Type = this.GetRequiredAttribute(xml, AttributeImplementerType);
            this.Name = this.GetRequiredAttribute(xml, AttributeImplementerName);
            this.Xml = xml;
        }

        /// <summary>
        /// Gets the Type.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the Name.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Gets the Xml.
        /// </summary>
        public XElement Xml { get; private set; }

        /// <summary>
        /// Converts the value of this instance to a System.String.
        /// </summary>
        /// <returns>Actual configuration</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Name: {0}\r\n", this.Name);
            sb.AppendFormat("Type: {0}\r\n", this.Type);
            sb.AppendFormat("Xml:\r\n{0}", this.Xml.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Gets a required attribute.
        /// </summary>
        /// <param name="xml">A XElement.</param>
        /// <param name="attributeName">The attribute's name.</param>
        /// <returns>The value of the attribute.</returns>
        private string GetRequiredAttribute(XElement xml, string attributeName)
        {
            XAttribute attribute = xml.Attribute(attributeName);
            if (attribute == null)
            {
                string message = string.Format("Missing required attribute. Node : {0}, Attribute : {1}.", xml.Name, attributeName);
                throw new ConfigurationErrorsException(message);
            }

            return attribute.Value;
        }
    }
}