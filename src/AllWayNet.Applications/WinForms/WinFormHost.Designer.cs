namespace AllWayNet.Applications.WinForms
{
    partial class WinFormHost
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.logsPanel = new AllWayNet.Applications.WinForms.LogsPanel();
            this.controlPanel = new AllWayNet.Applications.WinForms.ControlPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 31);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.logsPanel);
            this.splitContainer.Size = new System.Drawing.Size(692, 476);
            this.splitContainer.SplitterDistance = 275;
            this.splitContainer.TabIndex = 2;
            // 
            // logsPanel
            // 
            this.logsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logsPanel.Location = new System.Drawing.Point(0, 0);
            this.logsPanel.MaxSize = 102400;
            this.logsPanel.Name = "logsPanel";
            this.logsPanel.Size = new System.Drawing.Size(692, 197);
            this.logsPanel.TabIndex = 1;
            // 
            // controlPanel
            // 
            this.controlPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.controlPanel.Location = new System.Drawing.Point(0, 0);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(692, 31);
            this.controlPanel.TabIndex = 0;
            // 
            // WinFormHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 507);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.controlPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "WinFormHost";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinFormHost";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WinFormHostFormClosing);
            this.Load += new System.EventHandler(this.WinFormHostLoad);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ControlPanel controlPanel;
        private LogsPanel logsPanel;
        private System.Windows.Forms.SplitContainer splitContainer;
    }
}