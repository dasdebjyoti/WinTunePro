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

        public MainForm()
        {
            InitializeComponent();
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

            LoadCurrentValues();
            Logger.LogInfo("MainForm initialized");

            // Save window state on close
            this.FormClosing += MainForm_FormClosing;
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

        private void SetStatus(string text)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(() => lblStatus.Text = text);
            }
            else
            {
                lblStatus.Text = text;
            }
        }

        private void LoadCurrentValues()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", false);
                if (key != null)
                {
                    var v = key.GetValue("ShowSecondsInSystemClock");
                    if (v is int iv)
                    {
                        chkShowSeconds.Checked = iv != 0;
                        Logger.LogInfo($"Loaded ShowSecondsInSystemClock = {iv}");
                    }
                }
            }
            catch
            {
                Logger.LogWarning("Failed to read ShowSecondsInSystemClock from registry");
            }
        }

        private async void BtnApply_Click(object? sender, EventArgs e)
        {
            SetStatus("Applying...");
            btnApply.Enabled = false;
            btnRollback.Enabled = false;

            var tweak = new ShowSecondsTweak(chkShowSeconds.Checked);
            Logger.LogInfo($"Applying tweak {tweak.Id}, desiredValue={chkShowSeconds.Checked}");
            var ok = await tweak.ApplyAsync();

            if (ok)
            {
                SetStatus("Applied. Notifying shell...");
                Logger.LogInfo("Tweak applied; broadcasting setting change to shell");

                // Try to notify the shell of setting changes first
                var notified = BroadcastSettingChange("TraySettings");
                Logger.LogInfo($"BroadcastSettingChange returned {notified}");

                // Ask user to restart Explorer only if they want to or if notify failed
                if (notified)
                {
                    var resp = MessageBox.Show("Change applied. Explorer should update automatically. Restart Explorer now to ensure the change is visible?", "WinTunePro", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    var resp = MessageBox.Show("Could not notify Explorer of the change. Restart Explorer now to apply the setting?", "WinTunePro", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
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
            }
            else
            {
                SetStatus("Failed to apply.");
                Logger.LogWarning("Failed to apply tweak");
            }

            btnApply.Enabled = true;
            btnRollback.Enabled = true;
        }

        private async void BtnRollback_Click(object? sender, EventArgs e)
        {
            SetStatus("Rolling back...");
            btnApply.Enabled = false;
            btnRollback.Enabled = false;

            // Use the registry backup id from the tweak implementation so rollback stays in sync
            var backupId = new ShowSecondsTweak(chkShowSeconds.Checked).Id;
            Logger.LogInfo($"Attempting rollback using backup {backupId}");
            var record = RegistryBackup.Load(backupId);
            if (record == null)
            {
                SetStatus("No backup found for rollback.");
                Logger.LogWarning($"No registry backup found for {backupId}");
                MessageBox.Show("No backup was found for this tweak. Cannot rollback.", "WinTunePro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnApply.Enabled = true;
                btnRollback.Enabled = true;
                return;
            }

            var ok = await Task.Run(() => RegistryBackup.Restore(backupId));

            if (ok)
            {
                SetStatus("Rollback applied. Notifying shell...");
                Logger.LogInfo("Rollback applied; broadcasting setting change to shell");
                var notified = BroadcastSettingChange("TraySettings");
                Logger.LogInfo($"BroadcastSettingChange returned {notified}");
                if (notified)
                {
                    var resp = MessageBox.Show("Rollback applied. Restart Explorer now to ensure the change is visible?", "WinTunePro", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    var resp = MessageBox.Show("Could not notify Explorer of the rollback. Restart Explorer now to apply the setting?", "WinTunePro", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
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
                // Refresh checkbox value
                LoadCurrentValues();
            }
            else
            {
                SetStatus("Rollback failed.");
            }

            btnApply.Enabled = true;
            btnRollback.Enabled = true;
        }

        private void chkShowSeconds_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}

