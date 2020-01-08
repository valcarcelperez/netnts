namespace AllWayNet.Logger
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Performance counters for the Logger.
    /// </summary>
    public static class LoggerPerformanceCounters
    {
        /// <summary>
        /// Name used for the ErrorCount performance counter.
        /// </summary>
        public const string ErrorCountName = "ErrorCount";
        
        /// <summary>
        /// Text used as help in the ErrorCount performance counter.
        /// </summary>
        public const string ErrorCountHelp = "Total number of errors since the application started.";

        /// <summary>
        /// Name used for the WarningCount performance counter.
        /// </summary>
        public const string WarningCountName = "WarningCount";
        
        /// <summary>
        /// Text used as help in the WarningCount performance counter.
        /// </summary>
        public const string WarningCountHelp = "Total number of warnings since the application started.";

        /// <summary>
        /// Name used for the ErrorsPerMinuteCount performance counter.
        /// </summary>
        public const string ErrorsPerMinuteCountName = "ErrorsPerMinuteCount";

        /// <summary>
        /// Text used as help in the ErrorsPerMinuteCount performance counter.
        /// </summary>
        public const string ErrorsPerMinuteCountHelp = "Number of errors per minute.";

        /// <summary>
        /// Name used for the WarningsPerMinuteCount performance counter.
        /// </summary>
        public const string WarningsPerMinuteCountName = "WarningsPerMinuteCount";

        /// <summary>
        /// Text used as help in the WarningsPerMinuteCount performance counter.
        /// </summary>
        public const string WarningsPerMinuteCountHelp = "Number of warnings per minute.";

        /// <summary>
        /// Name used for the ErrorCountDelta performance counter.
        /// </summary>
        public const string ErrorCountDeltaName = "ErrorCountDelta";

        /// <summary>
        /// Text used as help in the ErrorCountDeltaName performance counter.
        /// </summary>
        public const string ErrorCountDeltaHelp = "Change in the number of errors between the two more recent sample intervals.";

        /// <summary>
        /// Name used for the WarningCountDelta performance counter.
        /// </summary>
        public const string WarningCountDeltaName = "WarningCountDelta";

        /// <summary>
        /// Text used as help in the WarningCountDelta performance counter.
        /// </summary>
        public const string WarningCountDeltaHelp = "Change in the number of warnings between the two more recent sample intervals.";

        /// <summary>
        /// Initializes static members of the <see cref="LoggerPerformanceCounters" /> class.
        /// </summary>
        static LoggerPerformanceCounters()
        {
            CountersPerMinuteEnabled = true;
            StandardCountersEnabled = true;
            DeltaCountersEnabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether performance counters CounterErrorsPerMinuteCount and CounterWarningsPerMinuteCount are being maintained. True is the default.
        /// </summary>
        public static bool CountersPerMinuteEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether performance counters ErrorCount and WarningsCount are being maintained. True is the default.
        /// </summary>
        public static bool StandardCountersEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether performance counters ErrorCountDelta and WarningsCountDelta are being maintained. True is the default.
        /// </summary>
        public static bool DeltaCountersEnabled { get; set; }

        /// <summary>
        /// Returns a collection with the performance counters used by the logger.
        /// </summary>
        /// <returns>A collection of CounterCreationData.</returns>
        public static IList<CounterCreationData> GetPerformanceCounters()
        {
            List<CounterCreationData> list = new List<CounterCreationData>();
            CounterCreationData counter;

            if (StandardCountersEnabled)
            {
                counter = new CounterCreationData(ErrorCountName, ErrorCountHelp, PerformanceCounterType.NumberOfItems32);
                list.Add(counter);

                counter = new CounterCreationData(WarningCountName, WarningCountHelp, PerformanceCounterType.NumberOfItems32);
                list.Add(counter);
            }

            if (CountersPerMinuteEnabled)
            {
                counter = new CounterCreationData(ErrorsPerMinuteCountName, ErrorsPerMinuteCountHelp, PerformanceCounterType.NumberOfItems32);
                list.Add(counter);

                counter = new CounterCreationData(WarningsPerMinuteCountName, WarningsPerMinuteCountHelp, PerformanceCounterType.NumberOfItems32);
                list.Add(counter);
            }

            if (DeltaCountersEnabled)
            {
                counter = new CounterCreationData(ErrorCountDeltaName, ErrorCountDeltaHelp, PerformanceCounterType.CounterDelta32);
                list.Add(counter);

                counter = new CounterCreationData(WarningCountDeltaName, WarningCountDeltaHelp, PerformanceCounterType.CounterDelta32);
                list.Add(counter);
            }

            return list;
        }
    }
}
