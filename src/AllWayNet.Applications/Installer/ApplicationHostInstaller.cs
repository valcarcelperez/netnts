namespace AllWayNet.Applications.Installer
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using AllWayNet.Applications.PerformanceCounters;
    using AllWayNet.Logger;

    /// <summary>
    /// Project installer for service applications.
    /// </summary>
    /// <typeparam name="T">The type of the <c>HostableProcess</c>.</typeparam>
    [RunInstaller(false)]
    public partial class ApplicationHostInstaller<T> : System.Configuration.Install.Installer where T : HostableProcess, new()
    {
        /// <summary>
        /// Parameter used to rebuild the performance counters.
        /// </summary>
        private const string ParameterRebuildPerformanceCounters = "/rpc";

        /// <summary>
        /// Indicates whether the installation is for rebuilding just one component of the full installation. e.g. rebuilding the performance counters.
        /// </summary>
        private bool isRebuiltInstallation = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationHostInstaller{T}" /> class.
        /// </summary>
        public ApplicationHostInstaller()
        {
            this.HostableProcess = new T();

            if (this.IsRebuildPerformanceCountersParameterPresent)
            {
                this.isRebuiltInstallation = true;
                return;
            }

            if (this.HostableProcess.InstallerRegisterAsWindowsNTService)
            {
                this.InitializeComponent();
                this.SetServiceProcessInstaller();
                this.SetServiceInstaller();
            }
        }

        /// <summary>
        /// Gets the <c>HostableProcess</c>.
        /// </summary>
        protected T HostableProcess { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the performance counters are being rebuilt.
        /// </summary>
        private bool IsRebuildPerformanceCountersParameterPresent
        {
            get
            {
                return Environment.GetCommandLineArgs().Contains(ParameterRebuildPerformanceCounters);
            }
        }

        /// <summary>
        /// Performs the installation.
        /// </summary>
        /// <param name="stateSaver">An IDictionary used to save information needed to perform a commit, rollback, or uninstall operation.</param>
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            if (this.IsRebuildPerformanceCountersParameterPresent)
            {
                this.Context.LogMessage("Rebuilding Performance Counters.");
                this.CreatePerformanceCounters();
            }

            if (this.isRebuiltInstallation)
            {
                return;
            }

            if (this.HostableProcess.InstallerCreatePerformanceCounters)
            {
                this.CreatePerformanceCounters();
            }

            if (!this.HostableProcess.InstallerRegisterAsWindowsNTService)
            {
                this.CreateEventLogSource();
            }
        }

        /// <summary>
        /// Restores the pre-installation state of the computer.
        /// </summary>
        /// <param name="savedState">An IDictionary that contains the state of the computer after the installation was complete.</param>
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);

            if (this.isRebuiltInstallation)
            {
                return;
            }

            if (this.HostableProcess.InstallerCreatePerformanceCounters)
            {
                this.RemovePerformanceCounters();
            }

            if (!this.HostableProcess.InstallerRegisterAsWindowsNTService)
            {
                this.RemoveEventLogSource();
            }
        }

        /// <summary>
        /// Removes an installation.
        /// </summary>
        /// <param name="savedState">An IDictionary that contains the state of the computer after the installation was complete.</param>
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            if (this.isRebuiltInstallation)
            {
                return;
            }

            if (this.HostableProcess.InstallerCreatePerformanceCounters)
            {
                this.RemovePerformanceCounters();
            }

            if (!this.HostableProcess.InstallerRegisterAsWindowsNTService)
            {
                this.RemoveEventLogSource();
            }
        }

        /// <summary>
        /// Sets the ServiceProcessInstaller.
        /// </summary>
        private void SetServiceProcessInstaller()
        {
            this.ServiceProcessInstaller.Account = this.HostableProcess.InstallerAccount;
            this.ServiceProcessInstaller.Username = this.HostableProcess.InstallerUsername;
            this.ServiceProcessInstaller.Password = this.HostableProcess.InstallerPassword;
        }

        /// <summary>
        /// Sets the ServiceInstaller.
        /// </summary>
        private void SetServiceInstaller()
        {
            this.ServiceInstaller.ServiceName = this.HostableProcess.ServiceName;
            this.ServiceInstaller.DisplayName = this.HostableProcess.InstallerServiceDisplayName;
            this.ServiceInstaller.Description = this.HostableProcess.InstallerServiceDescription;
            this.ServiceInstaller.StartType = this.HostableProcess.InstallerServiceStartType;
        }

        /// <summary>
        /// Creates the performance counters.
        /// </summary>
        private void CreatePerformanceCounters()
        {
            this.RemovePerformanceCounters();

            var list = LoggerPerformanceCounters.GetPerformanceCounters();
            if (this.HostableProcess.CustomCounters != null)
            {
                this.HostableProcess.CustomCounters.ToList().ForEach((item) => list.Add(item));
            }

            string counterNames = string.Join("/", list.Select(a => a.CounterName).ToArray());
            string message = string.Format(
                "Creating performance counters. Category Name: {0}. Counter Names: {1} ...",
                this.HostableProcess.PerformanceCountersCategoryName,
                counterNames);
            this.Context.LogMessage(message);
            ApplicationPerformanceCountersInstaller.CreatePerformanceCounters(
                this.HostableProcess.PerformanceCountersCategoryName,
                this.HostableProcess.PerformanceCountersCategoryHelp,
                PerformanceCounterCategoryType.MultiInstance,
                list);
        }

        /// <summary>
        /// Remove performance counters.
        /// </summary>
        private void RemovePerformanceCounters()
        {
            if (!PerformanceCounterCategory.Exists(this.HostableProcess.PerformanceCountersCategoryName))
            {
                return;
            }

            string message = string.Format("Removing performance counters. Category Name:  '{0}'...", this.HostableProcess.PerformanceCountersCategoryName);
            this.Context.LogMessage(message);
            ApplicationPerformanceCountersInstaller.RemovePerformanceCounters(this.HostableProcess.PerformanceCountersCategoryName);
        }

        /// <summary>
        /// Creates an EventLog source.
        /// </summary>
        private void CreateEventLogSource()
        {
            bool logExists = EventLog.SourceExists(this.HostableProcess.ServiceName);
            string logName = "Application";
            string message;
            if (logExists)
            {
                message = string.Format("EventLog source {0} is already registered in the local computer.", this.HostableProcess.ServiceName);
                this.Context.LogMessage(message);
            }
            else
            {
                message = string.Format("Creating EventLog source {0} in the log {1}...", this.HostableProcess.ServiceName, logName);
                this.Context.LogMessage(message);
                EventLog.CreateEventSource(this.HostableProcess.ServiceName, logName);
            }
        }

        /// <summary>
        /// Removes an EventLog source.
        /// </summary>
        private void RemoveEventLogSource()
        {
            bool logExists = EventLog.SourceExists(this.HostableProcess.ServiceName);
            if (!logExists)
            {
                return;
            }

            string message = string.Format("Removing EventLog source {0}...", this.HostableProcess.ServiceName);
            this.Context.LogMessage(message);
            EventLog.DeleteEventSource(this.HostableProcess.ServiceName);
        }
    }
}
