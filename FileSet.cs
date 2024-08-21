using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MiLauncherFW
{
    /// <summary>
    /// Provides static methods to handle set or list of <see cref="FileStats"/>
    /// </summary>
    internal static class FileSet
    {
        // Returns List<> for intended use and performance
        internal static List<FileStats> FilterWithCancellation
            (this IEnumerable<FileStats> sourceFiles, FileNameFilter filters, CancellationToken token)
        {
            try {
                return new List<FileStats>(
                    sourceFiles.AsParallel().WithCancellation(token)
                               .Where(x => filters.MatchedBy(x)));
            }
            catch (OperationCanceledException) {
                // Debug.WriteLine("cancel occurs Select");
                return new List<FileStats>();
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

        internal static HashSet<FileStats> SearchAllFiles()
        {
            List<string> searchPaths = Program.appSettings.TargetFolders;
            return new HashSet<FileStats>(
                searchPaths.SelectMany(
                    x => EnumerateAllFileSystemEntries(x).Select(fn => new FileStats(fn))));
        }
        private static IEnumerable<string> EnumerateAllFileSystemEntries(string path)
        {
            return EnumerateAllFileSystemEntries(path, "*");
        }
        private static IEnumerable<string> EnumerateAllFileSystemEntries(string path, string searchPattern)
        {
            var files = Enumerable.Empty<string>();
            try {
                //files = System.IO.Directory.EnumerateFiles(path, searchPattern);
                files = Directory.EnumerateFileSystemEntries(path, searchPattern);
            }
            catch (UnauthorizedAccessException) {
            }
            try {
                files = Directory.EnumerateDirectories(path)
                    .Aggregate(files, (a, v) => a.Union(EnumerateAllFileSystemEntries(v, searchPattern)));
            }
            catch (UnauthorizedAccessException) {
            }
            return files;
        }
    }
}
