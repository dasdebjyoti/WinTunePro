using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;

namespace WinTunePro.Utils
{
    public class RegistryBackupRecord
    {
        public string HiveName { get; set; } = string.Empty;
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

        public static string Save(string id, RegistryKey hk, string keyPath, string valueName)
        {
            // Determine the hive name from the RegistryKey
            string hiveName = GetHiveName(hk);

            var record = new RegistryBackupRecord 
            { 
                HiveName = hiveName,
                KeyPath = keyPath, 
                ValueName = valueName 
            };
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
                Logger.LogInfo($"Registry backup saved: {path} for {hiveName}\\{keyPath}::{valueName}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Failed to write registry backup {path}");
            }

            return path;
        }

        private static string GetHiveName(RegistryKey hk)
        {
            // Match the RegistryKey against known hives
            if (hk == Registry.CurrentUser || hk.Name.StartsWith("HKEY_CURRENT_USER"))
                return "HKEY_CURRENT_USER";
            if (hk == Registry.LocalMachine || hk.Name.StartsWith("HKEY_LOCAL_MACHINE"))
                return "HKEY_LOCAL_MACHINE";
            if (hk == Registry.ClassesRoot || hk.Name.StartsWith("HKEY_CLASSES_ROOT"))
                return "HKEY_CLASSES_ROOT";
            if (hk == Registry.Users || hk.Name.StartsWith("HKEY_USERS"))
                return "HKEY_USERS";
            if (hk == Registry.CurrentConfig || hk.Name.StartsWith("HKEY_CURRENT_CONFIG"))
                return "HKEY_CURRENT_CONFIG";

            // Fallback - extract from the key's name property
            var name = hk.Name;
            var parts = name.Split('\\');
            return parts.Length > 0 ? parts[0] : "HKEY_CURRENT_USER";
        }

        private static RegistryKey GetHiveKey(string hiveName)
        {
            return hiveName switch
            {
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_USERS" => Registry.Users,
                "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                _ => Registry.CurrentUser // Fallback
            };
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

        /// <summary>
        /// Returns true if a backup file exists for the given id.
        /// </summary>
        public static bool Exists(string id)
        {
            try
            {
                var path = Path.Combine(GetBackupFolder(), id + ".json");
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to check existence of backup for {id}: {ex.Message}");
                return false;
            }
        }

        public static bool Delete(string id)
        {
            try
            {
                var folder = GetBackupFolder();
                var path = Path.Combine(folder, id + ".json");
                if (!File.Exists(path)) return false;

                // Ensure archive folder exists
                var archiveFolder = Path.Combine(folder, "archive");
                Directory.CreateDirectory(archiveFolder);

                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var archivedName = id + "_" + timestamp + ".json";
                var archivedPath = Path.Combine(archiveFolder, archivedName);

                File.Move(path, archivedPath);
                Logger.LogInfo($"Archived registry backup file: {archivedPath}");

                // Cleanup: keep only the last 5 archives for this id
                try
                {
                    var pattern = id + "_" + "*.json";
                    var archivedFiles = Directory.GetFiles(archiveFolder, id + "_*.json");
                    // Order by filename descending (timestamp in name) and skip newest 5
                    var toDelete = archivedFiles
                        .OrderByDescending(f => f)
                        .Skip(5)
                        .ToList();

                    foreach (var f in toDelete)
                    {
                        try
                        {
                            File.Delete(f);
                            Logger.LogInfo($"Pruned old archive: {f}");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning($"Failed to prune archive file {f}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Failed to cleanup archives for {id}: {ex.Message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to archive registry backup file for {id}: {ex.Message}");
                return false;
            }
        }

        public static bool Restore(string id)
        {
            var record = Load(id);
            if (record == null)
            {
                Logger.LogWarning($"No registry backup found for {id}. Cannot rollback.");
                return false;
            }

            try
            {
                // Use the stored hive instead of hardcoding Registry.CurrentUser
                var hiveKey = GetHiveKey(record.HiveName);
                using var k = hiveKey.CreateSubKey(record.KeyPath);
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

                // Delete the backup file after successful restore
                Delete(id);

                Logger.LogInfo($"Registry restored for {record.HiveName}\\{record.KeyPath}::{record.ValueName}");
                return true;
            }
            catch
            {
                Logger.LogWarning($"Failed to restore registry for {record.HiveName}\\{record.KeyPath}::{record.ValueName}");
                return false;
            }
        }
    }
}
