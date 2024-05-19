using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MiLauncher
{
    /// <summary>
    /// Provides static methods to handle set or list of <see cref="FileStats"/>
    /// </summary>
    internal static class FileSet
    {
        internal static List<FileStats> FilterWithCancellation
            (this IEnumerable<FileStats> sourceFiles, IEnumerable<string> patterns, CancellationToken token)
        {
            try {
                return new List<FileStats>(
                    sourceFiles.AsParallel().WithCancellation(token)
                               .Where(x => x.IsMatchAllPatterns(patterns)));
            }
            catch (OperationCanceledException) {
                // Debug.WriteLine("cancel occurs Select");
                return [];
            }
        }

        // TODO: consider to create MatchCondition class and move there
        private static bool IsMatchAllPatterns(this FileStats fileStats, IEnumerable<string> patterns)
        {
            // TODO: consider to use LINQ
            foreach (var pattern in patterns) {
                if (!(pattern[..1] switch {
                    "-" => !IsMatchPattern(fileStats.FullPathName, pattern[1..]),
                    "!" => !IsMatchPattern(fileStats.FileName, pattern[1..]),
                    "/" => IsMatchPattern(fileStats.FullPathName, pattern[1..]),
                    _ => IsMatchPattern(fileStats.FileName, pattern),
                })) {
                    return false;
                }
            }
            return true;

            static bool IsMatchPattern(string name, string pattern)
            {
                // Simple search
                if (pattern.Length < Program.appSettings.MinMigemoLength) {
                    return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
                }
                // Migemo search
                else {
                    try {
                        return Regex.IsMatch(name, pattern.ToString(), RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException) {
                        return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }

        internal static HashSet<FileStats> SearchAllFiles()
        {
            List<string> searchPaths = Program.appSettings.TargetFolders;
            return new HashSet<FileStats>(
                searchPaths.SelectMany(
                    x => DirectorySearch.EnumerateAllFileSystemEntries(x).Select(fn => new FileStats(fn))));
        }

        internal static HashSet<FileStats> SearchFilesInPath(string searchPath)
        {
            try {
                return new HashSet<FileStats>(
                    Directory.GetFileSystemEntries(searchPath).Select(fn => new FileStats(fn)));
            }
            catch (Exception e) when (e is UnauthorizedAccessException ||
                                      e is PathTooLongException ||
                                      e is IOException ||
                                      e is DirectoryNotFoundException ||
                                      e is ArgumentNullException) {
                return null;
            }
        }

        internal static HashSet<FileStats> ImportPriorityAndExecTime
                (this IEnumerable<FileStats> targetFiles, IEnumerable<FileStats> sourceFiles)
        {
            return new HashSet<FileStats>(
                targetFiles.GroupJoin(sourceFiles,
                                      x => x.FullPathName,
                                      y => y.FullPathName,
                                      (x, y) => new FileStats(x.FullPathName,
                                                              x.FileName,
                                                              x.ShortPathName,
                                                              x.UpdateTime,
                                                              y.FirstOrDefault()?.Priority,
                                                              y.FirstOrDefault()?.ExecTime)));
        }
    }
}
