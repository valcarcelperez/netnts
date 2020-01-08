namespace AllWayNet.Common.Configuration
{
    using System;
    using System.Configuration;
    using System.Xml.Linq;

    /// <summary>
    /// Helper class with Configuration related methods.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Returns the value of an attribute.
        /// </summary>
        /// <param name="xml">A XElement with the configuration node.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="defaultValue">A default value to be returned if the attribute is not found.</param>
        /// <returns>The attribute or the default value.</returns>
        public static string GetAttributeValue(XElement xml, string attributeName, string defaultValue = null)
        {
            XAttribute attribute = xml.Attribute(attributeName);
            if (attribute == null)
            {
                if (defaultValue == null)
                {
                    string message = string.Format("Attribute not found. Attribute : {0}.", attributeName);
                    throw new ConfigurationErrorsException(message);
                }

                return defaultValue;
            }

            return attribute.Value;
        }

        /// <summary>
        /// Returns the value of an integer attribute.
        /// </summary>
        /// <param name="xml">A XElement with the configuration node.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="defaultValue">A default value to be returned if the attribute is not found.</param>
        /// <returns>The attribute or the default value.</returns>
        public static int GetIntAttributeValue(XElement xml, string attributeName, int? defaultValue = null)
        {
            string tempDefaultValue = null;
            if (defaultValue.HasValue)
            {
                tempDefaultValue = defaultValue.Value.ToString();
            }

            string temp = GetAttributeValue(xml, attributeName, tempDefaultValue);
            int result;
            bool converted = int.TryParse(temp, out result);
            if (!converted)
            {
                string message = string.Format("Invalid value for attribute : {0}.", attributeName);
                throw new ConfigurationErrorsException(message);
            }

            return result;
        }

        /// <summary>
        /// Returns the value of a Boolean attribute.
        /// </summary>
        /// <param name="xml">A XElement with the configuration node.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="defaultValue">A default value to be returned if the attribute is not found.</param>
        /// <returns>The attribute or the default value.</returns>
        public static bool GetBooleanAttributeValue(XElement xml, string attributeName, bool? defaultValue = null)
        {
            string tempDefaultValue = null;
            if (defaultValue.HasValue)
            {
                tempDefaultValue = defaultValue.Value.ToString();
            }

            string temp = GetAttributeValue(xml, attributeName, tempDefaultValue);
            bool result;
            bool converted = bool.TryParse(temp, out result);
            if (!converted)
            {
                string message = string.Format("Invalid value for attribute : {0}.", attributeName);
                throw new ConfigurationErrorsException(message);
            }

            return result;
        }

        /// <summary>
        /// Returns the value of an TimeSpan attribute.
        /// </summary>
        /// <param name="xml">A XElement with the configuration node.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="defaultValue">A default value to be returned if the attribute is not found.</param>
        /// <returns>The attribute or the default value.</returns>
        public static TimeSpan GetTimeSpanAttributeValue(XElement xml, string attributeName, TimeSpan? defaultValue = null)
        {
            string tempDefaultValue = null;
            if (defaultValue.HasValue)
            {
                tempDefaultValue = defaultValue.Value.ToString();
            }

            string temp = GetAttributeValue(xml, attributeName, tempDefaultValue);
            TimeSpan result;
            bool converted = TimeSpan.TryParse(temp, out result);
            if (converted)
            {
                return result;
            }
            else
            {
                string message = string.Format("Invalid value for attribute : {0}.", attributeName);
                throw new ConfigurationErrorsException(message);
            }
        }

        /// <summary>
        /// Returns the value of an element.
        /// </summary>
        /// <param name="xml">A XElement with the configuration node.</param>
        /// <param name="elementName">The element name.</param>
        /// <returns>The element or the default value.</returns>
        public static string GetElementValue(XElement xml, string elementName)
        {
            XElement templateNode = xml.Element(elementName);
            if (templateNode == null)
            {
                string message = string.Format("Element not found. Element : {0}.", elementName);
                throw new ConfigurationErrorsException(message);
            }

            return templateNode.Value;
        }
    }
}
