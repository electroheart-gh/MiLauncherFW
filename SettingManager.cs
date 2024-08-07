﻿using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MiLauncherFW
{
    /// <summary>
    /// Provides functionality to save(load) an object to to(from) specified file with <see cref="JsonSerializer"/>."
    /// </summary>
    // Used to save(load) both a set of searched files and Application Settings
    internal class SettingManager
    {
        private static readonly JsonSerializerOptions writeOptions = new JsonSerializerOptions() {
            WriteIndented = true
        };

        public static void SaveSettings<T>(T settingsObject, string path, bool escaping = true)
        {
            if (!escaping) {
                writeOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            }

            var tempFileName = path + ".temp";
            File.WriteAllText(tempFileName, JsonSerializer.Serialize(settingsObject, writeOptions));

            try {
                File.Delete(path);
            }
            catch (Exception e) {
                Logger.LogError(e.Message);
            }

            try {
                File.Move(tempFileName, path);
            }
            catch (Exception e) {
                Logger.LogError(e.Message);
            }
            Logger.LogInfo($"Saved {path}");
        }

        public static void SaveSettingsNoEscape<T>(T settingsObject, string path)
        {
            SaveSettings(settingsObject, path, false);
        }

        public static T LoadSettings<T>(string path)
        {
            try {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            }
            catch (Exception e) {
                Logger.LogError(e.Message);
                return default;
            }
        }
    }
}
