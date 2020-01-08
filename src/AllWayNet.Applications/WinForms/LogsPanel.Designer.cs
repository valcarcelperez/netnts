namespace AllWayNet.Applications.WinForms
{
    partial class LogsPanel
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
            this.components = new System.ComponentModel.Container();
            this.logsTextBox = new System.Windows.Forms.TextBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // logsTextBox
            // 
            this.logsTextBox.BackColor = System.Drawing.Color.Black;
            this.logsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logsTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logsTextBox.ForeColor = System.Drawing.Color.Yellow;
            this.logsTextBox.Location = new System.Drawing.Point(0, 0);
            this.logsTextBox.Multiline = true;
            this.logsTextBox.Name = "logsTextBox";
            this.logsTextBox.ReadOnly = true;
            this.logsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logsTextBox.Size = new System.Drawing.Size(716, 359);
            this.logsTextBox.TabIndex = 0;
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // LogsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.logsTextBox);
            this.Name = "LogsPanel";
            this.Size = new System.Drawing.Size(716, 359);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox logsTextBox;
        private System.Windows.Forms.Timer timer;
    }
}
