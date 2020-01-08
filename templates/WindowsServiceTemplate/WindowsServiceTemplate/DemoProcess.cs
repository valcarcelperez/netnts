namespace NETWAFService
{
    using System;
    using System.Diagnostics;
    using System.Timers;
    using AllWayNet.Logger;

    /// <summary>
    /// Defines a demo process that uses a timer and logs resources from the system.
    /// TODO: Remove this class.
    /// </summary>
    public class DemoProcess
    {
        private Timer timer = null;
        private PerformanceCounter processorTimeCounter;
        private PerformanceCounter ramCounter;

        public DemoProcess(int interval)
        {
            this.processorTimeCounter = new PerformanceCounter();
            this.processorTimeCounter.CategoryName = "Processor Information";
            this.processorTimeCounter.CounterName = "% Processor Time";
            this.processorTimeCounter.InstanceName = "_Total";

            this.ramCounter = new PerformanceCounter();
            this.ramCounter.CategoryName = "Memory";
            this.ramCounter.CounterName = "Available MBytes";

            this.timer = new Timer(interval);
            this.timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
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
            float processorTime = this.processorTimeCounter.NextValue() / 100;
            float availableMemory = this.ramCounter.NextValue() / 1000;
            ApplicationLogger.LogInfo("Processor Time: {0}, Available Memory: {1} GB", processorTime.ToString("P"), availableMemory.ToString("N"));
        }
    }
}
