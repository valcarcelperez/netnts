namespace AllWayNet.Applications.WinForms
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using AllWayNet.Logger;

    /// <summary>
    /// Form that hosts a <c>HostableProcess</c>.
    /// </summary>
    public partial class WinFormHost : Form, ILoggerProcessor
    {
        /// <summary>
        /// <c>HostableProcess</c> being hosted.
        /// </summary>
        private HostableProcess hostableProcess;

        /// <summary>
        /// Indicates whether to start the process when the form is loaded.
        /// </summary>
        private bool startOnLoad;

        /// <summary>
        /// Command line arguments.
        /// </summary>
        private string[] args;

        /// <summary>
        /// Initializes static members of the <see cref="WinFormHost" /> class.
        /// </summary>
        static WinFormHost()
        {
            MaxSize = 100 * 1024;
            DisplayControlPanel = true;
            ControlPanelEnabled = true;
            DisplayLogsPanel = true;
            LogsPanelEnabled = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormHost" /> class.
        /// </summary>
        /// <param name="hostableProcess">A <c>HostableProcess</c></param>
        /// <param name="args">Command line arguments.</param>
        public WinFormHost(HostableProcess hostableProcess, string[] args)
        {
            this.InitializeComponent();
            this.logsPanel.MaxSize = MaxSize;
            this.hostableProcess = hostableProcess;
            this.controlPanel.Visible = DisplayControlPanel;
            this.controlPanel.Enabled = ControlPanelEnabled;
            this.splitContainer.Panel1Collapsed = !DisplayCustonPanel;
            this.splitContainer.Panel2Collapsed = !DisplayLogsPanel;
            this.controlPanel.SetHostableProcess(this.hostableProcess, args);
            this.Text = hostableProcess.ServiceDisplayName;
            this.args = args;

            // When the ApplicationHost runs as a Desktop the hostable process will start automatically when AutoStart was set to true 
            // or when it is indicated in the command line arguments.
            this.startOnLoad = ApplicationHost.AutoStart;
        }

#pragma warning disable 67 // This form implements the ILoggerProcessor but will not raise the Error event.
        /// <summary>
        /// Event raised when an error occurs.
        /// </summary>
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.DocumentationRules",
            "SA1600:ElementsMustBeDocumented",
            Justification = "Required for ILoggerProcessor but not used.")]
        public event EventHandler<ErrorEventArgs> Error;
#pragma warning restore 67

        /// <summary>
        /// Gets or sets the maximum size of the text in the panel.
        /// </summary>
        public static int MaxSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Control Panel must be displayed.
        /// </summary>
        public static bool DisplayControlPanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Control Panel must be enabled.
        /// </summary>
        public static bool ControlPanelEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Custom Panel must be displayed.
        /// </summary>
        public static bool DisplayCustonPanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Custom Panel must be enabled.
        /// </summary>
        public static bool DisplayLogsPanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Logs Panel must be enabled.
        /// </summary>
        public static bool LogsPanelEnabled { get; set; }

        /// <summary>
        /// Begins running the Desktop application.
        /// </summary>
        /// <param name="hostableProcess">A <c>HostableProcess</c></param>
        /// <param name="args">Command line arguments.</param>
        public static void RunHost(HostableProcess hostableProcess, string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += ApplicationThreadException;

            using (WinFormHost form = new WinFormHost(hostableProcess, args))
            {
                ApplicationHost.Instance.Host = form;
                if (LogsPanelEnabled)
                {
                    ApplicationLogger.AddLoggerProcessor(form);
                    Application.Run(form);
                    ApplicationLogger.RemoveLoggerProcessor(form);
                }
                else
                {
                    Application.Run(form);
                }
            }
        }

        /// <summary>
        /// Displays a Log in the Log Panel.
        /// </summary>
        /// <param name="log">A LogItem.</param>
        public void Log(LogItem log)
        {
            this.logsPanel.Log(log);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.DocumentationRules",
            "SA1600:ElementsMustBeDocumented",
            Justification = "Required for ILoggerProcessor. Not used.")]
        public void Prepare(XElement xml)
        {
        }

        /// <summary>
        /// Handles the ThreadException event.
        /// </summary>
        /// <param name="sender">Object raising the event.</param>
        /// <param name="e">A ThreadExceptionEventArgs.</param>
        private static void ApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ApplicationLogger.LogError(e.Exception, "Unhandled UI Thread Exception.");
        }

        /// <summary>
        /// Handles the Load event.
        /// </summary>
        /// <param name="sender">Object raising the event.</param>
        /// <param name="e">An EventArgs.</param>
        private void WinFormHostLoad(object sender, EventArgs e)
        {
            if (this.startOnLoad)
            {
                this.hostableProcess.Start(this.args);
            }
        }

        /// <summary>
        /// Handles the FormClosing event.
        /// </summary>
        /// <param name="sender">Object raising the event.</param>
        /// <param name="e">A FormClosingEventArgs.</param>
        private void WinFormHostFormClosing(object sender, FormClosingEventArgs e)
        {
            bool canClose = MessageBox.Show("Do you want to close the application?", "Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            if (canClose)
            {
                e.Cancel = false;
                if (this.hostableProcess.Status != HostableProcessStatus.Stopped)
                {
                    this.hostableProcess.Stop();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
