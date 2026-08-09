namespace WinTunePro
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.CheckBox chkShowSeconds;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnRollback;
        private System.Windows.Forms.Label lblStatus;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            chkShowSeconds = new CheckBox();
            btnApply = new Button();
            btnRollback = new Button();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // chkShowSeconds
            // 
            chkShowSeconds.AutoSize = true;
            chkShowSeconds.Location = new Point(24, 24);
            chkShowSeconds.Name = "chkShowSeconds";
            chkShowSeconds.Size = new Size(572, 36);
            chkShowSeconds.TabIndex = 0;
            chkShowSeconds.Text = "Show seconds in taskbar clock (uses more power)";
            chkShowSeconds.UseVisualStyleBackColor = true;
            chkShowSeconds.CheckedChanged += chkShowSeconds_CheckedChanged;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(24, 72);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(120, 40);
            btnApply.TabIndex = 1;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += BtnApply_Click;
            // 
            // btnRollback
            // 
            btnRollback.Location = new Point(156, 72);
            btnRollback.Name = "btnRollback";
            btnRollback.Size = new Size(120, 40);
            btnRollback.TabIndex = 2;
            btnRollback.Text = "Rollback";
            btnRollback.UseVisualStyleBackColor = true;
            btnRollback.Click += BtnRollback_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(24, 128);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(78, 32);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Ready";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(615, 220);
            Controls.Add(chkShowSeconds);
            Controls.Add(btnApply);
            Controls.Add(btnRollback);
            Controls.Add(lblStatus);
            Name = "MainForm";
            Text = "WinTunePro";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
