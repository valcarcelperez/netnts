namespace ResourceLoggerService
{
    using System.ComponentModel;
    using AllWayNet.Applications.Installer;

    /// <summary>
    /// Application installer.
    /// </summary>
    [RunInstaller(true)]
    public class ResourceMonitorInstaller : ApplicationHostInstaller<ResourceMonitor>
    {
    }
}
