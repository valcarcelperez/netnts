namespace AllWayNet.Applications.WinForms
{
    using System;
    using System.Collections.Concurrent;
    using System.Windows.Forms;
    using AllWayNet.Logger;

    /// <summary>
    /// Defines a Panel control to display logs.
    /// </summary>
    public partial class LogsPanel : UserControl
    {
        /// <summary>
        /// Default value for MaxSize.
        /// </summary>
        private const int DefaultMaxSize = 100 * 1024;

        /// <summary>
        /// Text used as log delimiter.
        /// </summary>
        private const string LogDelimiter = "--------------------";

        /// <summary>
        /// Text used as template when formatting a log.
        /// </summary>
        private const string Template = "\r\n--------------------\r\nDateTime: #DateTime ThreadId: #ThreadId Type: #Type\r\nDescription:\r\n#Description\r\n#CustomLogItem";

        /// <summary>
        /// Queue of logs that are ready to be displayed.
        /// </summary>
        private ConcurrentQueue<LogItem> logsQueue = new ConcurrentQueue<LogItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LogsPanel" /> class.
        /// </summary>
        public LogsPanel()
        {
            this.InitializeComponent();
            this.MaxSize = DefaultMaxSize;
            this.timer.Start();
        }

        /// <summary>
        /// Gets or sets the maximum size of the text in the panel.
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Queues a log to be displayed.
        /// </summary>
        /// <param name="log">A LogItem.</param>
        public void Log(LogItem log)
        {
            this.logsQueue.Enqueue(log);
        }

        /// <summary>
        /// Handles the timer event.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">An EventArgs.</param>
        private void TimerTick(object sender, EventArgs e)
        {
            this.ProcessQueuedLogs();
        }

        /// <summary>
        /// <c>Dequeues</c> and displays logs.
        /// </summary>
        private void ProcessQueuedLogs()
        {
            LogItem log;
            while (this.logsQueue.TryDequeue(out log))
            {
                this.PrintLog(log);
            }

            if (this.logsTextBox.Text.Length > this.MaxSize)
            {
                this.TrimTextBox();
            }
        }

        /// <summary>
        /// Displays a log.
        /// </summary>
        /// <param name="log">A LogItem.</param>
        private void PrintLog(LogItem log)
        {
            string text = log.BuildText(Template, LogItem.DefaultDateTimeFormat);
            this.logsTextBox.AppendText(text);
        }

        /// <summary>
        /// Deleted part of the text being displayed.
        /// </summary>
        private void TrimTextBox()
        {
            string text = this.logsTextBox.Text.Substring(this.MaxSize / 3);
            int startIndex = text.IndexOf(LogDelimiter);
            if (startIndex > -1)
            {
                text = text.Substring(startIndex);
            }

            this.logsTextBox.Text = text;
            text = string.Format("\r\n{0}\r\nText Trimmed\r\n", LogDelimiter);
            this.logsTextBox.AppendText(text);
        }
    }
}
