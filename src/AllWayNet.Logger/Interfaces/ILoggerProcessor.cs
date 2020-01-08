namespace AllWayNet.Logger
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    /// <summary>
    /// Defines a Logger Processor.
    /// </summary>
    public interface ILoggerProcessor : IDisposable
    {
        /// <summary>
        /// Event raised when there is an internal error in the Logger Processor.
        /// </summary>
        event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Gets the Name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Logs a LogItem.
        /// </summary>
        /// <param name="log">A LogItem</param>
        void Log(LogItem log);

        /// <summary>
        /// Prepares the Logger Processor.
        /// </summary>
        /// <param name="xml">A XElement containing the configuration</param>
        void Prepare(XElement xml);
    }
}    