using System.Collections.Generic;

namespace MiLauncher
{
    /// <summary>
    /// Stores information and status to control Restore mode, 
    /// which is a short-life mode to restore the snapshot of MainForm and ListForm
    /// </summary>
    internal class RestoreMode
    {
        public RestoreMode()
        {
            Status = ModeStatus.Defective;
        }
        public RestoreMode(string text, int index, SortKeyOption sortKey, List<FileStats> items)
        {
            SavedText = text;
            SavedIndex = index;
            SavedSortKey = sortKey;
            SavedItems = items;
            Status = ModeStatus.Prepared;
        }

        public ModeStatus Status { get; internal set; }

        internal string SavedText { get; }
        internal int SavedIndex { get; }
        internal SortKeyOption SavedSortKey { get; }
        internal List<FileStats> SavedItems { get; }

    }
}
