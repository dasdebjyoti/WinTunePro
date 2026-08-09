using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;

namespace WinTunePro.Utils
{
    public class RegistryBackupRecord
    {
        public string KeyPath { get; set; } = string.Empty;
        public string ValueName { get; set; } = string.Empty;
        public object? PreviousValue { get; set; }
        public RegistryValueKind? ValueKind { get; set; }
    }

    public static class RegistryBackup
    {
        private static string GetBackupFolder()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinTunePro", "backups");
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static string Save(string id, string keyPath, string valueName)
        {
            var hk = Registry.CurrentUser;
            var record = new RegistryBackupRecord { KeyPath = keyPath, ValueName = valueName };
            try
            {
                using var k = hk.OpenSubKey(keyPath, writable: false);
                if (k != null)
                {
                    record.PreviousValue = k.GetValue(valueName);
                    record.ValueKind = k.GetValueKind(valueName);
                }
            }
            catch { }

            var path = Path.Combine(GetBackupFolder(), id + ".json");
            var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                File.WriteAllText(path, json);
                Logger.LogInfo($"Registry backup saved: {path} for {keyPath}::{valueName}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Failed to write registry backup {path}");
            }

            return path;
        }

        public static RegistryBackupRecord? Load(string id)
        {
            var path = Path.Combine(GetBackupFolder(), id + ".json");
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            try
            {
                var rec = JsonSerializer.Deserialize<RegistryBackupRecord>(json);
                Logger.LogInfo($"Loaded registry backup: {path}");
                return rec;
            }
            catch { return null; }
        }

        public static bool Restore(string id)
        {
            var record = Load(id);
            if (record == null) return false;

            try
            {
                using var k = Registry.CurrentUser.CreateSubKey(record.KeyPath);
                if (record.PreviousValue == null)
                {
                    k.DeleteValue(record.ValueName, throwOnMissingValue: false);
                }
                else
                {
                    object? valueToSet = record.PreviousValue;

                    // Handle JSON deserialization types (JsonElement) and convert to CLR types appropriate for Registry
                    if (record.PreviousValue is JsonElement je)
                    {
                        try
                        {
                            if (record.ValueKind.HasValue)
                            {
                                switch (record.ValueKind.Value)
                                {
                                    case RegistryValueKind.DWord:
                                        valueToSet = je.ValueKind == JsonValueKind.Number ? je.GetInt32() : int.Parse(je.GetString() ?? "0");
                                        break;
                                    case RegistryValueKind.QWord:
                                        valueToSet = je.ValueKind == JsonValueKind.Number ? je.GetInt64() : long.Parse(je.GetString() ?? "0");
                                        break;
                                    case RegistryValueKind.String:
                                    case RegistryValueKind.ExpandString:
                                        valueToSet = je.GetString();
                                        break;
                                    case RegistryValueKind.Binary:
                                        var s = je.GetString();
                                        valueToSet = s != null ? Convert.FromBase64String(s) : Array.Empty<byte>();
                                        break;
                                    case RegistryValueKind.MultiString:
                                        if (je.ValueKind == JsonValueKind.Array)
                                        {
                                            var arr = je.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();
                                            valueToSet = arr;
                                        }
                                        else
                                        {
                                            valueToSet = new string[] { je.GetString() ?? string.Empty };
                                        }
                                        break;
                                    default:
                                        // Fallback: try number then string
                                        if (je.ValueKind == JsonValueKind.Number)
                                            valueToSet = je.GetInt32();
                                        else
                                            valueToSet = je.GetString();
                                        break;
                                }
                            }
                            else
                            {
                                // No kind saved - infer
                                if (je.ValueKind == JsonValueKind.Number) valueToSet = je.GetInt32();
                                else if (je.ValueKind == JsonValueKind.Array) valueToSet = je.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();
                                else valueToSet = je.GetString();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex, "Failed converting JsonElement to registry value");
                            // fallback to string representation
                            valueToSet = je.ToString();
                        }
                    }

                    if (record.ValueKind.HasValue)
                        k.SetValue(record.ValueName, valueToSet!, record.ValueKind.Value);
                    else
                        k.SetValue(record.ValueName, valueToSet!);
                }

                Logger.LogInfo($"Registry restored for {record.KeyPath}::{record.ValueName}");
                return true;
            }
            catch
            {
                Logger.LogWarning($"Failed to restore registry for {record.KeyPath}::{record.ValueName}");
                return false;
            }
        }
    }
}
