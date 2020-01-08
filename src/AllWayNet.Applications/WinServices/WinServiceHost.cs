namespace AllWayNet.Applications.WinServices
{
    using System;
    using System.ServiceProcess;

    /// <summary>
    /// Windows Service that hosts a <c>HostableProcess</c>.
    /// </summary>
    public class WinServiceHost : ServiceBase
    {
        /// <summary>
        /// <c>HostableProcess</c> being hosted.
        /// </summary>
        private HostableProcess hostableProcess = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinServiceHost" /> class.
        /// </summary>
        /// <param name="hostableProcess"><c>HostableProcess</c> to be hosted.</param>
        public WinServiceHost(HostableProcess hostableProcess)
        {
            this.hostableProcess = hostableProcess;
            this.ServiceName = hostableProcess.ServiceName;
            this.AutoLog = hostableProcess.AutoLog;
            this.CanHandlePowerEvent = hostableProcess.CanHandlePowerEvent;
            this.CanPauseAndContinue = hostableProcess.CanPauseAndContinue;
            this.CanShutdown = hostableProcess.CanShutdown;
            this.CanStop = hostableProcess.CanStop;
        }

        /// <summary>
        /// Begins running the Windows NT Service application.
        /// </summary>
        /// <param name="hostableProcess">A <c>HostableProcess</c></param>
        public static void RunHost(HostableProcess hostableProcess)
        {
            WinServiceHost winService = new WinServiceHost(hostableProcess);
            ApplicationHost.Instance.Host = winService;
            ServiceBase[] servicesToRun = new ServiceBase[] { winService };
            ServiceBase.Run(servicesToRun);
        }

        /// <summary>
        /// Starts the <c>HostableProcess</c>.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                this.hostableProcess.Start(args);
            }
            catch (Exception ex)
            {
                ApplicationHost.Terminate(1, ex.ToString());
            }            
        }

        /// <summary>
        /// Stops the <c>HostableProcess</c>.
        /// </summary>
        protected override void OnStop()
        {
            this.hostableProcess.Stop();
        }

        /// <summary>
        /// Pauses the <c>HostableProcess</c> when running.
        /// </summary>
        protected override void OnPause()
        {
            this.hostableProcess.Pause();
        }

        /// <summary>
        /// Resumes the <c>HostableProcess</c> when paused.
        /// </summary>
        protected override void OnContinue()
        {
            this.hostableProcess.Continue();
        }

        /// <summary>
        /// Calls PowerEvent in <c>HostableProcess</c>.
        /// </summary>
        /// <param name="powerStatus">A PowerBroadcastStatus that indicates a notification from the system about its power status.</param>
        /// <returns>False if the application rejected the query passed.</returns>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return this.hostableProcess.PowerEvent(powerStatus);
        }

        /// <summary>
        /// Calls Shutdown in <c>HostableProcess</c>.
        /// </summary>
        protected override void OnShutdown()
        {
            this.hostableProcess.Shutdown();
        }
    }
}
