namespace AllWayNet.Applications
{
    /// <summary>
    /// Defines the statuses for <c>HostableProcess</c>.
    /// </summary>
    public enum HostableProcessStatus
    {
        /// <summary>
        /// The <c>HostableProcess</c> is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// The <c>HostableProcess</c> is running.
        /// </summary>
        Running,

        /// <summary>
        /// The <c>HostableProcess</c> is paused.
        /// </summary>
        Paused
    }
}
