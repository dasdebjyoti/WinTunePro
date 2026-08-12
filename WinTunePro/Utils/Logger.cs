using System;
using System.IO;
using System.Runtime.CompilerServices;
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
                LogPath = Path.Combine(tmp, AppInfo.Name + ".log"); // "WinTunePro.log");
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
                    File.WriteAllText(LogPath, $"--- Log purged at {DateTime.Now:O} ---\r\n");
                }
            }
            catch
            {
                // ignore logging errors
            }
        }

        private static void Write(string level, string message, string memberName = "", string file = "", int lineNo = 0)
        {
            try
            {
                EnsureSizeLimit();

                // Get class name from file path
                string className = "";
                if (!string.IsNullOrEmpty(file))
                {
                    var fi = new FileInfo(file);
                    className = Path.GetFileNameWithoutExtension(fi.Name);
                }

                var line = new StringBuilder();
                line.Append(DateTime.Now.ToString("o"));
                line.Append(" [").Append(level).Append("] ");
                if (!string.IsNullOrEmpty(memberName))
                {
                    line.Append(" [").Append(className).Append('.').Append(memberName).Append("] ");
                }
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

        public static void LogInfo(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNo = 0) => Write("INFO", message, memberName, file, lineNo);
        public static void LogWarning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNo = 0) => Write("WARN", message, memberName, file, lineNo);
        public static void LogError(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNo = 0) => Write("ERROR", message, memberName, file, lineNo);
        public static void LogException(Exception ex, string? message = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNo = 0)
        {
            try
            {
                var msg = message != null ? message + " - " + ex : ex.ToString();
                Write("EXCP", msg, memberName, file, lineNo);
            }
            catch { }
        }
    }
}
