using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using RemindMe.Models;

namespace RemindMe
{
    public class TaskStore
    {
        private readonly string _dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RemindMe", "tasks.json");

        private static byte[] GetEntropy() => System.Text.Encoding.UTF8.GetBytes("RemindMeEntropy");

        public async Task<List<TodoTask>> LoadAsync()
        {
            if (!File.Exists(_dataPath)) return new List<TodoTask>();
            var data = await File.ReadAllBytesAsync(_dataPath);
            if (!OperatingSystem.IsWindows())
            {
                var json = System.Text.Encoding.UTF8.GetString(data);
                return JsonSerializer.Deserialize<List<TodoTask>>(json) ?? new();
            }

#if DEBUG
            var jsonDebug = System.Text.Encoding.UTF8.GetString(data);
            try
            {
                return JsonSerializer.Deserialize<List<TodoTask>>(jsonDebug) ?? new();
            }
            catch
            {
                data = ProtectedData.Unprotect(data, GetEntropy(), DataProtectionScope.CurrentUser);
                var json = System.Text.Encoding.UTF8.GetString(data);
                return JsonSerializer.Deserialize<List<TodoTask>>(json) ?? new();
            }
#else
            try
            {
                data = ProtectedData.Unprotect(data, GetEntropy(), DataProtectionScope.CurrentUser);
            }
            catch
            {
                // if not encrypted
            }
            var jsonRelease = System.Text.Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<List<TodoTask>>(jsonRelease) ?? new();
#endif
        }

        public async Task SaveAsync(List<TodoTask> tasks)
        {
            var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            var data = System.Text.Encoding.UTF8.GetBytes(json);
            Directory.CreateDirectory(Path.GetDirectoryName(_dataPath)!);
#if DEBUG
            await File.WriteAllBytesAsync(_dataPath, data);
#else
            if (OperatingSystem.IsWindows())
            {
                data = ProtectedData.Protect(data, GetEntropy(), DataProtectionScope.CurrentUser);
            }
            await File.WriteAllBytesAsync(_dataPath, data);
#endif
        }
    }
}
