﻿using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MiLauncherFW
{
    /// <summary>
    /// Stores information of each file and provides methods to access them.
    /// Supposed to be used with <see cref="SettingManager"/>
    /// </summary>
    internal class FileStats
    {
        // JsonSerializer requires 'set' or 'private set' with [JsonInclude] 
        [JsonInclude]
        public string FullPathName { get; private set; }
        [JsonInclude]
        public string FileName { get; private set; }
        [JsonInclude]
        public string ShortPathName { get; private set; }
        [JsonInclude]
        public DateTime UpdateTime { get; private set; }
        [JsonInclude]
        public int Priority { get; set; }
        [JsonInclude]
        public DateTime ExecTime { get; set; }

        public FileStats()
        {
        }
        public FileStats(string pathName)
        {
            var dirSuffix = Directory.Exists(pathName) ? "\\" : "";
            FullPathName = pathName + dirSuffix;
            FileName = Path.GetFileName(pathName) + dirSuffix;

            // Considering balance of response, performance, file size and accuracy, 
            // GetShortenedString() estimates width without DirectorySeparation (giving up accuracy)
            // FileStats() stores null if it is not shortened  
            // listView_RetrieveVirtualItem() controls text of list item with DirectorySeparation,
            // because AutoResizeColumns() seems to check return of RetrieveVirtualItem()
            // listView_DrawItem() controls color of item text
            ShortPathName = GetShortenedString(pathName + dirSuffix);

            UpdateTime = File.GetLastWriteTime(pathName);
            Priority = 0;
            ExecTime = default;
        }
        public FileStats(string pathName, string fileName, string shortPathName, DateTime updateTime, int? priority, DateTime? execTime)
        {
            FullPathName = pathName;
            FileName = fileName;
            ShortPathName = shortPathName;
            UpdateTime = updateTime;
            Priority = priority ?? 0;
            ExecTime = execTime ?? default;
        }

        internal object SortValue(SortKeyOption key)
        {
            switch (key) {
                case SortKeyOption.FullPathName:
                    return FullPathName;
                case SortKeyOption.UpdateTime:
                    return UpdateTime;
                case SortKeyOption.ExecTime:
                    return ExecTime;
                default:
                    return Priority;
            }
        }

        internal static string GetShortenedString(string str, int offset = 0)
        {
            var fontName = Program.appSettings.ListViewFontName;
            var fontSize = Program.appSettings.ListViewFontSize;
            var font = new Font(fontName, fontSize);

            var realMaxWidth = Program.appSettings.MaxListWidth - offset;

            if (TextRenderer.MeasureText(str, font).Width < realMaxWidth) return null;

            while (TextRenderer.MeasureText(str + "...", font).Width > realMaxWidth) {
                str = str.Substring(1);
            }
            return "..." + str;
        }

        internal static string ParentPathName(string path)
        {
            var match = Regex.Match(path, @"^(?<parent>.*\\)(?<base>[^\\]+\\?)$");
            return match.Success ? match.Groups["parent"].Value : null;
        }
    }

    public enum SortKeyOption
    {
        Priority,
        ExecTime,
        UpdateTime,
        FullPathName,
    }
}
