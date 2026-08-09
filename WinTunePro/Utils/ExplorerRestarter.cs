using System;
using System.Diagnostics;
using System.Linq;

namespace WinTunePro.Utils
{
    public static class ExplorerRestarter
    {
        public static void RestartExplorer()
        {
            try
            {
                Logger.LogInfo("Restarting explorer.exe");

                // Kill all explorer processes
                var procs = Process.GetProcessesByName("explorer");
                foreach (var p in procs)
                {
                    try { p.Kill(); } catch (Exception ex) { Logger.LogWarning($"Failed to kill explorer process: {ex.Message}"); }
                }

                // Start explorer again
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                });

                Logger.LogInfo("Explorer restarted");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error restarting explorer");
            }
        }
    }
}
