namespace AllWayNet.Common.File
{
    using System;
    using System.IO;
    using System.Linq;
    using AllWayNet.Common.Threading;

    /// <summary>
    /// Process that deletes old files.
    /// </summary>
    public class ExpiredFileDeleter : IDisposable
    {
        /// <summary>
        /// Internal timer that executes the method that monitors the directory.
        /// </summary>
        private TimerEx timerEx;

        /// <summary>
        /// Indicates that the object has been disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredFileDeleter" /> class.
        /// </summary>
        /// <param name="interval">Time between checks.</param>
        /// <param name="maxAge">Age of the file to be deleted.</param>
        /// <param name="directory">The directory to search.</param>
        /// <param name="searchPattern">The search string to match against the names of files in path.</param>
        public ExpiredFileDeleter(TimeSpan interval, TimeSpan maxAge, string directory, string searchPattern)
        {
            this.Interval = interval;
            this.Directory = directory;
            this.SearchPattern = searchPattern;
            this.MaxAge = maxAge;

            this.timerEx = new TimerEx((int)interval.TotalMilliseconds, this.DeleteOldFiles);
            this.timerEx.Error += this.TimerError;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ExpiredFileDeleter" /> class.
        /// </summary>
        ~ExpiredFileDeleter()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event raised when an exception occurs.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Gets the Interval.
        /// </summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>
        /// Gets the Directory.
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// Gets the SearchPattern.
        /// </summary>
        public string SearchPattern { get; private set; }

        /// <summary>
        /// Gets the MaxAge.
        /// </summary>
        public TimeSpan MaxAge { get; private set; }

        /// <summary>
        /// Stops the timer and release the resources used by this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the process that deletes old files.
        /// </summary>
        public void Start()
        {
            this.timerEx.Start();
        }

        /// <summary>
        /// Stops the process that deletes old files.
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
        /// Deletes old files.
        /// </summary>
        private void DeleteOldFiles()
        {
            DateTime refLastWriteTime = DateTime.UtcNow - this.MaxAge;
            FileInfo[] fileInfos = System.IO.Directory.GetFiles(this.Directory, this.SearchPattern)
                .Select(a => new FileInfo(a))
                .ToArray();
            
            foreach (FileInfo fileInfo in fileInfos)
            {
                fileInfo.Refresh();
                if (fileInfo.LastWriteTimeUtc < refLastWriteTime)
                {
                    File.Delete(fileInfo.FullName);
                }
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
