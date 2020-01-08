namespace AllWayNet.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceProcess;
    using AllWayNet.Logger;

    /// <summary>
    /// Provides a base class for <c>hostable</c> process that are hosted by an ApplicationHost.
    /// </summary>
    public abstract class HostableProcess : IDisposable
    {
        /// <summary>
        /// Indicates the current status of this <c>HostableProcess</c>.
        /// </summary>
        private HostableProcessStatus status = HostableProcessStatus.Stopped;
        
        /// <summary>
        /// Object used for synchronization.
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="HostableProcess" /> class.
        /// </summary>
        public HostableProcess()
        {
            this.CanStop = true;
            this.InstallerAccount = ServiceAccount.LocalSystem;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HostableProcess" /> class.
        /// </summary>
        ~HostableProcess()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event raised when the status changes.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Gets the Status.
        /// </summary>
        public HostableProcessStatus Status
        {
            get
            {
                lock (this.lockObj)
                {
                    return this.status;
                }
            }

            private set
            {
                lock (this.lockObj)
                {
                    if (this.status == value)
                    {
                        return;
                    }

                    this.status = value;
                }

                this.OnStatusChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the name of this instance of the application. Identify the application in the performance counters.
        /// </summary>
        public string ApplicationInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the PerformanceCountersCategoryName.
        /// </summary>
        public string PerformanceCountersCategoryName { get; protected set; }

        /// <summary>
        /// Gets or sets the PerformanceCountersCategoryHelp.
        /// </summary>
        public string PerformanceCountersCategoryHelp { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether performance counters must be created during the installation.
        /// </summary>
        public bool InstallerCreatePerformanceCounters { get; protected set; }

        /// <summary>
        /// Gets or sets the InstallerAccount. Used when registering the application as a Widows NT Service.
        /// </summary>
        public ServiceAccount InstallerAccount { get; protected set; }

        /// <summary>
        /// Gets or sets the InstallerUsername. Used when registering the application as a Widows NT Service.
        /// </summary>
        public string InstallerUsername { get; protected set; }

        /// <summary>
        /// Gets or sets the InstallerPassword. Used when registering the application as a Widows NT Service.
        /// </summary>
        public string InstallerPassword { get; protected set; }

        /// <summary>
        /// Gets or sets the InstallerServiceDisplayName. Used when registering the Windows NT Service.
        /// </summary>
        public string InstallerServiceDisplayName { get; protected set; }

        /// <summary>
        /// Gets or sets the InstallerServiceDescription. Used when registering the Windows NT Service.
        /// </summary>
        public string InstallerServiceDescription { get; protected set; }

        /// <summary>
        /// Gets or sets the InstallerServiceStartType. Used when registering the Windows NT Service.
        /// </summary>
        public ServiceStartMode InstallerServiceStartType { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ApplicationHost and ApplicationLogger can update the performance counters.
        /// At this point there are Performance Counters updated by the ApplicationLogger only.
        /// </summary>
        public bool UpdatePerformanceCounters { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the installer must register the application as a Windows NT Service.
        /// </summary>
        public bool InstallerRegisterAsWindowsNTService { get; protected set; }

        /// <summary>
        /// Gets or sets the CustomCounters.
        /// </summary>
        public IList<CounterCreationData> CustomCounters { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether to report Start, Stop, Pause, and Continue commands in the event log.
        /// </summary>
        public bool AutoLog { get; set; }

        /// <summary>
        /// Gets or sets the short name used to identify the service to the system.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the ServiceDisplayName.
        /// </summary>
        public string ServiceDisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service can handle notifications of computer power status changes.
        /// </summary>
        public bool CanHandlePowerEvent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service can be paused and resumed.
        /// </summary>
        public bool CanPauseAndContinue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service should be notified when the system is shutting down.
        /// </summary>
        public bool CanShutdown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service can be stopped once it has started.
        /// </summary>
        public bool CanStop { get; set; }

        /// <summary>
        /// Initializes the <c>HostableProcess</c>.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the <c>HostableProcess</c>.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        public virtual void Start(string[] args)
        {
            this.Status = HostableProcessStatus.Running;
            if (ApplicationHost.Instance.IsDesktopApp)
            {
                ApplicationLogger.LogInfo("{0} started. Running as Desktop Application.", this.ServiceName);
            }
            else
            {
                ApplicationLogger.LogInfo("{0} started. Running as Windows Service Application.", this.ServiceName);
            }
        }

        /// <summary>
        /// Stops the <c>HostableProcess</c>.
        /// </summary>
        public virtual void Stop()
        {
            this.Status = HostableProcessStatus.Stopped;
            ApplicationLogger.LogInfo("{0} stopped.", this.ServiceName);
        }

        /// <summary>
        /// Resumes the <c>HostableProcess</c> when paused.
        /// </summary>
        public virtual void Continue()
        {
            this.Status = HostableProcessStatus.Running;
            ApplicationLogger.LogInfo("{0} resumed.", this.ServiceName);
        }

        /// <summary>
        /// Pauses the <c>HostableProcess</c>.
        /// </summary>
        public virtual void Pause()
        {
            this.Status = HostableProcessStatus.Paused;
            ApplicationLogger.LogInfo("{0} paused.", this.ServiceName);
        }

        /// <summary>
        /// Executes when the computer's power status has changed.
        /// </summary>
        /// <param name="powerStatus">A PowerBroadcastStatus that indicates a notification from the system about its power status.</param>
        /// <returns>False when rejecting a query.</returns>
        public virtual bool PowerEvent(PowerBroadcastStatus powerStatus)
        {
            return true;
        }

        /// <summary>
        /// Executes when the system is shutting down.
        /// </summary>
        public virtual void Shutdown()
        {
        }

        /// <summary>
        /// Part of disposing mechanism.
        /// </summary>
        /// <param name="disposing">True indicates that Dispose is being called from a user's code.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Executes when the Status had changed.
        /// </summary>
        /// <param name="e">An EventArgs.</param>
        protected virtual void OnStatusChanged(EventArgs e)
        {
            EventHandler handler = this.StatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
