namespace ResourceLoggerService
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using AllWayNet.Applications;
    using AllWayNet.Logger;

    public class ResourceMonitor : HostableProcess
    {
        public const string LogSourceName = "ResourceLoggerService";

        private int interval;
        private System.Timers.Timer timer = null;
        private PerformanceCounter processorTimeCounter;
        private PerformanceCounter ramCounter;

        /// <summary>
        /// Track whether Dispose has been called.
        /// </summary>
        private bool disposed = false;

        public ResourceMonitor()
        {
            if (string.IsNullOrWhiteSpace(ApplicationHost.CommandLineInstanceName))
            {
                this.ServiceName = "ResourceLoggerService";
            }
            else
            {
                this.ServiceName = ApplicationHost.CommandLineInstanceName;
            }

            this.InstallerRegisterAsWindowsNTService = true;
            this.InstallerServiceDisplayName = "NETWAF Demo - Resource Logger Service";
            this.InstallerServiceDescription = "NETWAF Demo - Application that logs CPU and Memory usage.";
            this.InstallerServiceStartType = ServiceStartMode.Manual;

            this.ApplicationInstanceName = this.ServiceName;
            this.PerformanceCountersCategoryName = InstallerServiceDisplayName;
            this.PerformanceCountersCategoryHelp = string.Format("Counters for {0}.", this.PerformanceCountersCategoryName);
            this.InstallerCreatePerformanceCounters = true;
            this.UpdatePerformanceCounters = true;

            // enable performance counters.
            //LoggerPerformanceCounters.StandardCountersEnabled = false;
            //LoggerPerformanceCounters.CountersPerMinuteEnabled = false;
            //LoggerPerformanceCounters.DeltaCountersEnabled = false;
        }

        public override void Init()
        {
            this.ServiceDisplayName = InstallerServiceDisplayName;
            this.interval = int.Parse(ConfigurationManager.AppSettings["interval"]);
            this.timer = new System.Timers.Timer(this.interval);
            this.timer.Elapsed += TimerElapsed;

            this.processorTimeCounter = new PerformanceCounter();
            this.processorTimeCounter.CategoryName = "Processor Information";
            this.processorTimeCounter.CounterName = "% Processor Time";
            this.processorTimeCounter.InstanceName = "_Total";

            this.ramCounter = new PerformanceCounter();
            this.ramCounter.CategoryName = "Memory";
            this.ramCounter.CounterName = "Available MBytes";

            this.CanPauseAndContinue = true;
        }

        public override void Start(string[] args)
        {
            try
            {
                // test an initialization error
                // throw new Exception("test-test");

                ApplicationHost.LogConfiguration();
                this.LogConfiguration();
                this.timer.Start();

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(3000);
                    ApplicationLogger.LogError("Demo - logging error #1.");
                    Thread.Sleep(3000);
                    ApplicationLogger.LogError("Demo - logging error #2.");

                    Thread.Sleep(3000);
                    ApplicationLogger.LogWarning("Demo - logging warning #1.");
                    Thread.Sleep(3000);
                    ApplicationLogger.LogWarning("Demo - logging warning #2.");
                });

                // Demonstrates a unhandled exception.
                //new Thread(() =>
                //{
                //    int exceptionDelay = 20000;
                //    Thread.Sleep(exceptionDelay);
                //    ApplicationLogger.LogInfo("Demo - An unhandled exception is about to be thrown from this thread.");
                //    throw new Exception(string.Format("Demo - This is a demo of unhandled exception. It is raised {0} ms after the process starts. To disable it commented the code in ResourceMonitor.Start()", exceptionDelay));
                //}).Start();
            }
            catch (Exception ex)
            {
                throw new Exception("Initialization Error", ex);
            }

            // Updates the status and generate an event.
            base.Start(args);
        }

        public override void Stop()
        {
            this.timer.Stop();

            // Updates the status and generate an event.
            base.Stop();
        }

        /// <summary>
        /// Part of disposing mechanism.
        /// </summary>
        /// <param name="disposing">True indicates that Dispose is being called from a user's code.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.timer.Dispose();
                    this.processorTimeCounter.Close();
                    this.ramCounter.Close();
                }

                disposed = true;
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
           try
           {
              this.LogResources();
           }
           catch (Exception ex)
           {
              ApplicationLogger.LogError(ex, "Error in LogResources.");
           }
        }

        private void LogResources()
        {
            if (this.Status == HostableProcessStatus.Paused)
            {
                ApplicationLogger.LogWarning("********** Paused **********");
                //ApplicationLogger.LogError("********** Paused **********");
                return;
            }

            float processorTime = this.processorTimeCounter.NextValue() / 100;
            float availableMemory = this.ramCounter.NextValue() / 1000;
            ApplicationLogger.LogInfo("Processor Time: {0}, Available Memory: {1} GB", processorTime.ToString("P"), availableMemory.ToString("N"));
        }

        private void LogConfiguration()
        {
            ApplicationLogger.LogInfo("{0} Configuration. \r\nConfiguration: Interval: {1}", this.ServiceName, this.interval);
        }
    }
}
