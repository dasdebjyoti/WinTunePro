namespace WinTunePro
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnTweakShowSecondsApply;
        private System.Windows.Forms.Button btnTweakShowSecondsRollback;
        private System.Windows.Forms.CheckBox chkTweakShowSeconds;
        private System.Windows.Forms.CheckBox chkTweakMinAnimate;
        private System.Windows.Forms.Button btnTweakMinAnimateApply;
        private System.Windows.Forms.Button btnTweakMinAnimateRollback;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;

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
            btnTweakShowSecondsApply = new Button();
            btnTweakShowSecondsRollback = new Button();
            btnTweakMinAnimateApply = new Button();
            btnTweakMinAnimateRollback = new Button();
            chkTweakShowSeconds = new CheckBox();
            chkTweakMinAnimate = new CheckBox();
            statusStrip1 = new StatusStrip();
            toolStripProgressBar1 = new ToolStripProgressBar();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnTweakShowSecondsApply
            // 
            btnTweakShowSecondsApply.Location = new Point(1219, 170);
            btnTweakShowSecondsApply.Name = "btnTweakShowSecondsApply";
            btnTweakShowSecondsApply.Size = new Size(120, 40);
            btnTweakShowSecondsApply.TabIndex = 1;
            btnTweakShowSecondsApply.Text = "Apply";
            btnTweakShowSecondsApply.UseVisualStyleBackColor = true;
            btnTweakShowSecondsApply.Click += BtnTweakShowSecondsApply_Click;
            // 
            // btnTweakShowSecondsRollback
            // 
            btnTweakShowSecondsRollback.Location = new Point(1345, 170);
            btnTweakShowSecondsRollback.Name = "btnTweakShowSecondsRollback";
            btnTweakShowSecondsRollback.Size = new Size(120, 40);
            btnTweakShowSecondsRollback.TabIndex = 2;
            btnTweakShowSecondsRollback.Text = "Rollback";
            btnTweakShowSecondsRollback.UseVisualStyleBackColor = true;
            btnTweakShowSecondsRollback.Click += BtnTweakShowSecondsRollback_Click;
            // 
            // btnTweakMinAnimateApply
            // 
            btnTweakMinAnimateApply.Location = new Point(1219, 227);
            btnTweakMinAnimateApply.Name = "btnTweakMinAnimateApply";
            btnTweakMinAnimateApply.Size = new Size(120, 40);
            btnTweakMinAnimateApply.TabIndex = 3;
            btnTweakMinAnimateApply.Text = "Apply";
            btnTweakMinAnimateApply.UseVisualStyleBackColor = true;
            btnTweakMinAnimateApply.Click += BtnTweakMinAnimateApply_Click;
            // 
            // btnTweakMinAnimateRollback
            // 
            btnTweakMinAnimateRollback.Location = new Point(1345, 227);
            btnTweakMinAnimateRollback.Name = "btnTweakMinAnimateRollback";
            btnTweakMinAnimateRollback.Size = new Size(120, 40);
            btnTweakMinAnimateRollback.TabIndex = 4;
            btnTweakMinAnimateRollback.Text = "Rollback";
            btnTweakMinAnimateRollback.UseVisualStyleBackColor = true;
            btnTweakMinAnimateRollback.Click += BtnTweakMinAnimateRollback_Click;
            // 
            // chkTweakShowSeconds
            // 
            chkTweakShowSeconds.AutoSize = true;
            chkTweakShowSeconds.Location = new Point(546, 173);
            chkTweakShowSeconds.Name = "chkTweakShowSeconds";
            chkTweakShowSeconds.Size = new Size(572, 36);
            chkTweakShowSeconds.TabIndex = 4;
            chkTweakShowSeconds.Text = "Show seconds in taskbar clock (uses more power)";
            chkTweakShowSeconds.UseVisualStyleBackColor = true;
            chkTweakShowSeconds.CheckedChanged += chkTweakShowSeconds_CheckedChanged;
            // 
            // chkTweakMinAnimate
            // 
            chkTweakMinAnimate.AutoSize = true;
            chkTweakMinAnimate.Location = new Point(546, 230);
            chkTweakMinAnimate.Name = "chkTweakMinAnimate";
            chkTweakMinAnimate.Size = new Size(580, 36);
            chkTweakMinAnimate.TabIndex = 4;
            chkTweakMinAnimate.Text = "Animate windows when minimizing && maximizing";
            chkTweakMinAnimate.UseVisualStyleBackColor = true;
            chkTweakMinAnimate.CheckedChanged += chkTweakMinAnimate_CheckedChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(32, 32);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripProgressBar1, toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 871);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1535, 40);
            statusStrip1.TabIndex = 5;
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(100, 28);
            toolStripProgressBar1.Click += toolStripProgressBar1_Click;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(0, 30);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1535, 911);
            Controls.Add(chkTweakShowSeconds);
            Controls.Add(chkTweakMinAnimate);
            Controls.Add(btnTweakShowSecondsApply);
            Controls.Add(btnTweakShowSecondsRollback);
            Controls.Add(btnTweakMinAnimateApply);
            Controls.Add(btnTweakMinAnimateRollback);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "MainForm";
            Text = "WinTunePro";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
