namespace AllWayNet.Logger
{
    using System.Configuration;

    /// <summary>
    /// Defines the configuration for a Application Logger
    /// </summary>
    public static class ApplicationLoggerConfig
    {
        /// <summary>
        /// Initializes static members of the <see cref="ApplicationLoggerConfig" /> class.
        /// </summary>
        static ApplicationLoggerConfig()
        {
            ApplicationLogger = LoadConfigurationSection();
        }

        /// <summary>
        /// Gets a ApplicationLoggerSection.
        /// </summary>
        public static ApplicationLoggerSection ApplicationLogger { get; private set; }

        /// <summary>
        /// Loads the configuration section.
        /// </summary>
        /// <returns>A ApplicationLoggerSection.</returns>
        private static ApplicationLoggerSection LoadConfigurationSection()
        {
            ApplicationLoggerSection applicationLogger = ConfigurationManager.GetSection(ApplicationLoggerSection.SectionName) as ApplicationLoggerSection;
            if (applicationLogger == null)
            {
                string message = string.Format("Section not found. {0}", ApplicationLoggerSection.SectionName);
                throw new ConfigurationErrorsException(message);
            }

            return applicationLogger;
        }
    }
}