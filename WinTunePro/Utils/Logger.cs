using System;
using System.IO;
using System.Text;

namespace WinTunePro.Utils
{
    public static class Logger
    {
        private static readonly object _sync = new object();
        private static readonly string LogPath;
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        static Logger()
        {
            try
            {
                var tmp = Path.GetTempPath();
                LogPath = Path.Combine(tmp, "WinTunePro.log");
            }
            catch
            {
                LogPath = "WinTunePro.log";
            }
        }

        private static void EnsureSizeLimit()
        {
            try
            {
                if (!File.Exists(LogPath)) return;
                var fi = new FileInfo(LogPath);
                if (fi.Length <= MaxBytes) return;

                // Purge content when exceeding max size
                lock (_sync)
                {
                    File.WriteAllText(LogPath, $"--- Log purged at {DateTime.UtcNow:O} ---\r\n");
                }
            }
            catch
            {
                // ignore logging errors
            }
        }

        private static void Write(string level, string message)
        {
            try
            {
                EnsureSizeLimit();
                var line = new StringBuilder();
                line.Append(DateTime.UtcNow.ToString("o"));
                line.Append(" [").Append(level).Append("] ");
                line.Append(message).Append(Environment.NewLine);

                lock (_sync)
                {
                    File.AppendAllText(LogPath, line.ToString());
                }
            }
            catch
            {
                // swallow logging exceptions
            }
        }

        public static void LogInfo(string message) => Write("INFO", message);
        public static void LogWarning(string message) => Write("WARN", message);
        public static void LogError(string message) => Write("ERROR", message);
        public static void LogException(Exception ex, string? message = null)
        {
            try
            {
                var msg = message != null ? message + " - " + ex : ex.ToString();
                Write("EXCP", msg);
            }
            catch { }
        }
    }
}
