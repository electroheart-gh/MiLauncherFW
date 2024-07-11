using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiLauncherFW
{
    /// <summary>
    /// Provides functionality and information to control Crawl mode, 
    /// which is a mode to search files in a specified directory.
    /// </summary>
    internal class CrawlMode
    {
        //
        // Properties
        //
        internal string CrawlPath { get; private set; }
        internal HashSet<FileStats> CrawlFileSet { get; set; }
        internal (string, string) Captions { get; private set; }
        public ModeStatus Status { get; private set; }

        //
        //Constructor
        //
        private CrawlMode(string path)
        {
            CrawlPath = path;
            // CMICst
            Captions = (path is null) ? (null, null) : ("CrawlMode", path);
            CrawlFileSet = FileSet.SearchFilesInPath(path);
            Status = (CrawlFileSet is null) ? ModeStatus.Defective : ModeStatus.Immature;
        }

        //
        // Methods
        //
        internal static CrawlMode Crawl(string path)
        {
            if (path is null) return null;
            var newCrawlMode = new CrawlMode(path);
            return (newCrawlMode.Status != ModeStatus.Defective) ? newCrawlMode : null;
        }

        internal void SyncFileSetMutually(HashSet<FileStats> sourceFileSet)
        {
            CrawlFileSet = CrawlFileSet.ImportPriorityAndExecTime(sourceFileSet);
            if (Program.appSettings.TargetFolders.Any(x => CrawlPath.StartsWith(x))) {
                sourceFileSet.RemoveWhere(x => Path.GetDirectoryName(x.FullPathName) == CrawlPath);
                sourceFileSet.UnionWith(CrawlFileSet);
            }
            Status = ModeStatus.Active;
        }
    }
}
