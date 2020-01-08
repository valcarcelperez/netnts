namespace AllWayNet.LogFile
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using AllWayNet.Common.Configuration;
    using AllWayNet.Common.File;
    using AllWayNet.Logger;

    /// <summary>
    /// Implements a logger processor that logs to the Event Logs.
    /// </summary>
    public class LoggerProcessorLogFile : ILoggerProcessor
    {
        private const bool DefaultIsXmlTemplate = false;
        private const string AttributeName = "name";
        private const string AttributeFilename = "filename";
        private const string AttributeDateTimeFormat = "dateTimeFormat";
        private const string AttributeMaxLogSizeKB = "maxLogSizeKB";
        private const string AttributeMaxLogAge = "maxLogAge";
        private const string ElementTemplate = "template";
        private const string AttributeIsXmlTemplate = "isXmlTemplate";

        private const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        private const int DefaultMaxLogSizeKB = 10240;
        private TimeSpan defaultMaxLogAge = new TimeSpan(7, 0, 0, 0);

        private FileStream fileStream = null;
        private object lockObj = new object();
        private FileSizeMonitor fileSizeMonitor = null;
        private ExpiredFileDeleter expiredFileDeleter = null;

        private bool disposed = false;

        /// <summary>
        /// Finalizes an instance of the <see cref="LoggerProcessorLogFile" /> class.
        /// </summary>
        ~LoggerProcessorLogFile()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event raised when there is an internal error in the Logger Processor.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Gets the Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the Filename.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the Template.
        /// </summary>
        public string Template { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the Template is a XML.
        /// </summary>
        public bool IsXmlTemplate { get; private set; }

        /// <summary>
        /// Gets the DateTimeFormat.
        /// </summary>
        public string DateTimeFormat { get; private set; }

        /// <summary>
        /// Gets the MaxLogSizeKB.
        /// </summary>
        public int MaxLogSizeKB { get; private set; }

        /// <summary>
        /// Gets the MaxLogAge.
        /// </summary>
        public TimeSpan MaxLogAge { get; private set; }

        /// <summary>
        /// Logs a LogItem.
        /// </summary>
        /// <param name="log"></param>
        public void Log(LogItem log)
        {
            string text = log.BuildText(this.Template, this.DateTimeFormat, this.IsXmlTemplate);
            byte[] data = Encoding.ASCII.GetBytes(text);

            lock (this.lockObj)
            {
                this.fileStream.Write(data, 0, data.Length);
                this.fileStream.Flush(true);
            }
        }

        /// <summary>
        /// Prepares the Logger Processor.
        /// </summary>
        /// <param name="xml"></param>
        public void Prepare(XElement xml)
        {
            this.DecodeConfig(xml);
            this.Template = ConfigurationHelper.GetElementValue(xml, ElementTemplate);
            this.CreateDirectory();
            this.OpenFile();

            int tenSeconds = 10000;
            this.fileSizeMonitor = new FileSizeMonitor(tenSeconds, this.Filename, this.MaxLogSizeKB * 1024);
            this.fileSizeMonitor.Error += this.ErrorHandler;
            this.fileSizeMonitor.MaxSize += this.FileSizeMonitorMaxSizeReached;
            this.fileSizeMonitor.Start();

            TimeSpan oneHour = new TimeSpan(1, 0, 0);
            string directory = Path.GetDirectoryName(this.Filename);
            string searchPattern = string.Format("*.{0}", Path.GetExtension(this.Filename));
            this.expiredFileDeleter = new ExpiredFileDeleter(oneHour, this.MaxLogAge, directory, searchPattern);
            this.expiredFileDeleter.Error += this.ErrorHandler;
            this.expiredFileDeleter.Start();
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called when handling errors.
        /// </summary>
        /// <param name="e"></param>
        protected void OnError(ErrorEventArgs e)
        {
            EventHandler<ErrorEventArgs> handler = this.Error;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.DisposeManagedResources();
                }

                this.disposed = true;
            }
        }

        private void DisposeManagedResources()
        {
            this.CloseFile();

            if (this.fileSizeMonitor != null)
            {
                this.fileSizeMonitor.Dispose();
            }

            if (this.expiredFileDeleter != null)
            {
                this.expiredFileDeleter.Dispose();
            }
        }

        private void DecodeConfig(XElement xml)
        {
            this.Name = ConfigurationHelper.GetAttributeValue(xml, AttributeName);
            this.Filename = ConfigurationHelper.GetAttributeValue(xml, AttributeFilename, this.GetDefaultFilename());
            this.DateTimeFormat = ConfigurationHelper.GetAttributeValue(xml, AttributeDateTimeFormat, DefaultDateTimeFormat);
            this.MaxLogSizeKB = ConfigurationHelper.GetIntAttributeValue(xml, AttributeMaxLogSizeKB, DefaultMaxLogSizeKB);
            this.MaxLogAge = ConfigurationHelper.GetTimeSpanAttributeValue(xml, AttributeMaxLogAge, this.defaultMaxLogAge);
            this.IsXmlTemplate = ConfigurationHelper.GetBooleanAttributeValue(xml, AttributeIsXmlTemplate, DefaultIsXmlTemplate);
        }

        private string GetDefaultFilename()
        {
            string fullPath = Environment.GetCommandLineArgs()[0];
            string directory = Path.GetDirectoryName(fullPath);
            string filename = string.Format("{0}.log", Path.GetFileNameWithoutExtension(fullPath));
            return Path.Combine(directory, filename);
        }

        private void CloseFile()
        {
            lock (this.lockObj)
            {
                if (this.fileStream == null)
                {
                    return;
                }

                this.fileStream.Close();
                this.fileStream = null;
            }
        }

        private void OpenFile()
        {
            lock (this.lockObj)
            {
                this.CloseFile();
                this.fileStream = new FileStream(this.Filename, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
        }

        private void ErrorHandler(object sender, ErrorEventArgs e)
        {
            this.OnError(e);
        }

        private void FileSizeMonitorMaxSizeReached(object sender, EventArgs e)
        {
            lock (this.lockObj)
            {
                this.CloseFile();
                DateTime datetime = DateTime.Now;
                string newFilename = this.GetNewFilename(datetime);
                File.Move(this.Filename, newFilename);
                File.SetLastWriteTime(this.Filename, datetime);
                this.OpenFile();
            }
        }

        private string GetNewFilename(DateTime datetime)
        {
            string currentFilename = Path.GetFileNameWithoutExtension(this.Filename);
            string currentFilenameExtension = Path.GetExtension(this.Filename);
            return string.Format("{0}-{1}.{2}", currentFilename, datetime.ToString("yyyy-MM-dd-mm-HH-ss"), currentFilenameExtension);
        }

        private void CreateDirectory()
        {
            string directory = Path.GetDirectoryName(this.Filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
