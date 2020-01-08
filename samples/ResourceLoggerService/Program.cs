using AllWayNet.Applications;
using AllWayNet.Applications.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ResourceLoggerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Starts the ResourceMonitor when the application runs as a Desktop Application.
            // ApplicationHost.AutoStart = true;
            
            // Logs unhandled exceptions to the EventLog.
            ApplicationHost.Run<ResourceMonitor>(ResourceMonitor.LogSourceName);

            // Does not log unhandled exceptions to the EventLog.
            //ApplicationHost.Run<ResourceMonitor>(null);
        }
    }
}
