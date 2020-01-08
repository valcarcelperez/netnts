namespace AllWayNet.Logger
{
    using System;
    using System.Configuration;
    using System.Runtime.Remoting;

    /// <summary>
    /// Loads a logger implementer.
    /// </summary>
    public class LoggerImplementerLoader
    {
        /// <summary>
        /// Loads a logger implementer.
        /// </summary>
        /// <param name="loggerConfig">A LoggerImplementerConfig.</param>
        /// <returns>An ILoggerProcessor.</returns>
        public ILoggerProcessor Load(LoggerImplementerConfig loggerConfig)
        {
            ImplementerType implementerType = this.GetTypeInfo(loggerConfig.Type, loggerConfig.Name);
            return this.CreateLoggerImplementer(implementerType, loggerConfig.Name);
        }

        /// <summary>
        /// Gets the information about the type of the implementer.
        /// </summary>
        /// <param name="typeInfo">A string with the type.</param>
        /// <param name="loggerName">The name of the logger implementer.</param>
        /// <returns>An ImplementerType.</returns>
        private ImplementerType GetTypeInfo(string typeInfo, string loggerName)
        {
            string[] fields = typeInfo.Split(',');
            if (fields.Length != 2)
            {
                string message = string.Format("Logger implementer '{0}'. Invalid type", loggerName);
                throw new ConfigurationErrorsException(message);
            }

            return new ImplementerType { ClassName = fields[0], AssemblyName = fields[1] };
        }

        /// <summary>
        /// Creates a logger implementer from an ImplementerType. 
        /// </summary>
        /// <param name="implementerType">An ImplementerType.</param>
        /// <param name="loggerName">The name of the logger implementer.</param>
        /// <returns>An ILoggerProcessor.</returns>
        private ILoggerProcessor CreateLoggerImplementer(ImplementerType implementerType, string loggerName)
        {
            ObjectHandle objectHandle = Activator.CreateInstance(implementerType.AssemblyName, implementerType.ClassName);
            object implementer = objectHandle.Unwrap();
            if (implementer is ILoggerProcessor)
            {
                return implementer as ILoggerProcessor;
            }
            else
            {
                string message = string.Format("Logger implementer '{0}'. Type does not implement ILoggerProcessor.", loggerName);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Holds information about a type.
        /// </summary>
        private class ImplementerType
        {
            /// <summary>
            /// Gets or sets the ClassName.
            /// </summary>
            public string ClassName { get; set; }
            
            /// <summary>
            /// Gets or sets the AssemblyName.
            /// </summary>
            public string AssemblyName { get; set; }
        }
    }
}