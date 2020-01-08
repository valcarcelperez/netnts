namespace AllWayNet.Applications.WinForms
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using AllWayNet.Logger;

    /// <summary>
    /// Defines a UserControl for controlling a <c>HostableProcess</c>.
    /// </summary>
    public partial class ControlPanel : UserControl
    {
        /// <summary>
        /// The <c>HostableProcess</c> being controlled.
        /// </summary>
        private HostableProcess hostableProcess;
        
        /// <summary>
        /// Model of this control.
        /// </summary>
        private ControlPanelModel controlPanelModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlPanel" /> class.
        /// </summary>
        public ControlPanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Sets the <c>HostableProcess</c> that will be controlled.
        /// </summary>
        /// <param name="hostableProcess">A <c>HostableProcess</c>.</param>
        /// <param name="args">Command line arguments.</param>
        public void SetHostableProcess(HostableProcess hostableProcess, string[] args)
        {
            this.hostableProcess = hostableProcess;
            this.controlPanelModel = new ControlPanelModel(this.hostableProcess, args);
            this.hostableProcess.StatusChanged += this.HostableProcessStatusChanged;
            this.timer.Enabled = WinFormHost.ControlPanelEnabled;
            this.controlPanelModel.CanPauseAndContinue = this.hostableProcess.CanPauseAndContinue;
            this.RefreshStatusInControlPanel();
        }

        /// <summary>
        /// Handles StatusChanged events raised by <c>HostableProcess</c>.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">An EventArgs.</param>
        private void HostableProcessStatusChanged(object sender, EventArgs e)
        {
            this.ExecuteInMainThread(this.RefreshStatusInControlPanel);
        }

        /// <summary>
        /// Refreshes the status in the control panel.
        /// </summary>
        private void RefreshStatusInControlPanel()
        {
            this.controlPanelModel.SetStatus(this.hostableProcess.Status);
            this.SetControlsWithStatusFromModel();
        }

        /// <summary>
        /// Handles the timer event and refresh the controls.
        /// </summary>
        /// <param name="sender">Object raising the event.</param>
        /// <param name="e">An EventArgs.</param>
        private void TimerTick(object sender, EventArgs e)
        {
            this.controlPanelModel.SetWarningCount(ApplicationLogger.WarningCount);
            this.controlPanelModel.SetErrorCount(ApplicationLogger.ErrorCount);
            this.SetControlsWithCounters();
        }

        /// <summary>
        /// Disable all the buttons in the control.
        /// </summary>
        private void DisableButtons()
        {
            this.startStopButton.Enabled = false;
            this.pauseResumeButton.Enabled = false;
        }

        /// <summary>
        /// Sets the control according to the status of the model.
        /// </summary>
        private void SetControlsWithStatusFromModel()
        {
            Action action = () =>
            {
                this.startStopButton.Enabled = this.controlPanelModel.StartStopEnabled;
                this.startStopButton.Text = this.controlPanelModel.StartStopText;
                this.pauseResumeButton.Enabled = this.controlPanelModel.PauseResumeEnabled;
                this.pauseResumeButton.Text = this.controlPanelModel.PauseResumeText;
                this.statusLabel.Text = this.controlPanelModel.StatusLabelText;
            };

            this.ExecuteInMainThread(action);
        }

        /// <summary>
        /// Sets the controls with the counters information.
        /// </summary>
        private void SetControlsWithCounters()
        {
            Action action = () =>
            {
                this.warningsLabel.Text = this.controlPanelModel.WarningCountLabelText;
                this.errorsLabel.Text = this.controlPanelModel.ErrorCountLabelText;
            };

            this.ExecuteInMainThread(action);
        }

        /// <summary>
        /// Handles the Click event from the Pause/Resume button.
        /// </summary>
        /// <param name="sender">Object raising the event.</param>
        /// <param name="e">An EventArgs.</param>
        private void PauseResumeButtonClick(object sender, EventArgs e)
        {
            // Disable the buttons to avoid another Clink event while the event handler is executing.
            this.DisableButtons();

            Action action = () =>
            {
                try
                {
                    this.controlPanelModel.PauseContinueHostableProcess();
                }
                finally
                {
                    this.SetControlsWithStatusFromModel();
                }
            };

            // Schedule the action in a separate thread to keep the form responsive if the call to Pause/Resume takes a long time.
            this.ScheduleAction(action, "Pause/Resume");
        }

        /// <summary>
        /// Handles the Click event from the Start/Stop button.
        /// </summary>
        /// <param name="sender">Object raising the event.</param>
        /// <param name="e">An EventArgs.</param>
        private void StartStopButtonClick(object sender, EventArgs e)
        {
            // Disable the buttons to avoid another Clink event while the event handler is executing.
            this.DisableButtons();

            Action action = () =>
            {
                try
                {
                    this.controlPanelModel.StartStopHostableProcess();
                }
                finally
                {
                    this.SetControlsWithStatusFromModel();
                }
            };

            // Schedule the action in a separate thread to keep the form responsive if the call to Start/Stop takes a long time.
            this.ScheduleAction(action, "Start/Stop");
        }

        /// <summary>
        /// Schedule an Action to be executed in another thread.
        /// </summary>
        /// <param name="action">A Action.</param>
        /// <param name="name">The name of the action.</param>
        private void ScheduleAction(Action action, string name)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    this.DisplayErrorInMainThread(ex, name);
                }
            });
        }

        /// <summary>
        /// Display an error dialog in the main thread.
        /// </summary>
        /// <param name="error">The error to be displayed.</param>
        /// <param name="caption">Text used in the caption.</param>
        private void DisplayErrorInMainThread(Exception error, string caption)
        {
            Action action  = () =>
            {
                MessageBox.Show(error.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            this.ExecuteInMainThread(action);
        }

        /// <summary>
        /// Executes an Action in the main thread.
        /// </summary>
        /// <param name="action">An Action.</param>
        private void ExecuteInMainThread(Action action)
        {
            MethodInvoker mi = (MethodInvoker)delegate
            {
                action();
            };

            if (this.InvokeRequired)
            {
                this.Invoke(mi);
            }
            else
            {
                mi.Invoke();
            }
        }

        /// <summary>
        /// Model used by the control.
        /// </summary>
        private class ControlPanelModel
        {
            /// <summary>
            /// <c>HostableProcess</c> controlled by the control.
            /// </summary>
            private HostableProcess hostableProcess;

            /// <summary>
            /// Command line arguments.
            /// </summary>
            private string[] args;

            /// <summary>
            /// Initializes a new instance of the <see cref="ControlPanelModel" /> class.
            /// </summary>
            /// <param name="hostableProcess"><c>HostableProcess</c> controlled by the control.</param>
            /// <param name="args">Command line arguments.</param>
            public ControlPanelModel(HostableProcess hostableProcess, string[] args)
            {
                this.hostableProcess = hostableProcess;
                this.args = args;
                this.StartStopEnabled = true;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the <c>HostableProcess</c> supports Pause and Resume.
            /// </summary>
            public bool CanPauseAndContinue { get; set; }

            /// <summary>
            /// Gets a value indicating whether the Pause/Resume button is enabled.
            /// </summary>
            public bool PauseResumeEnabled { get; private set; }
            
            /// <summary>
            /// Gets the text for the Pause/Resume button.
            /// </summary>
            public string PauseResumeText { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the Start/Stop button is enabled.
            /// </summary>
            public bool StartStopEnabled { get; private set; }
            
            /// <summary>
            /// Gets the text for the Start/Stop button.
            /// </summary>
            public string StartStopText { get; private set; }

            /// <summary>
            /// Gets the text used for the status label.
            /// </summary>
            public string StatusLabelText { get; private set; }
            
            /// <summary>
            /// Gets the text used for the warning count label.
            /// </summary>
            public string WarningCountLabelText { get; private set; }
            
            /// <summary>
            /// Gets the text used for the error count label.
            /// </summary>
            public string ErrorCountLabelText { get; private set; }

            /// <summary>
            /// Sets the status.
            /// </summary>
            /// <param name="status">A <c>HostableProcessStatus</c>.</param>
            public void SetStatus(HostableProcessStatus status)
            {
                this.StatusLabelText = string.Format("Status: {0}", status);

                switch (status)
                {
                    case HostableProcessStatus.Stopped:
                        this.StartStopText = "Start";
                        this.PauseResumeText = "Resume";
                        this.PauseResumeEnabled = false;
                        break;
                    case HostableProcessStatus.Running:
                        this.StartStopText = "Stop";
                        this.PauseResumeText = "Pause";
                        this.PauseResumeEnabled = this.CanPauseAndContinue;
                        break;
                    case HostableProcessStatus.Paused:
                        this.PauseResumeText = "Resume";
                        break;
                }
            }

            /// <summary>
            /// Sets the error count.
            /// </summary>
            /// <param name="count">The number of errors.</param>
            public void SetErrorCount(long count)
            {
                this.ErrorCountLabelText = string.Format("Errors: {0}", count);
            }

            /// <summary>
            /// Sets the warning count.
            /// </summary>
            /// <param name="count">The number of warnings.</param>
            public void SetWarningCount(long count)
            {
                this.WarningCountLabelText = string.Format("Warnings: {0}", count);
            }

            /// <summary>
            /// Starts or stops the <c>HostableProcess</c>.
            /// </summary>
            public void StartStopHostableProcess()
            {
                switch (this.hostableProcess.Status)
                {
                    case HostableProcessStatus.Stopped:
                        this.hostableProcess.Start(this.args);
                        break;
                    case HostableProcessStatus.Running:
                    case HostableProcessStatus.Paused:
                        this.hostableProcess.Stop();
                        break;
                }
            }

            /// <summary>
            /// Pauses or resumes the <c>HostableProcess</c>.
            /// </summary>
            public void PauseContinueHostableProcess()
            {
                switch (this.hostableProcess.Status)
                {
                    case HostableProcessStatus.Running:
                        this.hostableProcess.Pause();
                        break;
                    case HostableProcessStatus.Paused:
                        this.hostableProcess.Continue();
                        break;
                }
            }
        }
    }
}
