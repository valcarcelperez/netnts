namespace NETWAFService
{
    using AllWayNet.Applications;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Starts the MainProcess when the application runs as a Desktop Application.
            // ApplicationHost.AutoStart = true;

            // Logs unhandled exceptions to the EventLog.
            ApplicationHost.Run<MainProcess>(MainProcess.LogSourceName);

            // Does not log unhandled exceptions to the EventLog.
            //ApplicationHost.Run<MainProcess>(null);
        }
    }
}
