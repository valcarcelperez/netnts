namespace NETWAFService
{
    using System.ComponentModel;
    using AllWayNet.Applications.Installer;

    [System.ComponentModel.DesignerCategory("")] 
    [RunInstaller(true)] // This attribute is necessary for the installer to find this class.
    public class MainProcessInstaller : ApplicationHostInstaller<MainProcess>
    {
        // Override installer methods for adding custom installation.

        //public override void Install(System.Collections.IDictionary stateSaver)
        //{
        //    base.Install(stateSaver);

        //    // TODO: Install custom items.
        //}

        //public override void Rollback(System.Collections.IDictionary savedState)
        //{
        //    base.Rollback(savedState);

        //    // TODO: Rollback custom items.
        //}

        //public override void Uninstall(System.Collections.IDictionary savedState)
        //{
        //    base.Uninstall(savedState);

        //    // TODO: Uninstall custom items.
        //}
    }
}
