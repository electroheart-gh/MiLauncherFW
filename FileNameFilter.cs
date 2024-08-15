using KaoriYa.Migemo;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MiLauncherFW
{
    internal class FileNameFilter
    {
        private static readonly Migemo migemo = new Migemo("./Dict/migemo-dict");

        private static readonly char[] splitter = { ' ' };
        private static readonly string unmatchPath = "-";
        private static readonly string unmatchName = "!";
        private static readonly string matchPath = "/";

        private readonly string[] regexFilters;

        internal FileNameFilter(string rawInput)
        {
            string[] splitInput = rawInput.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            // Added ToArray() to apply eager evaluation because lazy evaluation makes it too slow 
            regexFilters = splitInput.Select(MigemoTransform).ToArray();
        }

        static string MigemoTransform(string s)
        {
            var specialFilters = unmatchPath + unmatchName + matchPath;
            var prefix = specialFilters.Contains(s.Substring(0, 1)) ? s.Substring(0, 1) : "";
            return s.Length - prefix.Length < Program.appSettings.MinMigemoLength ?
                s : prefix + migemo.GetRegex(s.Substring(prefix.Length));
        }

        internal bool MatchedBy(FileStats fileStats)
        {
            foreach (var filter in regexFilters) {
                var maybePrefix = filter.Substring(0, 1);

                if (maybePrefix == unmatchPath) {
                    if (Contains(fileStats.FullPathName, filter.Substring(1))) return false;
                }
                else if (maybePrefix == unmatchName) {
                    if (Contains(fileStats.FileName, filter.Substring(1))) return false;
                }
                else if (maybePrefix == matchPath) {
                    if (!Contains(fileStats.FullPathName, filter.Substring(1))) return false;
                }
                else {
                    if (!Contains(fileStats.FileName, filter)) return false;
                }
            }
            return true;
        }

        static bool Contains(string name, string pattern)
        {
            // Simple matching
            if (pattern.Length < Program.appSettings.MinMigemoLength) {
                return name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) != -1;
            }
            // Migemo matching - catch if migemoTransform failed to create regex
            else {
                try {
                    return Regex.IsMatch(name, pattern.ToString(), RegexOptions.IgnoreCase);
                }
                catch (ArgumentException) {
                    return name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) != -1;
                }
            }
        }
    }
}

