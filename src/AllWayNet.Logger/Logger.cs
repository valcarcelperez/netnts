namespace AllWayNet.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using AllWayNet.Common.Threading;

    /// <summary>
    /// Defines a logger that logs using log implementers.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Object used when locking.
        /// </summary>
        private object lockObject = new object();

        /// <summary>
        /// Collection of ILoggerProcessor.
        /// </summary>
        private List<ILoggerProcessor> loggerProcessors = new List<ILoggerProcessor>();

        /// <summary>
        /// Total number of Error logs.
        /// </summary>
        private ConcurrentCounter errorCount = ConcurrentCounter.Zero;
        
        /// <summary>
        /// Total number of FailureAudit logs.
        /// </summary>
        private ConcurrentCounter failureAuditCount = ConcurrentCounter.Zero;
        
        /// <summary>
        /// Total number of Information logs.
        /// </summary>
        private ConcurrentCounter informationCount = ConcurrentCounter.Zero;
        
        /// <summary>
        /// Total number of SuccessAudit logs.
        /// </summary>
        private ConcurrentCounter successAuditCount = ConcurrentCounter.Zero;
        
        /// <summary>
        /// Total number of Warning logs.
        /// </summary>
        private ConcurrentCounter warningCount = ConcurrentCounter.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger" /> class.
        /// </summary>
        /// <param name="name">Name that identify the logger.</param>
        public Logger(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Event raise when exceptions occurs.
        /// </summary>
        public event EventHandler<ErrorEventArgs> LoggerError;

        /// <summary>
        /// Gets the name that identify the logger.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the ErrorCount.
        /// </summary>
        public int ErrorCount
        {
            get
            {
                return this.errorCount.Value;
            }
        }

        /// <summary>
        /// Gets the FailureAuditCount.
        /// </summary>
        public int FailureAuditCount
        {
            get
            {
                return this.failureAuditCount.Value;
            }
        }

        /// <summary>
        /// Gets the InformationCount.
        /// </summary>
        public int InformationCount
        {
            get
            {
                return this.informationCount.Value;
            }
        }

        /// <summary>
        /// Gets the SuccessAuditCount.
        /// </summary>
        public int SuccessAuditCount
        {
            get
            {
                return this.successAuditCount.Value;
            }
        }

        /// <summary>
        /// Gets the WarningCount.
        /// </summary>
        public int WarningCount
        {
            get
            {
                return this.warningCount.Value;
            }
        }

        /// <summary>
        /// Adds a log processor.
        /// </summary>
        /// <param name="loggerProcessor">An ILoggerProcessor.</param>
        public void AddLoggerProcessor(ILoggerProcessor loggerProcessor)
        {
            lock (this.lockObject)
            {
                loggerProcessor.Error += this.HandleLoggerError;
                this.loggerProcessors.Add(loggerProcessor);
            }
        }

        /// <summary>
        /// Removes a log processor.
        /// </summary>
        /// <param name="loggerProcessor">An ILoggerProcessor.</param>
        public void RemoveLoggerProcessor(ILoggerProcessor loggerProcessor)
        {
            lock (this.lockObject)
            {
                this.loggerProcessors.Remove(loggerProcessor);
                loggerProcessor.Error -= this.HandleLoggerError;
            }
        }

        /// <summary>
        /// Logs a log.
        /// </summary>
        /// <param name="logItem">A LogItem.</param>
        public void Log(LogItem logItem)
        {
            this.IncLogCounter(logItem.LogType);
            List<Exception> exceptions = new List<Exception>();
            lock (this.lockObject)
            {
                foreach (ILoggerProcessor loggerProcessor in this.loggerProcessors)
                {
                    try
                    {
                        loggerProcessor.Log(logItem);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }

            if (exceptions.Count > 0)
            {
                this.OnError(exceptions);
            }
        }

        /// <summary>
        /// Logs a log.
        /// </summary>
        /// <param name="type">An EventLogEntryType.</param>
        /// <param name="description">A description.</param>
        /// <param name="customLogItem">An ICustomLogItem.</param>
        public void Log(EventLogEntryType type, string description, ICustomLogItem customLogItem = null)
        {
            LogItem logItem = new LogItem(type, description, customLogItem);
            this.Log(logItem);
        }

        /// <summary>
        /// Logs an error log.
        /// </summary>
        /// <param name="description">A description.</param>
        /// <param name="customLogItem">An ICustomLogItem.</param>
        public void Error(string description, ICustomLogItem customLogItem = null)
        {
            this.Log(EventLogEntryType.Error, description, customLogItem);
        }

        /// <summary>
        /// Logs a warning log.
        /// </summary>
        /// <param name="description">A description.</param>
        /// <param name="customLogItem">An ICustomLogItem.</param>
        public void Warning(string description, ICustomLogItem customLogItem = null)
        {
            this.Log(EventLogEntryType.Warning, description, customLogItem);
        }

        /// <summary>
        /// Logs an info log.
        /// </summary>
        /// <param name="description">A description.</param>
        /// <param name="customLogItem">An ICustomLogItem.</param>
        public void Info(string description, ICustomLogItem customLogItem = null)
        {
            this.Log(EventLogEntryType.Information, description, customLogItem);
        }

        /// <summary>
        /// Logs a failure audit log.
        /// </summary>
        /// <param name="description">A description.</param>
        /// <param name="customLogItem">An ICustomLogItem.</param>
        public void FailureAudit(string description, ICustomLogItem customLogItem = null)
        {
            this.Log(EventLogEntryType.FailureAudit, description, customLogItem);
        }

        /// <summary>
        /// Logs a success audit log.
        /// </summary>
        /// <param name="description">A description.</param>
        /// <param name="customLogItem">An ICustomLogItem.</param>
        public void SuccessAudit(string description, ICustomLogItem customLogItem = null)
        {
            this.Log(EventLogEntryType.SuccessAudit, description, customLogItem);
        }

        /// <summary>
        /// Dispose all the log processors.
        /// </summary>
        public void DisposeLoggerProcessors()
        {
            lock (this.lockObject)
            {
                foreach (var processor in this.loggerProcessors.ToArray())
                {
                    this.loggerProcessors.Remove(processor);
                    processor.Dispose();
                }
            }
        }

        /// <summary>
        /// Method called when an exception is handled.
        /// </summary>
        /// <param name="e">An ErrorEventArgs.</param>
        protected virtual void OnError(ErrorEventArgs e)
        {
            EventHandler<ErrorEventArgs> handler = this.LoggerError;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Method called when an exception is handled.
        /// </summary>
        /// <param name="exceptions">A List of exceptions.</param>
        protected virtual void OnError(IList<Exception> exceptions)
        {
            EventHandler<ErrorEventArgs> handler = this.LoggerError;
            if (handler != null)
            {
                AggregateException ex = new AggregateException(exceptions);
                handler(this, new ErrorEventArgs(ex));
            }
        }

        /// <summary>
        /// Handles the Error event from logger processors.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">An ErrorEventArgs.</param>
        private void HandleLoggerError(object sender, ErrorEventArgs e)
        {
            this.OnError(e);
        }

        /// <summary>
        /// Increments a log counter according to a log type.
        /// </summary>
        /// <param name="type">An EventLogEntryType.</param>
        private void IncLogCounter(EventLogEntryType type)
        {
            switch (type)
            {
                case EventLogEntryType.Error:
                    this.errorCount.Inc();
                    break;
                case EventLogEntryType.FailureAudit:
                    this.failureAuditCount.Inc();
                    break;
                case EventLogEntryType.Information:
                    this.informationCount.Inc();
                    break;
                case EventLogEntryType.SuccessAudit:
                    this.successAuditCount.Inc();
                    break;
                case EventLogEntryType.Warning:
                    this.warningCount.Inc();
                    break;
            }
        }
    }
}