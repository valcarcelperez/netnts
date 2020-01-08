namespace AllWayNet.Applications.PerformanceCounters
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Support for performance counters during the installation.
    /// </summary>
    public static class ApplicationPerformanceCountersInstaller
    {
        /// <summary>
        /// Creates performance counters.
        /// </summary>
        /// <param name="categoryName">The name of the custom performance counter category to create and register with the system.</param>
        /// <param name="categoryHelp">A description of the custom category.</param>
        /// <param name="categoryType">One of the System.Diagnostics.PerformanceCounterCategoryType values.</param>
        /// <param name="counters">A collection of CounterCreationData.</param>
        public static void CreatePerformanceCounters(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, IList<CounterCreationData> counters)
        {
            RemovePerformanceCounters(categoryName);

            // prepare counter creation collection
            CounterCreationDataCollection ccdc = new CounterCreationDataCollection();

            if (counters != null)
            {
                ccdc.AddRange(counters.ToArray());
            }

            // create the performance counter category
            PerformanceCounterCategory.Create(categoryName, categoryHelp, categoryType, ccdc);
        }

        /// <summary>
        /// Removes performance counters.
        /// </summary>
        /// <param name="categoryName">The name of the custom performance counter category to delete.</param>
        public static void RemovePerformanceCounters(string categoryName)
        {
            if (PerformanceCounterCategory.Exists(categoryName))
            {
                PerformanceCounterCategory.Delete(categoryName);
            }
        }
    }
}
