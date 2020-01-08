namespace NETWAFService
{
    using System;
    using System.ServiceProcess;
    using AllWayNet.Applications;

    public class MainProcess : HostableProcess
    {
        // TODO: Set the Log Source Name used by the service when logging events to the Application Logs. 
        // TODO: The same name must be used for the source attribute in the App.config file: <implementer name="EventLogLogger" ... source="NETWAFService">
        public const string LogSourceName = "NETWAFService";

        // TODO: Replace this process with you own processes.
        private DemoProcess demoProcess;

        public MainProcess()
        {
            // This section sets properties of the Windows Service.
            // TODO: Set the service name and service display name.
            this.ServiceName = "NETWAFService";
            this.ServiceDisplayName = "NETWAFService";

            // This section sets properties used for registering the service during installation.
            // TODO: Set the service description.
            this.InstallerRegisterAsWindowsNTService = true; // True to register the application as a Windows Service during the installation.
            this.InstallerServiceDisplayName = this.ServiceDisplayName;
            this.InstallerServiceDescription = "NETWAFService Application";
            this.InstallerServiceStartType = ServiceStartMode.Automatic;

            // This section sets properties used for creating the performance counters used by the application.
            this.InstallerCreatePerformanceCounters = true;
            this.ApplicationInstanceName = this.ServiceName;
            this.PerformanceCountersCategoryName = this.InstallerServiceDisplayName;
            this.PerformanceCountersCategoryHelp = string.Format("Counters for {0}.", this.PerformanceCountersCategoryName);

            // TODO: Set custom Performance Counters that need to be created by the installer.
            //this.CustomCounters = new System.Collections.Generic.List<System.Diagnostics.CounterCreationData>();
            //this.CustomCounters.Add(new System.Diagnostics.CounterCreationData("MyCounterName", "MyCounterHelp", System.Diagnostics.PerformanceCounterType.NumberOfItems32));
        }

        /// <summary>
        /// This method is called by NETWAF after this class is instantiated by the ApplicationHost. It is not called by the installer.
        /// </summary>
        public override void Init()
        {
            this.UpdatePerformanceCounters = true; // True enables the application logger to update the performance counters.

            // Windows Service settings
            this.CanShutdown = true; // Indicates whether the service should be notified when the system is shutting down.
            this.CanStop = true; // Indicates whether the service can be stopped once it has started.
            //this.CanPauseAndContinue = true;
            //this.CanHandlePowerEvent = true;

            // TODO: Replace this demo process by code that initializes objects used by the service.            
            this.demoProcess = new DemoProcess(2000);
        }

        /// <summary>
        /// Executes when the service starts.
        /// </summary>
        /// <param name="args"></param>
        public override void Start(string[] args)
        {
            try
            {
                // Loads the logger configuration.
                ApplicationHost.LogConfiguration();

                // TODO: Start your processes here.
                this.demoProcess.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("Initialization Error", ex);
            }

            // Updates the status and generate an event.
            base.Start(args);
        }

        /// <summary>
        /// Executes when the service stops.
        /// </summary>
        public override void Stop()
        {
            // TODO: Stop your processes here.
            this.demoProcess.Stop();

            // Updates the status and generate an event.
            base.Stop();
        }

        /// <summary>
        /// Executes when the system is shutting down. CanShutdown must be true for this method to be called.
        /// </summary>
        public override void Shutdown()
        {
            // TODO: Stop your processes here.
            this.demoProcess.Stop();

            // Updates the status and generate an event.
            base.Shutdown();
        }

        #region less common scenarios

        /// <summary>
        /// Executes when a paused service is resumed. CanPauseAndContinue must be true to allow the service to be paused.
        /// </summary>
        public override void Continue()
        {
            // TODO: Resume your process.

            // Updates the status and generate an event.
            base.Continue();
        }

        /// <summary>
        /// Executes when the service is paused. CanPauseAndContinue must be true to allow the service to be paused.
        /// </summary>
        public override void Pause()
        {
            // TODO: Pause your process.

            // Updates the status and generate an event.
            base.Pause();
        }

        /// <summary>
        /// Executes when the computer's power status has changed. CanHandlePowerEvent must be true for this method to be called.
        /// </summary>
        /// <param name="powerStatus"></param>
        /// <returns></returns>
        public override bool PowerEvent(PowerBroadcastStatus powerStatus)
        {
            //TODO: Decide whether the service accepts or rejects the power status being passed.
            return true;
        }

        #endregion
    }
}
