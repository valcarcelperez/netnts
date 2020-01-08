namespace AllWayNet.Common.File
{
    using System;
    using System.IO;
    using AllWayNet.Common.Threading;

    /// <summary>
    /// Defines a class that monitors a file and raises an event when the file's size is larger than a specified value.
    /// </summary>
    public class FileSizeMonitor : IDisposable
    {
        /// <summary>
        /// Internal timer that executes the method that monitors the file.
        /// </summary>
        private TimerEx timerEx;
        
        /// <summary>
        /// File to be monitored.
        /// </summary>
        private string filename;
        
        /// <summary>
        /// The max size that the file can has.
        /// </summary>
        private int maxSize;

        /// <summary>
        /// Indicates that the object has been disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSizeMonitor" /> class.
        /// </summary>
        /// <param name="interval">The time, in milliseconds, between the events that check the file size.</param>
        /// <param name="filename">The name of the file to be monitored.</param>
        /// <param name="maxSize">The max size of the file.</param>
        public FileSizeMonitor(int interval, string filename, int maxSize)
        {
            this.timerEx = new TimerEx(interval, this.CheckFile);
            this.timerEx.Error += this.TimerError;
            this.filename = filename;
            this.maxSize = maxSize;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FileSizeMonitor" /> class.
        /// </summary>
        ~FileSizeMonitor()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event raised when an exception occurs.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Event raised when the file reaches the max size.
        /// </summary>
        public event EventHandler MaxSize;

        /// <summary>
        /// Stops the timer and release the resources used by this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts monitoring the file.
        /// </summary>
        public void Start()
        {
            this.timerEx.Start();
        }

        /// <summary>
        /// Stops monitoring the file.
        /// </summary>
        public void Stop()
        {
            this.timerEx.Stop();
        }

        /// <summary>
        /// Part of the disposing mechanism.
        /// </summary>
        /// <param name="disposing">Indicates that the Dispose method is being called by the user code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.timerEx.Dispose();
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Executed when the file reaches the max size.
        /// </summary>
        /// <param name="e">An EventArgs </param>
        protected virtual void OnMaxSize(EventArgs e)
        {
            EventHandler handler = this.MaxSize;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Executed when an exception occurs.
        /// </summary>
        /// <param name="e">An ErrorEventArgs containing the exception information.</param>
        protected virtual void OnError(ErrorEventArgs e)
        {
            EventHandler<ErrorEventArgs> handler = this.Error;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Checks the file size.
        /// </summary>
        private void CheckFile()
        {
            FileInfo fileInfo = new FileInfo(this.filename);
            if (fileInfo.Length > this.maxSize)
            {
                this.OnMaxSize(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the Error event of the internal timer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An ErrorEventArgs object that contains the exception.</param>
        private void TimerError(object sender, ErrorEventArgs e)
        {
            this.OnError(e);
        }
    }
}
