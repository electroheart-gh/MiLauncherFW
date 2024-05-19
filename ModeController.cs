using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiLauncher
{
    /// <summary>
    /// Provides functionality to integrates control of <see cref="CrawlMode"/>,
    /// <see cref="RestoreMode"/> and normal mode 
    /// </summary>
    internal class ModeController
    {
        // Mode and Status
        // CrawlMode: Null, Defective, Active
        // RestoreMode: Null, Defective, Prepared, Active
        private CrawlMode crawlMode;
        private RestoreMode restoreMode;

        internal ModeController()
        {
        }

        //
        // Methods for Crawl mode
        //
        internal bool CrawlUp(string itemPath, HashSet<FileStats> sourceFileSet)
        {
            // Returns false when no change needed
            // If Crawl mode active, use CrawlPath instead of itemPath
            if (IsCrawlMode()) {
                var upperPath = Path.GetDirectoryName(crawlMode.CrawlPath);
                CrawlMode crawlResult = CrawlMode.Crawl(upperPath, sourceFileSet);
                crawlMode = (crawlResult?.Status == ModeStatus.Active) ? crawlResult : crawlMode;
                return crawlResult is not null;
            }
            else {
                crawlMode = CrawlMode.Crawl(Path.GetDirectoryName(itemPath), sourceFileSet);
                return crawlMode is not null;
            }
        }
        internal bool CrawlDown(string itemPath, HashSet<FileStats> sourceFileSet)
        {
            CrawlMode crawlResult = CrawlMode.Crawl(itemPath, sourceFileSet);
            crawlMode = (crawlResult?.Status == ModeStatus.Active) ? crawlResult : crawlMode;
            return crawlMode is not null;
        }
        internal bool IsCrawlMode()
        {
            return crawlMode?.Status == ModeStatus.Active;
        }
        internal (string, string) GetCrawlCaptions()
        {
            return crawlMode.Captions;
        }

        internal HashSet<FileStats> GetCrawlFileSet()
        {
            return crawlMode?.CrawlFileSet;
        }

        internal void ExitCrawl()
        {
            // TODO: consider to dispose instance
            crawlMode = null;
        }

        //
        // Methods for Restore mode
        //
        internal bool IsRestoreMode()
        {
            return restoreMode?.Status == ModeStatus.Active;
        }
        internal bool IsRestorePrepared()
        {
            return restoreMode?.Status == ModeStatus.Prepared || restoreMode?.Status == ModeStatus.Active;
        }
        internal void PrepareRestore(string text, int index, SortKeyOption sortKey, List<FileStats> items)
        {
            restoreMode = new RestoreMode(text, index, sortKey, items);
        }
        internal void ActivateRestore()
        {
            restoreMode ??= new RestoreMode();
            restoreMode.Status = ModeStatus.Active;
        }
        internal SortKeyOption RestoreSortKey()
        {
            return restoreMode.SavedSortKey;
        }
        internal List<FileStats> RestoreItems()
        {
            return restoreMode.SavedItems;
        }
        internal string RestoreCmdBoxText()
        {
            return restoreMode.SavedText;
        }
        internal int RestoreIndex()
        {
            return restoreMode.SavedIndex;
        }
        internal void ExitRestore()
        {
            // TODO: consider to dispose instance
            restoreMode = null;
        }

        //
        // Methods for Plain mode
        //
        internal bool IsPlain()
        {
            return !IsCrawlMode() && !IsRestoreMode();
        }

        // Change both searchedFileSet and CrawlFileSet
        internal void ApplyCrawlFileSet(HashSet<FileStats> sourceFileSet)
        {
            crawlMode.CrawlFileSet = crawlMode.CrawlFileSet.ImportPriorityAndExecTime(sourceFileSet);
            if (Program.appSettings.TargetFolders.Any(x => crawlMode.CrawlPath.StartsWith(x))) {
                sourceFileSet.RemoveWhere(x => Path.GetDirectoryName(x.FullPathName) == crawlMode.CrawlPath);
                sourceFileSet.UnionWith(crawlMode.CrawlFileSet);
            }
        }
    }
    internal enum ModeStatus
    {
        Defective,
        Prepared,
        Active,
    }
}
