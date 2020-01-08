namespace AllWayNet.Applications.Installer
{
    partial class ApplicationHostInstaller<T>
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ServiceProcessInstaller
            // 
            this.ServiceProcessInstaller.Password = null;
            this.ServiceProcessInstaller.Username = null;
            // 
            // ServiceInstaller
            // 
            this.ServiceInstaller.ServiceName = "WinServiceHost";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ServiceProcessInstaller,
            this.ServiceInstaller});

        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        protected System.ServiceProcess.ServiceProcessInstaller ServiceProcessInstaller;
        
        /// <summary>
        /// 
        /// </summary>
        protected System.ServiceProcess.ServiceInstaller ServiceInstaller;
    }
}