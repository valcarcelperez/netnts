namespace AllWayNet.EventLog
{
    using System;
    using System.IO;
    using System.Xml.Linq;
    using AllWayNet.Common.Configuration;
    using AllWayNet.Logger;

    /// <summary>
    /// Implements a logger processor that logs to the Event Logs.
    /// </summary>
    public class LoggerProcessorEventLog : ILoggerProcessor
    {
        private const string AttributeName = "name";
        private const string AttributeSource = "source";
        private const string AttributeLogName = "logName";
        private const string AttributeDateTimeFormat = "dateTimeFormat";
        private const string ElementTemplate = "template";

        private const string DefaultLogName = "Application";

        private System.Diagnostics.EventLog eventLog = null;
        private bool disposed = false;

        /// <summary>
        /// Finalizes an instance of the <see cref="LoggerProcessorEventLog" /> class.
        /// </summary>
        ~LoggerProcessorEventLog()
        {
            this.Dispose(false);
        }

#pragma warning disable 67
        /// <summary>
        /// Event raised when there is an internal error in the Logger Processor.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;
#pragma warning restore 67

        /// <summary>
        /// Gets the Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the Source.
        /// </summary>
        public string Source { get; private set; }
        
        /// <summary>
        /// Gets the LogName.
        /// </summary>
        public string LogName { get; private set; }
        
        /// <summary>
        /// Gets the Template.
        /// </summary>
        public string Template { get; private set; }
        
        /// <summary>
        /// Gets the DateTimeFormat.
        /// </summary>
        public string DateTimeFormat { get; private set; }

        /// <summary>
        /// Logs a LogItem.
        /// </summary>
        /// <param name="log"></param>
        public void Log(LogItem log)
        {
            string message = log.BuildText(this.Template, this.DateTimeFormat);
            this.eventLog.WriteEntry(message, log.LogType);
        }

        /// <summary>
        /// Prepares the Logger Processor.
        /// </summary>
        /// <param name="xml"></param>
        public void Prepare(XElement xml)
        {
            this.DecodeConfig(xml);
            this.Template = ConfigurationHelper.GetElementValue(xml, ElementTemplate);

            this.DisposeEventLog();
            this.eventLog = new System.Diagnostics.EventLog(this.LogName);
            this.eventLog.Source = this.Source;
        }

        /// <summary>
        /// Creates the Log Source.
        /// </summary>
        /// <param name="xml"></param>
        public void Install(XElement xml)
        {
            this.DecodeConfig(xml);
            if (!System.Diagnostics.EventLog.SourceExists(this.Source))
            {
                System.Diagnostics.EventLog.CreateEventSource(this.Source, this.LogName);            
            }
        }

        /// <summary>
        /// Removes the Log Source.
        /// </summary>
        /// <param name="xml"></param>
        public void Uninstall(XElement xml)
        {
            this.DecodeConfig(xml);
            if (System.Diagnostics.EventLog.SourceExists(this.Source))
            {
                System.Diagnostics.EventLog.DeleteEventSource(this.Source);
            }
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.DisposeEventLog();
                }

                this.disposed = true;
            }
        }

        private void DecodeConfig(XElement xml)
        {
            this.Name = ConfigurationHelper.GetAttributeValue(xml, AttributeName);
            this.Source = ConfigurationHelper.GetAttributeValue(xml, AttributeSource);
            this.LogName = ConfigurationHelper.GetAttributeValue(xml, AttributeLogName, DefaultLogName);
            this.DateTimeFormat = ConfigurationHelper.GetAttributeValue(xml, AttributeDateTimeFormat, LogItem.DefaultDateTimeFormat);
        }

        private void DisposeEventLog()
        {
            if (this.eventLog != null)
            {
                this.eventLog.Dispose();
                this.eventLog = null;
            }
        }
    }
}