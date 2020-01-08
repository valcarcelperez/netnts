namespace AllWayNet.Applications.WinForms
{
    partial class ControlPanel
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
            this.startStopButton = new System.Windows.Forms.Button();
            this.pauseResumeButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.warningsLabel = new System.Windows.Forms.Label();
            this.errorsLabel = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // startStopButton
            // 
            this.startStopButton.Location = new System.Drawing.Point(3, 3);
            this.startStopButton.Name = "startStopButton";
            this.startStopButton.Size = new System.Drawing.Size(75, 23);
            this.startStopButton.TabIndex = 0;
            this.startStopButton.Text = "Start";
            this.startStopButton.UseVisualStyleBackColor = true;
            this.startStopButton.Click += new System.EventHandler(this.StartStopButtonClick);
            // 
            // pauseResumeButton
            // 
            this.pauseResumeButton.Enabled = false;
            this.pauseResumeButton.Location = new System.Drawing.Point(84, 3);
            this.pauseResumeButton.Name = "pauseResumeButton";
            this.pauseResumeButton.Size = new System.Drawing.Size(75, 23);
            this.pauseResumeButton.TabIndex = 1;
            this.pauseResumeButton.Text = "Pause";
            this.pauseResumeButton.UseVisualStyleBackColor = true;
            this.pauseResumeButton.Click += new System.EventHandler(this.PauseResumeButtonClick);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(179, 8);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(40, 13);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "Status:";
            // 
            // warningsLabel
            // 
            this.warningsLabel.AutoSize = true;
            this.warningsLabel.Location = new System.Drawing.Point(306, 8);
            this.warningsLabel.Name = "warningsLabel";
            this.warningsLabel.Size = new System.Drawing.Size(55, 13);
            this.warningsLabel.TabIndex = 3;
            this.warningsLabel.Text = "Warnings:";
            // 
            // errorsLabel
            // 
            this.errorsLabel.AutoSize = true;
            this.errorsLabel.Location = new System.Drawing.Point(420, 8);
            this.errorsLabel.Name = "errorsLabel";
            this.errorsLabel.Size = new System.Drawing.Size(37, 13);
            this.errorsLabel.TabIndex = 4;
            this.errorsLabel.Text = "Errors:";
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // ControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.errorsLabel);
            this.Controls.Add(this.warningsLabel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.pauseResumeButton);
            this.Controls.Add(this.startStopButton);
            this.Name = "ControlPanel";
            this.Size = new System.Drawing.Size(602, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startStopButton;
        private System.Windows.Forms.Button pauseResumeButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label warningsLabel;
        private System.Windows.Forms.Label errorsLabel;
        private System.Windows.Forms.Timer timer;
    }
}
