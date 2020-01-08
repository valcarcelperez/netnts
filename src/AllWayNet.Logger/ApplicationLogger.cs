namespace AllWayNet.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using AllWayNet.Common.Threading;

    /// <summary>
    /// Singleton Application Logger.
    /// </summary>
    public class ApplicationLogger : IDisposable
    {
        /// <summary>
        /// Template used when when logging exceptions.
        /// </summary>
        private const string ExceptionText = "{0}\r\nException:\r\n{1}";

        /// <summary>
        /// Instance of ApplicationLogger.
        /// </summary>
        private static ApplicationLogger instance;

        /// <summary>
        /// Indicates that the object has been disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Timer for updating performance counters.
        /// </summary>
        private TimerEx counterUpdaterTimer = null;

        /// <summary>
        /// Helper counter to count 60 seconds.
        /// </summary>
        private int counterUpdaterTimerCount = 0;

        /// <summary>
        /// Value of ErrorCount when ErrorsPerMinuteCount was calculated.
        /// </summary>
        private int lastPerformanceCounterErrorCount = 0;

        /// <summary>
        /// Value of WarningCount when WarningsPerMinuteCount was calculated.
        /// </summary>
        private int lastPerformanceCounterWarningCount = 0;

        /// <summary>
        /// Performance counter for keeping track of the number of errors.
        /// </summary>
        private PerformanceCounter performanceCounterErrorCount = null;

        /// <summary>
        /// Performance counter for keeping track of the number of warnings.
        /// </summary>
        private PerformanceCounter performanceCounterWarningCount = null;

        /// <summary>
        /// Performance counter for keeping track of the number of errors per minute.
        /// </summary>
        private PerformanceCounter performanceCounterErrorsPerMinuteCount = null;

        /// <summary>
        /// Performance counter for keeping track of the number of warnings per minute.
        /// </summary>
        private PerformanceCounter performanceCounterWarningsPerMinuteCount = null;

        /// <summary>
        /// Performance counter for keeping track of the number of errors.
        /// </summary>
        private PerformanceCounter performanceCounterErrorCountDelta = null;

        /// <summary>
        /// Performance counter for keeping track of the number of warnings.
        /// </summary>
        private PerformanceCounter performanceCounterWarningCountDelta = null;

        /// <summary>
        /// A list with the delegate that maintain performance counters.
        /// </summary>
        private List<Action> performanceCounterManagers = new List<Action>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLogger" /> class.
        /// </summary>
        /// <param name="updatePerformaceCounters">Indicates that the performance counters must be updated.</param>
        /// <param name="performaceCountersCategoryName">Category name to locate the performance counters.</param>
        /// <param name="applicationInstanceName">InstanceName to set the performance counters.</param>
        public ApplicationLogger(bool updatePerformaceCounters, string performaceCountersCategoryName, string applicationInstanceName)
        {
            this.Logger = new Logger("Application Logger");
            if (!updatePerformaceCounters)
            {
                return;
            }

            if (LoggerPerformanceCounters.StandardCountersEnabled)
            {
                this.performanceCounterErrorCount = this.CreatePerformanceCounterInstance(
                    performaceCountersCategoryName,
                    LoggerPerformanceCounters.ErrorCountName,
                    applicationInstanceName);

                this.performanceCounterWarningCount = this.CreatePerformanceCounterInstance(
                    performaceCountersCategoryName,
                    LoggerPerformanceCounters.WarningCountName,
                    applicationInstanceName);

                this.performanceCounterManagers.Add(this.UpdateStandardPerformanceCounters);
            }

            if (LoggerPerformanceCounters.CountersPerMinuteEnabled)
            {
                this.performanceCounterErrorsPerMinuteCount = this.CreatePerformanceCounterInstance(
                    performaceCountersCategoryName,
                    LoggerPerformanceCounters.ErrorsPerMinuteCountName,
                    applicationInstanceName);

                this.performanceCounterWarningsPerMinuteCount = this.CreatePerformanceCounterInstance(
                    performaceCountersCategoryName,
                    LoggerPerformanceCounters.WarningsPerMinuteCountName,
                    applicationInstanceName);

                this.performanceCounterManagers.Add(this.UpdatePerMinutePerformanceCounters);
            }

            if (LoggerPerformanceCounters.DeltaCountersEnabled)
            {
                this.performanceCounterErrorCountDelta = this.CreatePerformanceCounterInstance(
                    performaceCountersCategoryName,
                    LoggerPerformanceCounters.ErrorCountDeltaName,
                    applicationInstanceName);

                this.performanceCounterWarningCountDelta = this.CreatePerformanceCounterInstance(
                    performaceCountersCategoryName,
                    LoggerPerformanceCounters.WarningCountDeltaName,
                    applicationInstanceName);

                this.performanceCounterManagers.Add(this.UpdateDeltaPerformanceCounters);
            }

            this.counterUpdaterTimer = new TimerEx(1000, this.UpdateCounts);
            this.counterUpdaterTimer.Error += this.CounterUpdaterTimerError;
            this.counterUpdaterTimer.Start();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ApplicationLogger" /> class.
        /// </summary>
        ~ApplicationLogger()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event raised for errors inside the logger.
        /// </summary>
        public static event EventHandler<ErrorEventArgs> LoggerError
        {
            add
            {
                instance.Logger.LoggerError += value;
            }

            remove
            {
                instance.Logger.LoggerError -= value;
            }
        }

        /// <summary>
        /// Gets the total number Information logs that have been logged since the application started.
        /// </summary>
        public static long InformationCount
        {
            get
            {
                return instance.Logger.InformationCount;
            }
        }

        /// <summary>
        /// Gets the total number Error logs that have been logged since the application started.
        /// </summary>
        public static long ErrorCount
        {
            get
            {
                return instance.Logger.ErrorCount;
            }
        }

        /// <summary>
        /// Gets the total number FailureAudit logs that have been logged since the application started.
        /// </summary>
        public static long FailureAuditCount
        {
            get
            {
                return instance.Logger.FailureAuditCount;
            }
        }

        /// <summary>
        /// Gets the total number SuccessAudit logs that have been logged since the application started.
        /// </summary>
        public static long SuccessAuditCount
        {
            get
            {
                return instance.Logger.SuccessAuditCount;
            }
        }

        /// <summary>
        /// Gets the total number Warning logs that have been logged since the application started.
        /// </summary>
        public static long WarningCount
        {
            get
            {
                return instance.Logger.WarningCount;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ApplicationLogger was initialized.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Gets or sets the Logger.
        /// </summary>
        private Logger Logger { get; set; }

        /// <summary>
        /// Initializes the ApplicationLogger.
        /// </summary>
        /// <param name="updatePerformaceCounters">Indicates that the performance counters must be updated.</param>
        /// <param name="performaceCountersCategoryName">Category name to locate the performance counters.</param>
        /// <param name="instanceName">InstanceName to set the performance counters.</param>
        public static void Initialize(bool updatePerformaceCounters, string performaceCountersCategoryName, string instanceName)
        {
            instance = new ApplicationLogger(updatePerformaceCounters, performaceCountersCategoryName, instanceName);
            Initialized = true;
        }

        /// <summary>
        /// Adds a Logger Processor. This is the way of registering Loggers processor with the Application Logger.
        /// </summary>
        /// <param name="loggerProcessor">A ILoggerProcessor</param>
        public static void AddLoggerProcessor(ILoggerProcessor loggerProcessor)
        {
            instance.Logger.AddLoggerProcessor(loggerProcessor);
        }

        /// <summary>
        /// Removes a Logger Processor. This is the way of un-registering Loggers processor with the Application Logger.
        /// </summary>
        /// <param name="loggerProcessor">An ILoggerProcessor</param>
        public static void RemoveLoggerProcessor(ILoggerProcessor loggerProcessor)
        {
            instance.Logger.RemoveLoggerProcessor(loggerProcessor);
        }

        /// <summary>
        /// Logs a log entry.
        /// </summary>
        /// <param name="type">An EventLogEntryType</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(EventLogEntryType type, string description, params object[] args)
        {
            instance.Logger.Log(type, string.Format(description, args));
        }

        /// <summary>
        /// Schedule a log to be logged in a separate thread.
        /// </summary>
        /// <param name="type">An EventLogEntryType</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void ScheduleLog(EventLogEntryType type, string description, params object[] args)
        {
            ScheduleLog(type, null, description, args);
        }

        /// <summary>
        /// Logs a log entry passing a custom log item.
        /// </summary>
        /// <param name="type">An EventLogEntryType</param>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Log(EventLogEntryType type, ICustomLogItem customLogItem, string description, params object[] args)
        {
            instance.Logger.Log(type, string.Format(description, args), customLogItem);
        }

        /// <summary>
        /// Schedule a logs a log entry passing a custom log item.
        /// </summary>
        /// <param name="type">An EventLogEntryType</param>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void ScheduleLog(EventLogEntryType type, ICustomLogItem customLogItem, string description, params object[] args)
        {
            description = string.Format(description, args);
            LogItem logItem = new LogItem(type, description, customLogItem);
            Task task = Task.Factory.StartNew(() => instance.Logger.Log(logItem));
        }

        /// <summary>
        /// Logs an information log entry.
        /// </summary>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogInfo(string description, params object[] args)
        {
            instance.Logger.Info(string.Format(description, args));
        }

        /// <summary>
        /// Logs an information log entry passing a custom log item.
        /// </summary>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogInfo(ICustomLogItem customLogItem, string description, params object[] args)
        {
            instance.Logger.Info(string.Format(description, args), customLogItem);
        }

        /// <summary>
        /// Logs a warning log entry.
        /// </summary>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogWarning(string description, params object[] args)
        {
            instance.Logger.Warning(string.Format(description, args));
        }

        /// <summary>
        /// Logs a warning log entry passing a custom log item.
        /// </summary>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogWarning(ICustomLogItem customLogItem, string description, params object[] args)
        {
            instance.Logger.Warning(string.Format(description, args), customLogItem);
        }

        /// <summary>
        /// Logs a warning log entry passing an exception.
        /// </summary>
        /// <param name="exception">An Exception</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogWarning(Exception exception, string description, params object[] args)
        {
            string newDescription = string.Format(description, args);
            instance.Logger.Warning(string.Format(ExceptionText, newDescription, exception.ToString()));
        }

        /// <summary>
        /// Logs an error log entry.
        /// </summary>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogError(string description, params object[] args)
        {
            instance.Logger.Error(string.Format(description, args));
        }

        /// <summary>
        /// Logs an error log entry passing a custom log item.
        /// </summary>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogError(ICustomLogItem customLogItem, string description, params object[] args)
        {
            instance.Logger.Error(string.Format(description, args), customLogItem);
        }

        /// <summary>
        /// Logs an error log entry passing an exception.
        /// </summary>
        /// <param name="exception">An Exception</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogError(Exception exception, string description, params object[] args)
        {
            string newDescription = string.Format(description, args);
            instance.Logger.Error(string.Format(ExceptionText, newDescription, exception.ToString()));
        }

        /// <summary>
        /// Logs a failure audit log.
        /// </summary>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogFailureAudit(string description, params object[] args)
        {
            instance.Logger.FailureAudit(string.Format(description, args));
        }

        /// <summary>
        /// Logs a failure audit log entry passing a custom log item.
        /// </summary>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogFailureAudit(ICustomLogItem customLogItem, string description, params object[] args)
        {
            instance.Logger.FailureAudit(string.Format(description, args), customLogItem);
        }

        /// <summary>
        /// Logs a success audit log entry.
        /// </summary>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogSuccessAudit(string description, params object[] args)
        {
            instance.Logger.SuccessAudit(string.Format(description, args));
        }

        /// <summary>
        /// Logs a success audit log entry passing a custom log item.
        /// </summary>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="description">Log's description</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogSuccessAudit(ICustomLogItem customLogItem, string description, params object[] args)
        {
            instance.Logger.SuccessAudit(string.Format(description, args), customLogItem);
        }

        /// <summary>
        /// Adds logger processors that are declared in the configuration file.
        /// </summary>
        public static void AddLoggerProcessorsFromConfig()
        {
            LoggerImplementerLoader loader = new LoggerImplementerLoader();

            try
            {
                foreach (LoggerImplementerConfig loggerConfig in ApplicationLoggerConfig.ApplicationLogger.LoggerImplementers)
                {
                    ILoggerProcessor loggerProcessor = loader.Load(loggerConfig);
                    loggerProcessor.Prepare(loggerConfig.Xml);
                    AddLoggerProcessor(loggerProcessor);
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("Error while adding Logger Processors.", ex);
            }
        }

        /// <summary>
        /// Disposes and removes the Logger Processors.
        /// </summary>
        public static void DisposeLoggerProcessors()
        {
            instance.Logger.DisposeLoggerProcessors();
        }

        /// <summary>
        /// Stops the timer if running and release the resources used by this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Part of the disposing mechanism.
        /// </summary>
        /// <param name="disposing">Indicates that the Dispose method is being called by the user code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.counterUpdaterTimer.Dispose();
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Creates an instance of PerformanceCounter.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <param name="counterName">Performance Counter name.</param>
        /// <param name="instanceName">Application instance name.</param>
        /// <returns>A instance of PerformanceCounter.</returns>
        private PerformanceCounter CreatePerformanceCounterInstance(string categoryName, string counterName, string instanceName)
        {
            PerformanceCounter performanceCounter = new PerformanceCounter();
            performanceCounter.CategoryName = categoryName;
            performanceCounter.CounterName = counterName;
            performanceCounter.InstanceName = instanceName;
            performanceCounter.ReadOnly = false;
            performanceCounter.RawValue = 0;
            return performanceCounter;
        }

        /// <summary>
        /// Validates if performance counters exist. May take a long time.
        /// </summary>
        /// <param name="categoryName">Category name where the performance counters are located.</param>
        /// <param name="counterNames">Collection of performance counters.</param>
        private void ValidatePerformanceCounters(string categoryName, string[] counterNames)
        {
            bool categoriExists = PerformanceCounterCategory.Exists(categoryName);
            if (!categoriExists)
            {
                string message = string.Format(
                    "Performance Counter error. Category name '{0}' does not exists. Installing or reinstalling the application may fix the issue.",
                    categoryName);
                throw new InvalidOperationException(message);
            }

            bool counterExists;
            foreach (string counterName in counterNames)
            {
                // 2014-06-21: EV: As per MS documentation, avoid calling this during initialization.
                counterExists = PerformanceCounterCategory.CounterExists(counterName, categoryName);
                if (!counterExists)
                {
                    string message = string.Format(
                        "Performance Counter error. Category name '{0}'. Counter '{1}' does not exists. Installing or reinstalling the application may fix the issue.",
                        categoryName,
                        counterName);
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Updates performance counters ErrorCount and WarningCount.
        /// </summary>
        private void UpdateCounts()
        {
            foreach (var action in this.performanceCounterManagers)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Updates performance counters ErrorCount and WarningsCount.
        /// </summary>
        private void UpdateStandardPerformanceCounters()
        {
            this.performanceCounterErrorCount.RawValue = this.Logger.ErrorCount;
            this.performanceCounterWarningCount.RawValue = this.Logger.WarningCount;
        }

        /// <summary>
        /// Updates performance counters ErrorCountDelta and WarningsCountDelta.
        /// </summary>
        private void UpdateDeltaPerformanceCounters()
        {
            this.performanceCounterErrorCountDelta.RawValue = this.Logger.ErrorCount;
            this.performanceCounterWarningCountDelta.RawValue = this.Logger.WarningCount;
        }

        /// <summary>
        /// Updates counters CounterErrorsPerMinuteCount and CounterWarningsPerMinuteCount.
        /// </summary>
        private void UpdatePerMinutePerformanceCounters()
        {
            this.counterUpdaterTimerCount++;

            if (this.counterUpdaterTimerCount >= 60)
            {
                this.performanceCounterErrorsPerMinuteCount.RawValue = this.Logger.ErrorCount - this.lastPerformanceCounterErrorCount;
                this.lastPerformanceCounterErrorCount = this.Logger.ErrorCount;

                this.performanceCounterWarningsPerMinuteCount.RawValue = this.Logger.WarningCount - this.lastPerformanceCounterWarningCount;
                this.lastPerformanceCounterWarningCount = this.Logger.WarningCount;

                this.counterUpdaterTimerCount = 0;
            }
        }

        /// <summary>
        /// Handles timer errors.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">An ErrorEventArgs.</param>
        private void CounterUpdaterTimerError(object sender, ErrorEventArgs e)
        {
            ApplicationLogger.LogError(e.GetException(), "Error in counterUpdaterTimer.");
        }
    }
}