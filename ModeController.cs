using System.Collections.Generic;
using System.IO;

namespace MiLauncherFW
{
    /// <summary>
    /// Provides functionality to integrates control of <see cref="MiLauncherFW.CrawlMode"/>,
    /// <see cref="MiLauncherFW.RestoreMode"/> and normal mode 
    /// </summary>
    internal class ModeController
    {
        // Mode and Status
        // CrawlMode: Null, Defective, Immature, Active
        // RestoreMode: Null, Defective, Prepared, Active
        internal RestoreMode RestoreMode { get; set; }

        internal CrawlMode CrawlMode { get; set; }

        internal ModeController()
        {
        }

        //
        // Methods for Crawl mode
        //
        internal bool CrawlUp(string itemPath)
        {
            // Returns false when no change needed
            // If Crawl mode active, use CrawlPath instead of itemPath
            if (IsCrawlMode()) {
                var upperPath = Path.GetDirectoryName(CrawlMode.CrawlPath);
                CrawlMode crawlResult = CrawlMode.Crawl(upperPath);
                if ((crawlResult is null) || (crawlResult.Status == ModeStatus.Defective)) return false;
                CrawlMode = crawlResult;
                return true;
            }
            else {
                CrawlMode = CrawlMode.Crawl(Path.GetDirectoryName(itemPath));
                return CrawlMode != null;
            }
        }
        internal bool CrawlDown(string itemPath)
        {
            CrawlMode crawlResult = CrawlMode.Crawl(itemPath);
            if ((crawlResult is null) || (crawlResult.Status == ModeStatus.Defective)) return false;
            CrawlMode = crawlResult;
            return true;
        }
        internal bool IsCrawlMode()
        {
            return CrawlMode?.Status == ModeStatus.Active;
        }
        internal (string, string) GetCrawlCaptions()
        {
            return CrawlMode.Captions;
        }

        internal HashSet<FileStats> GetCrawlFileSet()
        {
            return CrawlMode?.CrawlFileSet;
        }

        internal void ExitCrawl()
        {
            // TODO: consider to dispose instance
            CrawlMode = null;
        }

        //
        // Methods for Restore mode
        //
        internal bool IsRestoreMode()
        {
            return RestoreMode?.Status == ModeStatus.Active;
        }
        internal bool IsRestorePrepared()
        {
            return RestoreMode?.Status == ModeStatus.Prepared || RestoreMode?.Status == ModeStatus.Active;
        }
        internal void PrepareRestore(string text, int index, SortKeyOption sortKey, List<FileStats> items)
        {
            RestoreMode = new RestoreMode(text, index, sortKey, items);
        }
        internal void ActivateRestore()
        {
            RestoreMode = RestoreMode ?? new RestoreMode();
            RestoreMode.Status = ModeStatus.Active;
        }
        internal SortKeyOption RestoreSortKey()
        {
            return RestoreMode.SavedSortKey;
        }
        internal List<FileStats> RestoreItems()
        {
            return RestoreMode.SavedItems;
        }
        internal string RestoreCmdBoxText()
        {
            return RestoreMode.SavedText;
        }
        internal int RestoreIndex()
        {
            return RestoreMode.SavedIndex;
        }
        internal void ExitRestore()
        {
            // TODO: consider to dispose instance
            RestoreMode = null;
        }

        //
        // Methods for Plain mode
        //
        internal bool IsPlain()
        {
            return !IsCrawlMode() && !IsRestoreMode();
        }

        //// Change both searchedFileSet and CrawlFileSet
        //internal void SyncCrawlFileSetMutually(HashSet<FileStats> sourceFileSet)
        //{
        //    CrawlMode.CrawlFileSet = CrawlMode.CrawlFileSet.ImportPriorityAndExecTime(sourceFileSet);
        //    if (Program.appSettings.TargetFolders.Any(x => CrawlMode.CrawlPath.StartsWith(x))) {
        //        sourceFileSet.RemoveWhere(x => Path.GetDirectoryName(x.FullPathName) == CrawlMode.CrawlPath);
        //        sourceFileSet.UnionWith(CrawlMode.CrawlFileSet);
        //    }
        //}
    }
    internal enum ModeStatus
    {
        Defective,
        Immature,
        Prepared,
        Active,
    }
}
