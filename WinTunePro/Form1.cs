using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using WinTunePro.Tweaks;
using WinTunePro.Utils;

namespace WinTunePro
{
    public partial class MainForm : Form
    {
        private const uint HWND_BROADCAST = 0xFFFF;
        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam,
            uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        public MainForm()
        {
            InitializeComponent();
            Logger.LogInfo("----");
            Logger.LogInfo("MainForm initializing");

            // Restore previous window location/size if available
            try
            {
                RestoreWindowState();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to restore window state");
            }

            // Initialize checkbox states
            InitializeCheckboxStates();
            SetStatus("Ready.");
            // Save window state on close
            this.FormClosing += MainForm_FormClosing;
            Logger.LogInfo("MainForm initialized");
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                SaveWindowState();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to save window state");
            }
            Logger.LogInfo("Exiting");
        }

        private void ChkTweakShowSeconds_CheckedChanged(object sender, EventArgs e)
        {
            bool originalState = chkTweakShowSeconds.Tag is bool b && b;
            btnTweakShowSecondsApply.Enabled = chkTweakShowSeconds.Checked != originalState;
        }
        private void ChkTweakMinAnimate_CheckedChanged(object sender, EventArgs e)
        {
            bool originalState = chkTweakMinAnimate.Tag is bool b && b;
            btnTweakMinAnimateApply.Enabled = chkTweakMinAnimate.Checked != originalState;
        }

