using KaoriYa.Migemo;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MiLauncherFW
{
    internal class FileNameFilter
    {
        private readonly char[] splitter = { ' ' };

        //Dictionary<SpecialFilter, char> FilterSpecifier = new Dictionary<SpecialFilter, char>() {
        //    [SpecialFilter.UnMatchName] = '!',
        //    [SpecialFilter.UnMatchPath] = '-',
        //    [SpecialFilter.MatchPath] = '/'
        //};

        // CMIC
        static readonly string unmatchPath = "-";
        static readonly string unmatchName = "!";
        static readonly string matchPath = "/";

        string[] regexFilters;

        internal FileNameFilter(string rawInput)
        {
            string[] splitInput = rawInput.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            // Added ToArray() to apply eager evaluation because lazy evaluation makes it too slow 
            regexFilters = splitInput.Select(MigemoTransform).ToArray();
        }

        static string MigemoTransform(string s)
        {
            using (var migemo = new Migemo("./Dict/migemo-dict")) {
                var specialFilters = unmatchPath + unmatchName + matchPath;
                var prefix = specialFilters.Contains(s.Substring(0, 1)) ? s.Substring(0, 1) : "";
                return s.Length - prefix.Length < Program.appSettings.MinMigemoLength ?
                    s : prefix + migemo.GetRegex(s.Substring(prefix.Length));
            };
        }

        internal bool MatchedBy(FileStats fileStats)
        {
            foreach (var filter in regexFilters) {
                var maybePrefix = filter.Substring(0, 1);
                var maybeSuffix = filter.Substring(1);
                if (maybePrefix == unmatchPath) {
                    if (IsMigemoMatch(fileStats.FullPathName, maybeSuffix)) return false;
                }
                else if (maybePrefix == unmatchName) {
                    if (IsMigemoMatch(fileStats.FileName, maybeSuffix)) return false;
                }
                else if (maybePrefix == matchPath) {
                    if (!IsMigemoMatch(fileStats.FullPathName, maybeSuffix)) return false;
                }
                else {
                    if (!IsMigemoMatch(fileStats.FileName, filter)) return false;
                }
            }
            return true;
        }

        static bool IsMigemoMatch(string name, string pattern)
        {
            // Simple matching
            if (pattern.Length < Program.appSettings.MinMigemoLength) {
                return name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) != -1;
            }
            // Migemo matching
            else {
                try {
                    return Regex.IsMatch(name, pattern.ToString(), RegexOptions.IgnoreCase);
                }
                catch (ArgumentException) {
                    return name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) != -1;
                }
            }
        }

        //enum SpecialFilter
        //{
        //    MatchPath,
        //    UnMatchName,
        //    UnMatchPath,
        //}
    }
}

