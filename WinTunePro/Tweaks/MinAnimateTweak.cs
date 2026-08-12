using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using WinTunePro.Utils;

namespace WinTunePro.Tweaks
{
    public class MinAnimateTweak : RegistryTweak
    {
        public override string Id => "winanimation";
        public override string Name => "Window animations";
        public override string Description => "Enable common UI animations (MinAnimate) when windows are minimized or maximized.";
        public override bool RequiresElevation => false;

        // Though articles on the web say that MinAnimate should exist in HKEY_CURRENT_USER\Control Panel\Desktop,
        // on my Windows 11 Home Version 2SH2 (OS Build 26200.8875) it actually exists in HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics.
        // Microsoft Copilot says: Windows 11 stores MinAnimate in two different registry locations, but only one of them is actually used by the system.
        //                         Windows 11 reads the animation setting from: HKEY_CURRENT_USER\Control Panel\Desktop
        //                         The value in the second location HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics exists, but Windows 11 does not use it anymore.
        //                         It’s a legacy Windows 95/98/XP location. Microsoft kept it for backward compatibility.
        // TODO: Check if this is a version-specific issue and adjust the path accordingly. For now, we'll use the path that works on my system.
        public const string KeyMinAnimatePath = "Control Panel\\Desktop\\WindowMetrics";
        public const string KeyMinAnimateName = "MinAnimate"; // REG_SZ "0"/"1"

        private const string KeyTaskbarPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";
        private const string KeyTaskbarName = "TaskbarAnimations"; // REG_DWORD 0/1

        private readonly bool _enable;

        public MinAnimateTweak(bool enable)
        {
            _enable = enable;
        }

        /// <summary>
        /// Read the current MinAnimate value from registry and return whether animations are enabled.
        /// Returns null if the value cannot be determined.
        /// </summary>
        public bool? GetCurrentEnabled()
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(KeyMinAnimatePath, writable: false);
                if (k == null) return null;
                var val = k.GetValue(KeyMinAnimateName);
                if (val is string s)
                {
                    if (s == "1") return true;
                    if (s == "0") return false;
                }

                // If stored as numeric in some systems
                if (val is int iv) return iv != 0;
                if (val is long lv) return lv != 0L;

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to read MinAnimate current state");
                return null;
            }
        }

        /// <summary>
        /// Check whether a backup file for the current MinAnimate value exists.
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
                Logger.LogException(ex, "Failed to read MinAnimate current state");
                return null;
            }
        }

        public override Task<bool> ApplyAsync()
        {
            try
            {
                Logger.LogInfo($"Applying: (tweak {Id}, enable={_enable})");

                // Backup both values
                RegistryBackup.Save(Id, Registry.CurrentUser, KeyMinAnimatePath, KeyMinAnimateName);

                // Set MinAnimate (string)
                using (var k = Registry.CurrentUser.CreateSubKey(KeyMinAnimatePath))
                {
                    k.SetValue(KeyMinAnimateName, _enable ? "1" : "0", RegistryValueKind.String);
                }

                // Set TaskbarAnimations (DWORD)
                using (var k = Registry.CurrentUser.CreateSubKey(KeyTaskbarPath))
                {
//                    k.SetValue(KeyTaskbarName, _enable ? 1 : 0, RegistryValueKind.DWord);
                }

                Logger.LogInfo($"Applied: (tweak {Id}, enable={_enable})");
                // Apply immediately for current session
                var enable = _enable; // explicit local variable for clarity
                ApplyAnimationImmediately(enable);

                // Try to notify the shell of setting changes
                bool notified = BroadcastSettingChange("Windows");
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
                    // Read current MinAnimate value and apply immediately
                    try
                    {
                        using var k = Registry.CurrentUser.OpenSubKey(KeyMinAnimatePath, writable: false);
                        if (k != null)
                        {
                            var val = k.GetValue(KeyMinAnimateName);
                            if (val is string s)
                            {
                                var enable = s == "1";
                                ApplyAnimationImmediately(enable);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, $"Apply animation after rollback failed: (tweak {Id}, enable={_enable})");
                        return Task.FromResult(false);
                    }

                    // Try to notify the shell of setting changes
                    bool notified = BroadcastSettingChange("Windows");
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

        // P/Invoke for applying animation state immediately
        [StructLayout(LayoutKind.Sequential)]
        private struct ANIMATIONINFO
        {
            public uint cbSize;
            public int iMinAnimate;
        }

        private const uint SPI_SETANIMATION = 0x0049;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref ANIMATIONINFO pvParam, uint fWinIni);

        private static void ApplyAnimationImmediately(bool enable)
        {
            try
            {
                var ai = new ANIMATIONINFO { cbSize = (uint)Marshal.SizeOf<ANIMATIONINFO>(), iMinAnimate = enable ? 1 : 0 };
                var ok = SystemParametersInfo(SPI_SETANIMATION, 0, ref ai, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                Logger.LogInfo($"SystemParametersInfo SPI_SETANIMATION ok={ok}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "ApplyAnimationImmediately failed");
            }
        }
    }
}
