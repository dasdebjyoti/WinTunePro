using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using WinTunePro.Utils;

namespace WinTunePro.Tweaks
{
    public class TweakShowSeconds : RegistryTweak
    {
        public override string Id => "show-seconds-clock";
        public override string Name => "Seconds in taskbar clock";
        public override string Description => "Toggle showing seconds in the Windows taskbar clock (HKCU).";
        public override bool RequiresElevation => false;

        private const string KeyPathShowSec = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";
        private const string KeyValueShowSec = "ShowSecondsInSystemClock";
        private const RegistryValueKind KeyKindShowSec = RegistryValueKind.DWord;

        private readonly bool _enable;

        public TweakShowSeconds(bool enable)
        {
            _enable = enable;
        }

        /// <summary>
        /// Read the current value from registry and return whether the tweak is enabled.
        /// Returns null if the value cannot be determined.
        /// </summary>
        public bool? GetCurrentEnabled()
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(KeyPathShowSec, writable: false);
                if (k == null) return null;
                var val = k.GetValue(KeyValueShowSec);

                // Registry value is a DWORD (REG_DWORD), so check numeric types first
                if (val is int iv) return iv != 0;
                if (val is long lv) return lv != 0L;

                // Fallback: handle string representation (in case it was set as string)
                if (val is string s)
                {
                    if (int.TryParse(s, out int parsed))
                        return parsed != 0;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to read TaskbarAnimations current state");
                return null;
            }
        }

        /// <summary>
        /// Check whether a backup file for the current value exists.
        /// Returns null if there was a failure.
        /// </summary>
        public bool? IsBackupFileAvailable()
        {
            try
            {
                bool isAvailable = RegistryBackup.Exists(Id);
                return isAvailable;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to read backup file");
                return null;
            }
        }

        public override Task<bool> ApplyAsync()
        {
            try
            {
                Logger.LogInfo($"Applying: (tweak {Id}, enable={_enable})");

                // Backup current value
                RegistryBackup.Save(Id, Registry.CurrentUser, KeyPathShowSec, KeyValueShowSec);

                // Set Registry value (DWORD)
                using (var k = Registry.CurrentUser.CreateSubKey(KeyPathShowSec))
                {
                    k.SetValue(KeyValueShowSec, _enable ? 1 : 0, KeyKindShowSec);
                }
                Logger.LogInfo($"Applied: (tweak {Id}, enable={_enable})");

                // Try to notify the shell of setting changes
                bool notified = BroadcastSettingChange("TraySettings");
                Logger.LogInfo($"BroadcastSettingChange returned {notified}");

                if (notified)
                {
                    // Add code if there are actions to take when notification succeeds, if needed
                }
                else
                {
                    // Add code if there are actions to take when notification fails, if needed
                    // Explorer should be restarted for the change to take effect.
                }

                Logger.LogInfo($"Applied: (tweak {Id}, enable={_enable}, notified={notified})");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Apply failed: (tweak {Id}, enable={_enable})");
                return Task.FromResult(false);
            }
        }

        public override Task<bool> RollbackAsync()
        {
            try
            {
                Logger.LogInfo($"Rolling back: (tweak {Id}, enable={_enable})");
                bool ok = RegistryBackup.Restore(Id);
                Logger.LogInfo($"Rollback result: (tweak {Id}, enable={_enable}, success={ok})");

                if (ok)
                {
                    // Try to notify the shell of setting changes
                    bool notified = BroadcastSettingChange("TraySettings");
                    Logger.LogInfo($"BroadcastSettingChange returned {notified}");

                    if (notified)
                    {
                        // Add code if there are actions to take when notification succeeds, if needed
                    }
                    else
                    {
                        // Add code if there are actions to take when notification fails, if needed
                        // Explorer should be restarted for the change to take effect.
                    }

                    Logger.LogInfo($"Rollback applied: (tweak {Id}, enable={_enable}, notified={notified})");
                    return Task.FromResult(true);
                }
                else
                {
                    Logger.LogWarning($"Rollback failed: (tweak {Id}, enable={_enable})");
                    return Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Rollback failed: (tweak {Id}, enable={_enable})");
                return Task.FromResult(false);
            }
        }
    }
}
