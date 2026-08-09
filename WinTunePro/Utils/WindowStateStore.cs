using System;
using System.IO;
using System.Text.Json;
using System.Drawing;

namespace WinTunePro.Utils
{
    public class WindowStateModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsMaximized { get; set; }
    }

    public static class WindowStateStore
    {
        private static string GetPath()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinTunePro");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "window.json");
        }

        public static void Save(WindowStateModel model)
        {
            try
            {
                var path = GetPath();
                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Logger.LogInfo($"Window state saved to {path}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed saving window state");
            }
        }

        public static WindowStateModel? Load()
        {
            try
            {
                var path = GetPath();
                if (!File.Exists(path)) return null;
                var json = File.ReadAllText(path);
                var model = JsonSerializer.Deserialize<WindowStateModel>(json);
                Logger.LogInfo($"Window state loaded from {path}");
                return model;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed loading window state");
                return null;
            }
        }
    }
}
