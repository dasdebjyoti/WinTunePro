using System.Threading.Tasks;
using Microsoft.Win32;
using WinTunePro.Utils;
using System;

namespace WinTunePro.Tweaks
{
    public class ShowSecondsTweak : RegistryTweak
    {
        public override string Id => "show-seconds-clock";
        public override string Name => "Show seconds in taskbar clock";
        public override string Description => "Toggle showing seconds in the Windows taskbar clock (HKCU).";
        public override bool RequiresElevation => false;

        public const string KeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";
        public const string ValueName = "ShowSecondsInSystemClock";

        private readonly int _desiredValue;

        public ShowSecondsTweak(bool enable)
        {
            _desiredValue = enable ? 1 : 0;
        }

        public override Task<bool> ApplyAsync()
        {
            try
            {
                Logger.LogInfo($"Applying tweak {Id}: setting {KeyPath}::{ValueName} to {_desiredValue}");
                // Backup current value
                RegistryBackup.Save(Id, Registry.CurrentUser, KeyPath, ValueName);

                using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
                key.SetValue(ValueName, _desiredValue, RegistryValueKind.DWord);
                Logger.LogInfo($"Tweak {Id} applied");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Failed to apply tweak {Id}");
                return Task.FromResult(false);
            }
        }

        public override Task<bool> RollbackAsync()
        {
            try
            {
                Logger.LogInfo($"Rolling back tweak {Id}");
                var ok = RegistryBackup.Restore(Id);
                Logger.LogInfo($"Rollback {Id} result: {ok}");
                return Task.FromResult(ok);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Failed rollback for tweak {Id}");
                return Task.FromResult(false);
            }
        }
    }
}
