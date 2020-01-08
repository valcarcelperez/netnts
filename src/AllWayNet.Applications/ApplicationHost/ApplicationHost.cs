namespace AllWayNet.Applications
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using AllWayNet.Applications.WinForms;
    using AllWayNet.Applications.WinServices;
    using AllWayNet.Logger;

    /// <summary>
    /// Defines an ApplicationHost.
    /// </summary>
    public class ApplicationHost
    {
        /// <summary>
        /// Object used for locking.
        /// </summary>
        private object displayLock = new object();

        /// <summary>
        /// Indicates that a message is being displayed. Used when running as desktop.
        /// </summary>
        private bool displaying = false;

        /// <summary>
        /// Indicates that the ApplicationHost is initialized.
        /// </summary>
        private bool applicationHostInitialized = false;

        /// <summary>
        /// Indicates that the errors from the logger must be logs to the EventLog.
        /// </summary>
        private volatile bool logErrorsFromLogger = true;

        /// <summary>
        /// Command line arguments.
        /// </summary>
        private string[] args;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationHost" /> class.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <param name="logSourceName">EventLog source used for unhandled exceptions and exception in the loggers.</param>
        public ApplicationHost(string[] args, string logSourceName)
        {
            AppDomain.CurrentDomain.UnhandledException += this.UnhandledException;
            
            // All the initialization exceptions are handled in the UnhandledException.

            // The application runs as a Desktop application when running in user interactive mode.
            this.IsDesktopApp = Environment.UserInteractive;

            this.args = args;

            // When the ApplicationHost runs as a Desktop the hostable process will start automatically when AutoStart was set to true in Program.Main()
            // or when it is indicated in the command line arguments.
            AutoStart = AutoStart || this.args.Any(a => a == "-autostart");

            this.ValidateAndSetLogSourceName(logSourceName);
        }

        /// <summary>
        /// Gets or sets a delegate called when the application host is initializing.
        /// </summary>
        public static Action ApplicationHostInitializing { get; set; }

        /// <summary>
        /// Gets or sets a delegate called when the application host is finalizing.
        /// </summary>
        public static Action ApplicationHostFinalizing { get; set; }

        /// <summary>
        /// Gets the CommandLineInstanceName. When using a custom instance name it shall be passed as [-instance=Name] in the command line.
        /// </summary>
        public static string CommandLineInstanceName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Desktop application must starts the <c>HostableProcess</c>.
        /// </summary>
        public static bool AutoStart { get; set; }

        /// <summary>
        /// Gets the source name when writing to the event log.
        /// </summary>
        public string LogSourceName { get; private set; }

        /// <summary>
        /// Gets the <c>HostableProcess</c> that is being hosted by the application.
        /// </summary>
        public HostableProcess HostableProcess { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the ApplicationHost is running as a Desktop application.
        /// </summary>
        public bool IsDesktopApp { get; private set; }

        /// <summary>
        /// Gets a reference to the host. It can be a WinFormHost or a WinServiceHost
        /// </summary>
        public object Host { get; internal set; }
        
        /// <summary>
        /// Gets or sets a delegate called when the application host is initializing.
        /// </summary>
        public Action Initializing { get; set; }

        /// <summary>
        /// Gets or sets a delegate called when the application host is finalizing.
        /// </summary>
        public Action Finalizing { get; set; }

        /// <summary>
        /// Gets or sets the running the RunHost delegate.
        /// </summary>
        public Action<HostableProcess> RunHost { get; set; }

        /// <summary>
        /// Gets the internal instance.
        /// </summary>
        internal static ApplicationHost Instance { get; private set; }

        /// <summary>
        /// Begins running an application that hosts a <c>HostableProcess</c>.
        /// </summary>
        /// <typeparam name="T">The type of the <c>HostableProcess</c>.</typeparam>
        /// <param name="logSourceName">EventLog source used for unhandled exceptions and exception in the loggers.</param>
        /// <remarks>The parameter isDesktopApp can be overwritten by passing [-desktop] in the command line args.</remarks>
        /// <remarks>When logSourceName is not null it is validated checking that the EventLog source exists. Use null if writing to the EventLog is not necessary.</remarks>
        public static void Run<T>(string logSourceName) where T : HostableProcess, new()
        {
            PrepareApplicationHostInstance(logSourceName);
            Instance.Run<T>();
        }

        /// <summary>
        /// Begins running an application that hosts a <c>HostableProcess</c>.
        /// </summary>
        /// <param name="hostableProcess">A <c>HostableProcess</c>.</param>
        /// <param name="logSourceName">EventLog source used for unhandled exceptions and exception in the loggers.</param>
        /// <remarks>The parameter isDesktopApp can be overwritten by passing [-desktop] in the command line args.</remarks>
        /// <remarks>When logSourceName is not null it is validated checking that the EventLog source exists. Use null if writing to the EventLog is not necessary.</remarks>
        public static void Run(HostableProcess hostableProcess, string logSourceName = null)
        {
            PrepareApplicationHostInstance(logSourceName);
            Instance.Run(hostableProcess);
        }

        /// <summary>
        /// Logs the configuration.
        /// </summary>
        public static void LogConfiguration()
        {
            string text = ApplicationLoggerConfig.ApplicationLogger.ToString();
            ApplicationLogger.LogInfo(text);
        }

        /// <summary>
        /// Terminates the application.
        /// </summary>
        /// <param name="exitCode">Exit code to be given to the operating system.</param>
        /// <param name="logMessage">A message to be logged.</param>
        public static void Terminate(int exitCode, string logMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(logMessage))
            {
                EventLogEntryType type = exitCode == 1 ? EventLogEntryType.Error : EventLogEntryType.Information;
                ApplicationLogger.Log(type, logMessage);
                Thread.Sleep(2000);
            }

            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Logs an exception to the EventLog.
        /// </summary>
        /// <param name="exception">An Exception.</param>
        /// <param name="description">An description about the exception.</param>
        /// <remarks>If writing to the EventLog fails while the application is loading the log and the new exception are saved to the ConcurrentLogsQueue.</remarks>
        /// <remarks>If writing to the EventLog fails after the application was loaded the while the application is loading the log is saved to the ConcurrentLogsQueue.</remarks>
        public void LogErrorToEventLog(Exception exception, string description)
        {
            if (string.IsNullOrWhiteSpace(this.LogSourceName))
            {
                return;
            }

            try
            {
                using (EventLog eventLog = new EventLog())
                {
                    eventLog.Source = this.LogSourceName;
                    string newDescription = string.Format("{0}. {1}", description, exception.ToString());
                    eventLog.WriteEntry(newDescription, EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while attempting to write to the EventLog.", ex);
            }
        }

        /// <summary>
        /// Begins running an application that hosts a <c>HostableProcess</c>.
        /// </summary>
        /// <typeparam name="T">The type of the <c>HostableProcess</c>.</typeparam>
        public void Run<T>() where T : HostableProcess, new()
        {
            T hostableProcess = this.InstantiateHostableProcess<T>();
            this.Run(hostableProcess);
        }

        /// <summary>
        /// Begins running an application that hosts a <c>HostableProcess</c>.
        /// </summary>
        /// <param name="hostableProcess">A <c>HostableProcess</c>.</param>
        public void Run(HostableProcess hostableProcess)
        {
            this.HostableProcess = hostableProcess;
            this.InitApplicationHost();

            // after this point unhandled exceptions are logged to the ApplicationLogger.
            this.applicationHostInitialized = true;

            this.RunApplicationHost();
            this.EndApplicationHost();
        }

        /// <summary>
        /// Prepares the private instance of the ApplicationHost.
        /// </summary>
        /// <param name="logSourceName">EventLog source used for unhandled exceptions and exception in the loggers.</param>
        private static void PrepareApplicationHostInstance(string logSourceName)
        {
            string[] args = Environment.GetCommandLineArgs();
            SetCommandLineInstanceName(args);
            Instance = new ApplicationHost(args, logSourceName);
            Instance.Finalizing = () =>
            {
                if (ApplicationHostFinalizing != null)
                {
                    ApplicationHostFinalizing();
                }
            };

            Instance.Initializing = () =>
            {
                ApplicationLogger.Initialize(
                    Instance.HostableProcess.UpdatePerformanceCounters,
                    Instance.HostableProcess.PerformanceCountersCategoryName,
                    Instance.HostableProcess.ApplicationInstanceName);
                ApplicationLogger.AddLoggerProcessorsFromConfig();

                if (ApplicationHostInitializing != null)
                {
                    ApplicationHostInitializing();
                }
            };

            Instance.RunHost = (hostableProcess) =>
            {
                if (Instance.IsDesktopApp)
                {
                    WinFormHost.RunHost(hostableProcess, Instance.args);
                }
                else
                {
                    WinServiceHost.RunHost(hostableProcess);
                }
            };
        }

        /// <summary>
        /// Loads the custom instance name from the command line.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private static void SetCommandLineInstanceName(string[] args)
        {
            string arg = args.FirstOrDefault(a => a.StartsWith("-instance="));
            if (arg == null)
            {
                return;
            }

            string[] lines = arg.Split('=');
            CommandLineInstanceName = lines[1];
        }

        /// <summary>
        /// Initializes the Application Host.
        /// </summary>
        /// <remarks>Exceptions in this method are handled in UnhandledException.</remarks>
        private void InitApplicationHost()
        {
            try
            {
                if (this.Initializing != null)
                {
                    this.Initializing();
                }

                if (ApplicationLogger.Initialized)
                {
                    ApplicationLogger.LoggerError += Instance.ApplicationLoggerLoggerError;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while initializing ApplicationHost.", ex);
            }
        }

        /// <summary>
        /// Begins running the application.
        /// </summary>
        /// <remarks>Exceptions in this method are handled in UnhandledException.</remarks>
        private void RunApplicationHost()
        {
            try
            {
                if (this.RunHost != null)
                {
                    this.RunHost(this.HostableProcess);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while running ApplicationHost.", ex);
            }
        }

        /// <summary>
        /// Handles the LoggerError event from the ApplicationLogger.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">An ErrorEventArgs.</param>
        private void ApplicationLoggerLoggerError(object sender, ErrorEventArgs e)
        {
            Exception exception = e.GetException();

            // When running as a Desktop application we need to display the error because this may be the only way we can see it 
            // since the logger may not be working and logging to the event log could fail as well.
            this.DisplayErrorWhenRunningAsDesktopApplication(exception, "Logger Error");

            if (this.logErrorsFromLogger)
            {
                this.LogErrorToEventLog(exception, "Logger Error");
            }
        }

        /// <summary>
        /// Process unhandled exceptions. 
        /// After this method executes the application is terminated and a .NET Runtime error is logged in the Application logs.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">An UnhandledExceptionEventArgs.</param>
        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            // When running as a Desktop application we need to display the error because this may be the only way we can see it 
            // since the logger may not be working and logging to the event log could fail as well.
            this.DisplayErrorWhenRunningAsDesktopApplication(exception, "Unhandled Exception");

            // Attempt to log to the event log.
            try
            {
                this.LogErrorToEventLog(exception, "Unhandled Exception");
            }
            catch (Exception ex)
            {
                this.DisplayErrorWhenRunningAsDesktopApplication(ex, "Error while logging exception to the EventLog.");
            }

            if (!this.applicationHostInitialized)
            {
                // the application logger is not ready to be used.
                return;
            }

            // avoids handling more errors from the logger.
            this.logErrorsFromLogger = false;
            try
            {
                ApplicationLogger.LogError(exception, "Unhandled Exception.");
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                this.DisplayErrorWhenRunningAsDesktopApplication(ex, "Attempting to log to the Application Logger from UnhandledException.");
            }

            // after this point the application is terminated.
        }

        /// <summary>
        /// Checks that a message can be displayed. Used when running as desktop.
        /// </summary>
        /// <returns>True when a message can be displayed.</returns>
        private bool CanDisplay()
        {
            lock (this.displayLock)
            {
                if (this.displaying)
                {
                    return false;
                }

                this.displaying = true;
                return true;
            }
        }

        /// <summary>
        /// Indicates that the displayed message is closed. Used when running as desktop.
        /// </summary>
        private void DoneDisplaying()
        {
            lock (this.displayLock)
            {
                this.displaying = false;
            }
        }

        /// <summary>
        /// Displays an error in a dialog.
        /// </summary>
        /// <param name="error">An error.</param>
        /// <param name="caption">A text used in the caption.</param>
        private void DisplayErrorWhenRunningAsDesktopApplication(Exception error, string caption)
        {
            if (this.IsDesktopApp)
            {
                if (!this.CanDisplay())
                {
                    return;
                }

                try
                {
                    MessageBox.Show(error.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.DoneDisplaying();
                }
            }
        }

        /// <summary>
        /// Instantiates an instance of T.
        /// </summary>
        /// <typeparam name="T">The type of the <c>HostableProcess</c>.</typeparam>
        /// <returns>An instance of type T.</returns>
        /// <remarks>Exceptions in this method are handled in UnhandledException.</remarks>
        private T InstantiateHostableProcess<T>() where T : HostableProcess, new()
        {
            T hostableProcess = default(T);
            try
            {
                hostableProcess = new T();
                hostableProcess.Init();
                return hostableProcess;
            }
            catch (Exception ex)
            {
                string message = string.Format("Error while instantiating {0}", typeof(T).FullName);
                throw new Exception(message, ex);
            }
        }

        /// <summary>
        /// Executes when the ApplicationHost is closing.
        /// </summary>
        /// <remarks>Exceptions in this method are handled in UnhandledException.</remarks>
        private void EndApplicationHost()
        {
            try
            {
                if (this.Finalizing != null)
                {
                    this.Finalizing();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while finalizing ApplicationHost.", ex);
            }
        }

        /// <summary>
        /// Validates the EventLog source.
        /// </summary>
        /// <param name="logSourceName">An EventLog source.</param>
        private void ValidateAndSetLogSourceName(string logSourceName)
        {
            if (string.IsNullOrWhiteSpace(logSourceName))
            {
                this.LogSourceName = null;
                return;
            }

            try
            {
                bool eventLogSourceFound = EventLog.SourceExists(logSourceName);
                if (!eventLogSourceFound)
                {
                    string message = string.Format(
                        "EventLog source {0} not found. Installing or reinstalling the application may fix the issue.",
                        logSourceName);
                    throw new InvalidOperationException(message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while validating EventLog source. Installing or reinstalling the application may fix the issue.",
                    ex);
            }

            this.LogSourceName = logSourceName;
        }
    }
}