        private void ChkTweakTaskbarAnimate_CheckedChanged(object sender, EventArgs e)
        {
            bool originalState = chkTweakTaskbarAnimate.Tag is bool b && b;
            btnTweakTaskbarAnimateApply.Enabled = chkTweakTaskbarAnimate.Checked != originalState;
        }
        private bool BroadcastSettingChange(string area)
        {
            try
            {
                SendMessageTimeout((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, area, SMTO_ABORTIFHUNG, 5000, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RestoreWindowState()
        {
            var model = WindowStateStore.Load();
            if (model == null) return;

            this.StartPosition = FormStartPosition.Manual;

            // Apply size
            if (model.Width > 0 && model.Height > 0)
            {
                this.Size = new System.Drawing.Size(model.Width, model.Height);
            }

            // Apply location
            var desired = new System.Drawing.Point(model.X, model.Y);
            this.Location = desired;

            // Ensure at least partly visible; if not, center on primary screen
            var windowRect = new System.Drawing.Rectangle(this.Location, this.Size);
            bool visible = false;
            foreach (var s in Screen.AllScreens)
            {
                if (s.WorkingArea.IntersectsWith(windowRect)) { visible = true; break; }
            }

            if (!visible)
            {
                var wa = Screen.PrimaryScreen.WorkingArea;
                var cx = wa.X + Math.Max(0, (wa.Width - this.Width) / 2);
                var cy = wa.Y + Math.Max(0, (wa.Height - this.Height) / 2);
                this.Location = new System.Drawing.Point(cx, cy);
            }

            // Apply maximized state
            if (model.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void SaveWindowState()
        {
            var isMax = this.WindowState == FormWindowState.Maximized;
            var bounds = isMax ? this.RestoreBounds : this.Bounds;
            var model = new WindowStateModel
            {
                X = bounds.X,
                Y = bounds.Y,
                Width = bounds.Width,
                Height = bounds.Height,
                IsMaximized = isMax
            };

            WindowStateStore.Save(model);
        }

        private void InitializeCheckboxStates()
        {
            // TODO: Instead of reading registry values here, add functions in the respective classes (ShowSecondsTweak, MinAnimateTweak) to get the current state, and call those functions here
            // This will encapsulate the logic and make it easier to maintain.
            try
            {
                // Initialize the "Show Seconds" checkbox based on the registry value
                var showSecondsTweak = new TweakShowSeconds(chkTweakShowSeconds.Checked);
                chkTweakShowSeconds.Checked = showSecondsTweak.GetCurrentEnabled() ?? false;
                chkTweakShowSeconds.Tag = chkTweakShowSeconds.Checked; // store initial state for comparison
                btnTweakShowSecondsApply.Enabled = false;
                btnTweakShowSecondsRollback.Enabled = showSecondsTweak.IsBackupFileAvailable() ?? false;

                // Initialize the "Animate Windows" checkbox based on the registry value
                var tweak = new TweakMinAnimate(chkTweakMinAnimate.Checked);
                chkTweakMinAnimate.Checked = tweak.GetCurrentEnabled() ?? false;
                chkTweakMinAnimate.Tag = chkTweakMinAnimate.Checked; // store initial state for comparison
                btnTweakMinAnimateApply.Enabled = false;
                btnTweakMinAnimateRollback.Enabled = tweak.IsBackupFileAvailable() ?? false;

                // Initialize the "Taskbar Animate" checkbox based on the registry value
                var taskbarTweak = new TweakTaskbarAnimations(chkTweakTaskbarAnimate.Checked);
                chkTweakTaskbarAnimate.Checked = taskbarTweak.GetCurrentEnabled() ?? false;
                chkTweakTaskbarAnimate.Tag = chkTweakTaskbarAnimate.Checked; // store initial state for comparison
                btnTweakTaskbarAnimateApply.Enabled = false;
                btnTweakTaskbarAnimateRollback.Enabled = taskbarTweak.IsBackupFileAvailable() ?? false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to initialize checkbox states");
            }
        }

        private void SetStatus(string text)
        {
            try
            {
                if (toolStripStatusLabel1 != null && !toolStripStatusLabel1.IsDisposed)
                {
                    if (toolStripStatusLabel1.GetCurrentParent().InvokeRequired)
                    {
                        toolStripStatusLabel1.GetCurrentParent().Invoke(() => toolStripStatusLabel1.Text = text);
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = text;
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private void StartProgress()
        {
            try
            {
                if (toolStripProgressBar1 != null)
                {
                    toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
                    toolStripProgressBar1.MarqueeAnimationSpeed = 30;
                    toolStripProgressBar1.Visible = true;

                    btnTweakTaskbarAnimateApply.Enabled = false;
                    btnTweakTaskbarAnimateRollback.Enabled = false;
                }
            }
            catch { }
        }

        private void StopProgress()
        {
            try
            {
                if (toolStripProgressBar1 != null)
                {
                    //toolStripProgressBar1.Visible = false;
                    toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Blocks;
                    toolStripProgressBar1.Value = 0;
                }
            }
            catch { }
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private async void BtnTweakShowSecondsApply_Click(object? sender, EventArgs e)
        {
            SetStatus("Applying show seconds tweak...");
            CheckBox tweakCheckbox = chkTweakShowSeconds;
            Button tweakButtonApply = btnTweakShowSecondsApply;
            Button tweakButtonRollback = btnTweakShowSecondsRollback;

            tweakButtonApply.Enabled = false;
            tweakButtonRollback.Enabled = false;

            var tweak = new TweakShowSeconds(tweakCheckbox.Checked);
            Logger.LogInfo($"Applying: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            var ok = await Task.Run(() => tweak.ApplyAsync());

            if (ok)
            {
                SetStatus("Tweak applied.");
                Logger.LogInfo($"Applied: ({tweak.Id}, enable={tweakCheckbox.Checked})");

                var resp = MessageBox.Show(this, "Change applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    Logger.LogInfo("User agreed to restart Explorer");
                    SetStatus("Restarting Explorer...");
                    await Task.Run(() => ExplorerRestarter.RestartExplorer());
                    SetStatus("Restarted Explorer.");
                }
                else
                {
                    Logger.LogInfo("User declined to restart Explorer after apply");
                    SetStatus("Applied (no restart).");
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Failed to apply tweak.");
                Logger.LogWarning($"Apply failed: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            }
            StopProgress();
        }

        private async void BtnTweakShowSecondsRollback_Click(object? sender, EventArgs e)
        {
            SetStatus("Rolling back show seconds tweak...");
            StartProgress();
            CheckBox tweakCheckbox = chkTweakShowSeconds;
            Button tweakButtonApply = btnTweakShowSecondsApply;
            Button tweakButtonRollback = btnTweakShowSecondsRollback;

            tweakButtonApply.Enabled = false;
            tweakButtonRollback.Enabled = false;

            var tweak = new TweakShowSeconds(false);
            Logger.LogInfo($"Rolling back: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            var ok = await Task.Run(() => tweak.RollbackAsync());

            if (ok)
            {
                SetStatus("Rollback applied.");
                Logger.LogInfo($"Rollback applied: ({tweak.Id}, enable={tweakCheckbox.Checked})");

                var resp = MessageBox.Show(this, "Rollback applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    Logger.LogInfo("User agreed to restart Explorer");
                    SetStatus("Restarting Explorer...");
                    await Task.Run(() => ExplorerRestarter.RestartExplorer());
                    SetStatus("Restarted Explorer.");
                }
                else
                {
                    Logger.LogInfo("User declined to restart Explorer after rollback");
                    SetStatus("Rollback applied (no restart).");
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Failed to rollback tweak.");
                Logger.LogWarning($"Rollback failed: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            }
            StopProgress();
       }

        private async void BtnTweakMinAnimateApply_Click(object? sender, EventArgs e)
        {
            SetStatus("Applying minimize animations tweak...");
            StartProgress();
            CheckBox tweakCheckbox = chkTweakMinAnimate;
            Button tweakButtonApply = btnTweakMinAnimateApply;
            Button tweakButtonRollback = btnTweakMinAnimateRollback;

            tweakButtonApply.Enabled = false;
            tweakButtonRollback.Enabled = false;

            var tweak = new TweakMinAnimate(tweakCheckbox.Checked);
            Logger.LogInfo($"Applying: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            var ok = await Task.Run(() => tweak.ApplyAsync());

            if (ok)
            {
                SetStatus("Tweak applied.");
                Logger.LogInfo($"Applied: ({tweak.Id}, enable={tweakCheckbox.Checked})");

                var resp = MessageBox.Show(this, "Change applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    Logger.LogInfo("User agreed to restart Explorer");
                    SetStatus("Restarting Explorer...");
                    await Task.Run(() => ExplorerRestarter.RestartExplorer());
                    SetStatus("Restarted Explorer.");
                }
                else
                {
                    Logger.LogInfo("User declined to restart Explorer after apply");
                    SetStatus("Applied (no restart).");
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Failed to apply tweak.");
                Logger.LogWarning($"Apply failed: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            }
            StopProgress();
        }

        private async void BtnTweakMinAnimateRollback_Click(object? sender, EventArgs e)
        {
            SetStatus("Rolling back minimize animations tweak...");
            StartProgress();
            CheckBox tweakCheckbox = chkTweakMinAnimate;
            Button tweakButtonApply = btnTweakMinAnimateApply;
            Button tweakButtonRollback = btnTweakMinAnimateRollback;

            tweakButtonApply.Enabled = false;
            tweakButtonRollback.Enabled = false;

            var tweak = new TweakMinAnimate(false);
            Logger.LogInfo($"Rolling back: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            var ok = await Task.Run(() => tweak.RollbackAsync());

            if (ok)
            {
                SetStatus("Rollback applied.");
                Logger.LogInfo($"Rollback applied: ({tweak.Id}, enable={tweakCheckbox.Checked})");

                var resp = MessageBox.Show(this, "Rollback applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    Logger.LogInfo("User agreed to restart Explorer");
                    SetStatus("Restarting Explorer...");
                    await Task.Run(() => ExplorerRestarter.RestartExplorer());
                    SetStatus("Restarted Explorer.");
                }
                else
                {
                    Logger.LogInfo("User declined to restart Explorer after rollback");
                    SetStatus("Rollback applied (no restart).");
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Failed to rollback tweak.");
                Logger.LogWarning($"Rollback failed: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            }
            StopProgress();
        }

        private async void BtnTweakTaskbarAnimateApply_Click(object sender, EventArgs e)
        {
            SetStatus("Applying taskbar animations tweak...");
            StartProgress();
            CheckBox tweakCheckbox = chkTweakTaskbarAnimate;
            Button tweakButtonApply = btnTweakTaskbarAnimateApply;
            Button tweakButtonRollback = btnTweakTaskbarAnimateRollback;

            tweakButtonApply.Enabled = false;
            tweakButtonRollback.Enabled = false;

            var tweak = new TweakTaskbarAnimations(tweakCheckbox.Checked);
            Logger.LogInfo($"Applying: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            var ok = await Task.Run(() => tweak.ApplyAsync());

            if (ok)
            {
                SetStatus("Tweak applied.");
                Logger.LogInfo($"Applied: ({tweak.Id}, enable={tweakCheckbox.Checked})");

                var resp = MessageBox.Show(this, "Change applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    Logger.LogInfo("User agreed to restart Explorer");
                    SetStatus("Restarting Explorer...");
                    await Task.Run(() => ExplorerRestarter.RestartExplorer());
                    SetStatus("Restarted Explorer.");
                }
                else
                {
                    Logger.LogInfo("User declined to restart Explorer after apply");
                    SetStatus("Applied (no restart).");
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Failed to apply tweak.");
                Logger.LogWarning($"Apply failed: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            }
            StopProgress();
        }

        private async void BtnTweakTaskbarAnimateRollback_Click(object sender, EventArgs e)
        {
            SetStatus("Rolling back taskbar animations tweak...");
            StartProgress();
            CheckBox tweakCheckbox = chkTweakTaskbarAnimate;
            Button tweakButtonApply = btnTweakTaskbarAnimateApply;
            Button tweakButtonRollback = btnTweakTaskbarAnimateRollback;
            
            tweakButtonApply.Enabled = false;
            tweakButtonRollback.Enabled = false;

            var tweak = new TweakTaskbarAnimations(false);
            Logger.LogInfo($"Rolling back: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            var ok = await Task.Run(() => tweak.RollbackAsync());

            if (ok)
            {
                SetStatus("Rollback applied.");
                Logger.LogInfo($"Rollback applied: ({tweak.Id}, enable={tweakCheckbox.Checked})");

                var resp = MessageBox.Show(this, "Rollback applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    Logger.LogInfo("User agreed to restart Explorer");
                    SetStatus("Restarting Explorer...");
                    await Task.Run(() => ExplorerRestarter.RestartExplorer());
                    SetStatus("Restarted Explorer.");
                }
                else
                {
                    Logger.LogInfo("User declined to restart Explorer after rollback");
                    SetStatus("Rollback applied (no restart).");
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Failed to rollback tweak.");
                Logger.LogWarning($"Rollback failed: ({tweak.Id}, enable={tweakCheckbox.Checked})");
            }
            StopProgress();
        }
    }
}

