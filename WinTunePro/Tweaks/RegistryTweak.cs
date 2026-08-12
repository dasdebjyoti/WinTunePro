using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinTunePro.Utils;

namespace WinTunePro.Tweaks
{
    public abstract class RegistryTweak : ITweak
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract bool RequiresElevation { get; }

        public abstract Task<bool> ApplyAsync();
        public abstract Task<bool> RollbackAsync();

        // Win32 interop for broadcasting setting changes
        private const uint HWND_BROADCAST = 0xFFFF;
        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam,
            uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        /// <summary>
        /// Broadcasts a WM_SETTINGCHANGE message to notify Windows of configuration changes.
        /// </summary>
        /// <param name="area">The setting area name (e.g., "Windows", "TraySettings", "Environment")</param>
        /// <returns>True if the broadcast succeeded, false otherwise</returns>
        protected static bool BroadcastSettingChange(string area)
        {
            try
            {
                SendMessageTimeout((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, area, SMTO_ABORTIFHUNG, 5000, out _);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "BroadcastSettingChange failed");
                return false;
            }
        }
    }
}
