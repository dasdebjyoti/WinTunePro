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
        // private string AppName => Application.ProductName ?? "WinTunePro";
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
                using var key = Registry.CurrentUser.OpenSubKey(ShowSecondsTweak.KeyPath, false);
                if (key != null)
                {
                    var v = key.GetValue(ShowSecondsTweak.ValueName);
                    if (v is int iv)
                    {
                        chkTweakShowSeconds.Checked = iv != 0;
                    }
                }

                // Initialize the "Animate Windows" checkbox based on the registry value
                var tweak = new MinAnimateTweak(chkTweakMinAnimate.Checked);
                chkTweakMinAnimate.Checked = tweak.GetCurrentEnabled() ?? false;
                chkTweakMinAnimate.Tag = chkTweakMinAnimate.Checked; // store initial state for comparison
                btnTweakMinAnimateApply.Enabled = false;
                btnTweakMinAnimateRollback.Enabled = tweak.IsBackupFileAvailable() ?? false;

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

        // Batch apply/rollback removed - using individual static checkboxes in designer

        // LoadCurrentValues removed: InitializeCheckboxStates() is used to initialize and refresh UI checkboxes

        private async void BtnTweakShowSecondsApply_Click(object? sender, EventArgs e)
        {
            SetStatus("Applying show seconds tweak...");
            btnTweakShowSecondsApply.Enabled = false;
            btnTweakShowSecondsRollback.Enabled = false;

            var tweak = new ShowSecondsTweak(chkTweakShowSeconds.Checked);
            Logger.LogInfo($"Applying tweak {tweak.Id}, enable={chkTweakShowSeconds.Checked}");
            StartProgress();
            var ok = await Task.Run(() => tweak.ApplyAsync());
            StopProgress();

            if (ok)
            {
                SetStatus("Tweak applied. Notifying shell. Restart Explorer or sign out to ensure full effect.");
                Logger.LogInfo("Tweak applied. Notifying shell. Restart Explorer or sign out to ensure full effect.");

                // Try to notify the shell of setting changes first
                bool notified = BroadcastSettingChange("TraySettings");
                Logger.LogInfo($"BroadcastSettingChange returned {notified}");

                // Ask user to restart Explorer only if they want to or if notify failed
                if (notified)
                {
                    var resp = MessageBox.Show(this, "Change applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (resp == DialogResult.Yes)
                    {
                        Logger.LogInfo("User agreed to restart Explorer after apply");
                        SetStatus("Restarting Explorer...");
                        await Task.Run(() => ExplorerRestarter.RestartExplorer());
                        SetStatus("Applied successfully.");
                    }
                    else
                    {
                        Logger.LogInfo("User declined to restart Explorer after apply");
                        SetStatus("Applied (no restart).");
                    }
                }
                else
                {
                    var resp = MessageBox.Show(this, "Could not notify Explorer of the rollback. Restart Explorer now to apply the setting?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (resp == DialogResult.Yes)
                    {
                        Logger.LogInfo("User agreed to restart Explorer after apply (notify failed)");
                        SetStatus("Restarting Explorer...");
                        await Task.Run(() => ExplorerRestarter.RestartExplorer());
                        SetStatus("Applied successfully.");
                    }
                    else
                    {
                        Logger.LogInfo("User declined to restart Explorer after apply (notify failed)");
                        SetStatus("Applied (notification failed; restart deferred).");
                    }
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Failed to apply.");
                Logger.LogWarning("Failed to apply tweak");
            }

            btnTweakShowSecondsApply.Enabled = true;
            btnTweakShowSecondsRollback.Enabled = true;
        }

        private async void BtnTweakShowSecondsRollback_Click(object? sender, EventArgs e)
        {
            SetStatus("Rolling back show seconds tweak...");
            btnTweakShowSecondsApply.Enabled = false;
            btnTweakShowSecondsRollback.Enabled = false;

            // Use the registry backup id from the tweak implementation so rollback stays in sync
            var tweak = new ShowSecondsTweak(false);
            Logger.LogInfo($"Rolling back tweak {tweak.Id}");
            StartProgress();
            var ok = await Task.Run(() => RegistryBackup.Restore(tweak.Id));
            StopProgress();

            if (ok)
            {
                SetStatus("Rollback applied. Notifying shell. Restart Explorer or sign out to ensure full effect.");
                Logger.LogInfo("Rollback applied. Notifying shell. Restart Explorer or sign out to ensure full effect.");
                bool notified = BroadcastSettingChange("TraySettings");
                Logger.LogInfo($"BroadcastSettingChange returned {notified}");
                if (notified)
                {
                    var resp = MessageBox.Show(this, "Rollback applied. Restart Explorer now to ensure the change is visible?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (resp == DialogResult.Yes)
                    {
                        Logger.LogInfo("User agreed to restart Explorer after rollback");
                        SetStatus("Restarting Explorer...");
                        await Task.Run(() => ExplorerRestarter.RestartExplorer());
                        SetStatus("Rollback successful.");
                    }
                    else
                    {
                        Logger.LogInfo("User declined to restart Explorer after rollback");
                        SetStatus("Rollback applied (no restart).");
                    }
                }
                else
                {
                    var resp = MessageBox.Show(this, "Could not notify Explorer of the rollback. Restart Explorer now to apply the setting?", AppInfo.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (resp == DialogResult.Yes)
                    {
                        Logger.LogInfo("User agreed to restart Explorer after rollback (notify failed)");
                        SetStatus("Restarting Explorer...");
                        await Task.Run(() => ExplorerRestarter.RestartExplorer());
                        SetStatus("Rollback successful.");
                    }
                    else
                    {
                        Logger.LogInfo("User declined to restart Explorer after rollback (notify failed)");
                        SetStatus("Rollback applied (notification failed; restart deferred).");
                    }
                }
                // Refresh checkbox values
                InitializeCheckboxStates();
            }
            else
            {
                SetStatus("Rollback failed.");
                Logger.LogWarning("Rollback failed.");
            }

            btnTweakShowSecondsApply.Enabled = true;
            btnTweakShowSecondsRollback.Enabled = true;
        }

        private async void BtnTweakMinAnimateApply_Click(object? sender, EventArgs e)
        {
            SetStatus("Applying minimize animations tweak...");
            StartProgress();
            btnTweakMinAnimateApply.Enabled = false;
            btnTweakMinAnimateRollback.Enabled = false;

            var tweak = new MinAnimateTweak(chkTweakMinAnimate.Checked);
            Logger.LogInfo($"Applying: ({tweak.Id}, enable={chkTweakMinAnimate.Checked})");
            var ok = await Task.Run(() => tweak.ApplyAsync());

            if (ok)
            {
                SetStatus("Tweak applied.");
                Logger.LogInfo($"Applied: ({tweak.Id}, enable={chkTweakMinAnimate.Checked})");

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
                Logger.LogWarning($"Apply failed: ({tweak.Id}, enable={chkTweakMinAnimate.Checked})");
            }
            StopProgress();
        }

        private async void BtnTweakMinAnimateRollback_Click(object? sender, EventArgs e)
        {
            SetStatus("Rolling back minimize animations tweak...");
            StartProgress();
            btnTweakMinAnimateApply.Enabled = false;
            btnTweakMinAnimateRollback.Enabled = false;

            var tweak = new MinAnimateTweak(false);
            Logger.LogInfo($"Rolling back: ({tweak.Id}, enable={chkTweakMinAnimate.Checked})");
            var ok = await Task.Run(() => tweak.RollbackAsync());

            if (ok)
            {
                SetStatus("Rollback applied.");
                Logger.LogInfo($"Rollback applied: ({tweak.Id}, enable={chkTweakMinAnimate.Checked})");

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
                Logger.LogWarning($"Rollback failed: ({tweak.Id}, enable={chkTweakMinAnimate.Checked})");
            }
            StopProgress();
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void chkTweakShowSeconds_CheckedChanged(object sender, EventArgs e)
        {
            bool originalState = chkTweakShowSeconds.Tag is bool b && b;
            btnTweakShowSecondsApply.Enabled = chkTweakShowSeconds.Checked != originalState;
        }
        private void chkTweakMinAnimate_CheckedChanged(object sender, EventArgs e)
        {
            bool originalState = chkTweakMinAnimate.Tag is bool b && b;
            btnTweakMinAnimateApply.Enabled = chkTweakMinAnimate.Checked != originalState;
        }

    }
}

