using System;
using System.IO;
using System.Text.Json;

namespace MiLauncher
{
    /// <summary>
    /// Provides functionality to save(load) an object to to(from) specified file with <see cref="JsonSerializer"/>."
    /// </summary>
    // Used to save(load) both a set of searched files and Application Settings
    internal class SettingManager
    {
        private static readonly JsonSerializerOptions s_writeOptions = new() {
            WriteIndented = true
        };

        public static void SaveSettings<T>(T settingsObject, string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(settingsObject, s_writeOptions));
        }

        public static T LoadSettings<T>(string path)
        {
            try {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            }
            catch (Exception ex) {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return default;
            }
        }
    }
}
