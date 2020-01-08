namespace AllWayNet.Logger
{
    /// <summary>
    /// Defines a Custom Log Item that allows to include custom information in the logs.
    /// </summary>
    public interface ICustomLogItem
    {
        /// <summary>
        /// Clones this object.
        /// </summary>
        /// <returns>A ICustomLogItem.</returns>
        ICustomLogItem Clone();
    }
}
