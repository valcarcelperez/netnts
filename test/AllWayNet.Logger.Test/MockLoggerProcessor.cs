namespace AllWayNet.Logger.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public class MockLoggerProcessor : ILoggerProcessor
    {
        public MockLoggerProcessor()
            : this(string.Empty)
        {
        }

        public MockLoggerProcessor(string name)
        {
            this.Name = name;
            this.LogItems = new List<LogItem>();
        }

        public string Name { get; private set; }

        public Exception Exception { get; set; }

        public List<LogItem> LogItems { get; private set; }

        public int DisposeCount { get; private set; }

        public void Prepare(XElement xml)
        {
        }

        public void Dispose()
        {
            this.DisposeCount++;
        }

        public bool HasErrorEventSubscribers
        {
            get
            {
                return Error != null;
            }
        }

        public void Log(LogItem log)
        {
            if (this.Exception != null)
            {
                throw this.Exception;
            }

            this.LogItems.Add(log);
        }

        public event EventHandler<ErrorEventArgs> Error;

        public void RaiseErrorEvent(Exception exception)
        {
            EventHandler<ErrorEventArgs> handler = this.Error;
            if (handler != null)
            {
                handler(this, new ErrorEventArgs(exception));
            }
        }
    }
}