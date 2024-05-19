using System;
using System.Collections.Generic;

namespace MiLauncher
{
    /// <summary>
    /// Stores all user settings that is supposed to be used with <see cref="SettingManager"/>
    /// </summary>
    internal class AppSettings
    {
        // TODO: Consider to use <Record> Type (Can JsonSerializer handle other than properties?)
        public List<string> TargetFolders { get; set; }
        public int MinMigemoLength { get; set; }
        public int MaxListLine { get; set; }
        public int MaxListWidth { get; set; }

        public AppSettings()
        {
            // Default Settings
            TargetFolders = [
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                ];
            MinMigemoLength = 3;
            MaxListLine = 30;
            MaxListWidth = 1000;

            // TODO: settings for Keymap 
            // TODO: settings for specific application to open file, such as sakura
        }
    }
}
