using System;
using System.Collections.Generic;
using System.Drawing;

namespace MiLauncherFW
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
        public string CmdBoxFontName { get; set; }
        public float CmdBoxFontSize { get; set; }
        public string ListViewFontName { get; set; }
        public float ListViewFontSize { get; set; }

        public AppSettings()
        {
            // Default Settings
            TargetFolders = new List<string> {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            MinMigemoLength = 3;
            MaxListLine = 30;
            MaxListWidth = 1000;

            CmdBoxFontName = "Meiryo UI";
            CmdBoxFontSize = 12F;
            ListViewFontName = "Meiryo UI";
            ListViewFontSize = 9.75F;

            // TODO: settings for Keymap 
            // TODO: settings for specific application to open file, such as sakura
        }
    }
}
