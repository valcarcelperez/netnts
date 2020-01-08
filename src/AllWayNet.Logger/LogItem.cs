namespace AllWayNet.Logger
{
    using System;
    using System.Diagnostics;
    using System.Security;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Defines an Item that can be logged.
    /// </summary>
    public class LogItem
    {
        /// <summary>
        /// Default DateTimeFormat.
        /// </summary>
        public const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// DateTime token used when building a text from the log.
        /// </summary>
        private const string TokenDateTime = "#DateTime";

        /// <summary>
        /// ThreadId token used when building a text from the log.
        /// </summary>
        private const string TokenThreadId = "#ThreadId";

        /// <summary>
        /// Type token used when building a text from the log.
        /// </summary>
        private const string TokenType = "#Type";

        /// <summary>
        /// Description token used when building a text from the log.
        /// </summary>
        private const string TokenDescription = "#Description";
        
        /// <summary>
        /// CustomLogItem token used when building a text from the log.
        /// </summary>
        private const string TokenCustomLogItem = "#CustomLogItem";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LogItem" /> class
        /// </summary>
        /// <param name="logType">An EventLogEntryType</param>
        /// <param name="description">A description</param>
        /// <param name="customLogItem">A ICustomLogItem</param>
        public LogItem(EventLogEntryType logType, string description, ICustomLogItem customLogItem = null)
            : this(logType, description, customLogItem, DateTime.Now, Thread.CurrentThread.ManagedThreadId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogItem" /> class
        /// </summary>
        /// <param name="logType">An EventLogEntryType</param>
        /// <param name="description">A description</param>
        /// <param name="customLogItem">A ICustomLogItem</param>
        /// <param name="dateTime">A DateTime</param>
        /// <param name="threadId">A ThreadId</param>
        public LogItem(EventLogEntryType logType, string description, ICustomLogItem customLogItem, DateTime dateTime, int threadId)
        {
            this.LogType = logType;
            this.Description = description;
            this.CustomLogItem = customLogItem;

            this.DateTime = dateTime;
            this.ThreadId = threadId;
        }

        /// <summary>
        /// Gets the LogType
        /// </summary>
        public EventLogEntryType LogType { get; private set; }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description { get; private set; }
        
        /// <summary>
        /// Gets the DateTime
        /// </summary>
        public DateTime DateTime { get; private set; }

        /// <summary>
        /// Gets the Managed Thread Id
        /// </summary>
        public int ThreadId { get; private set; }

        /// <summary>
        /// Gets the CustomLogItem in the log.
        /// </summary>
        public ICustomLogItem CustomLogItem { get; private set; }

        /// <summary>
        /// Clones a LogItem.
        /// </summary>
        /// <returns>A new LogItem</returns>
        public LogItem Clone()
        {
            ICustomLogItem customLogItem = this.CustomLogItem == null ? null : this.CustomLogItem.Clone();
            LogItem result = new LogItem(this.LogType, this.Description, customLogItem, this.DateTime, this.ThreadId);
            return result;
        }

        /// <summary>
        /// Builds a text that represents this LogItem.
        /// </summary>
        /// <param name="template">A template used to format the text.</param>
        /// <param name="dateTimeFormat">Format used when adding the dateTime property.</param>
        /// <param name="isXmlTemplate">Indicates that the template is a xml.</param>
        /// <returns>A text that represents this LogItem.</returns>
        public string BuildText(string template, string dateTimeFormat = DefaultDateTimeFormat, bool isXmlTemplate = false)
        {
            StringBuilder sb = new StringBuilder(template);
            sb.Replace(TokenDateTime, this.DateTime.ToString(dateTimeFormat));
            sb.Replace(TokenThreadId, this.ThreadId.ToString());
            sb.Replace(TokenType, this.LogType.ToString());

            string customLogItemText = string.Empty;
            if (this.CustomLogItem != null)
            {
                customLogItemText = (this.CustomLogItem as object).ToString();
            }

            if (isXmlTemplate)
            {
                sb.Replace(TokenDescription, SecurityElement.Escape(this.Description));
            }
            else
            {
                sb.Replace(TokenDescription, this.Description);
            }

            sb.Replace(TokenCustomLogItem, customLogItemText);
            return sb.ToString();
        }
    }
}
