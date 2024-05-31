using KaoriYa.Migemo;
using MiLauncherFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiLauncherFW
{
    internal class FileNameFilter
    {
        string[] regexFilters;

        private readonly char[] splitter = { ' ' };

        internal FileNameFilter(string rawInput)
        {
            string[] splitInput = rawInput.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            // Added ToArray() to apply eager evaluation because lazy evaluation makes it too slow 
            regexFilters = splitInput.Select(MigemoTransform).ToArray();
        }

        static string MigemoTransform(string s)
        {
            using (var migemo = new Migemo("./Dict/migemo-dict")) {
                // TODO: Consider to make MatchCondition class,
                // which should have a method to parse string to select condition
                var prefix = "-!/".Contains(s.Substring(0, 1)) ? s.Substring(0, 1) : "";
                return s.Length - prefix.Length < Program.appSettings.MinMigemoLength ?
                    s : prefix + migemo.GetRegex(s.Substring(prefix.Length));
            };
        }

        internal bool MatchedBy(FileStats fileStats)
        {
            foreach (var filter in regexFilters) {
                switch (filter.Substring(0, 1)) {
                    case "-":
                        if (IsMigemoMatch(fileStats.FullPathName, filter.Substring(1))) return false;
                        break;
                    case "!":
                        if (IsMigemoMatch(fileStats.FileName, filter.Substring(1))) return false;
                        break;
                    case "/":
                        if (!IsMigemoMatch(fileStats.FullPathName, filter.Substring(1))) return false;
                        break;
                    default:
                        if (!IsMigemoMatch(fileStats.FileName, filter)) return false;
                        break;
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
    }
}

