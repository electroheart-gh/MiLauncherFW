using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiLauncherFW
{
    partial class MainForm
    {
        // TODO: Consider to merge to MainForm.cs later

        private void ExecItem()
        {
            var execFileStats = listForm.CurrentItem();
            if (execFileStats is null) return;

            // Fire and forget pattern
            // No error handling so as to increment priority even if failed to open the file
            Task.Run(() =>
            {
                Process.Start("explorer.exe", execFileStats.FullPathName);

                // TODO: CMIC priority +1
                var fileStats = searchedFileSet.FirstOrDefault(x => x.FullPathName == execFileStats.FullPathName);
                if (fileStats is null) {
                    // Add to searchedFileSet temporarily, even though it might be removed after searchAllFiles()
                    execFileStats.Priority += 1;
                    execFileStats.ExecTime = DateTime.Now;
                    searchedFileSet.Add(execFileStats);
                }
                else {
                    fileStats.Priority += 1;
                    fileStats.ExecTime = DateTime.Now;
                }
            });
            HideMainForm();
        }

        private void OpenDirectory()
        {
            var execFileStats = listForm.CurrentItem();
            if (execFileStats is null) return;

            // Fire and forget pattern
            // No error handling so as to increment priority even if failed to open the directory
            Task.Run(() =>
            {
                var targetDirectoryName = (Program.appSettings.OpenDirectoryItself &&
                                           Directory.Exists(execFileStats.FullPathName))
                                        ? execFileStats.FullPathName
                                        : FileStats.ParentPathName(execFileStats.FullPathName);
                Process.Start("explorer.exe", targetDirectoryName);
                // TODO: CMIC priority +1
                var fileStats = searchedFileSet.FirstOrDefault(x => x.FullPathName == targetDirectoryName);
                if (fileStats != null) {
                    fileStats.Priority += 1;
                    fileStats.ExecTime = DateTime.Now;
                }
            });
            HideMainForm();
        }

        private void BeginningLine()
        {
            cmdBox.SelectionStart = 0;
        }

        private void EndLine()
        {
            cmdBox.SelectionStart = cmdBox.Text.Length;

        }

        private void ForwardChar()
        {
            cmdBox.SelectionStart++;
        }

        private void BackwardChar()
        {
            cmdBox.SelectionStart = Math.Max(0, cmdBox.SelectionStart - 1);
        }

        private void BackSpace()
        {
            var pos = cmdBox.SelectionStart;
            if (pos > 0) {
                cmdBox.Text = cmdBox.Text.Remove(pos - 1, 1);
                cmdBox.SelectionStart = pos - 1;
            }
        }

        private void DeleteChar()
        {
            var pos = cmdBox.SelectionStart;
            if (pos < cmdBox.Text.Length) {
                cmdBox.Text = cmdBox.Text.Remove(pos, 1);
                cmdBox.SelectionStart = pos;
            }
        }

        private void ForwardWord()
        {
            Regex pattern = NextWordRegex();
            Match m = pattern.Match(cmdBox.Text, cmdBox.SelectionStart);
            cmdBox.SelectionStart = Math.Max(m.Index + m.Length, cmdBox.SelectionStart);
        }

        private void BackwardWord()
        {
            Regex pattern = PreviousWordRegex();
            Match m = pattern.Match(cmdBox.Text.Substring(0, cmdBox.SelectionStart));
            cmdBox.SelectionStart = m.Index;
        }

        private void DeleteWord()
        {
            var cursorPosition = cmdBox.SelectionStart;
            Regex pattern = NextWordRegex();
            cmdBox.Text = pattern.Replace(cmdBox.Text, "", 1, cursorPosition);
            cmdBox.SelectionStart = cursorPosition;
        }

        private void BackwardDeleteWord()
        {
            // Using Non-backtracking and negative lookahead assertion of Regex
            Regex pattern = PreviousWordRegex();
            var firstHalf = pattern.Replace(cmdBox.Text.Substring(0, cmdBox.SelectionStart), "");
            cmdBox.Text = firstHalf + cmdBox.Text.Substring(cmdBox.SelectionStart);
            cmdBox.SelectionStart = firstHalf.Length;
        }

        //[GeneratedRegex(@"\w*\W*")]
        private static Regex NextWordRegex()
        {
            return new Regex(@"\w*\W*");
        }

        // Using Non-backtracking and negative lookahead assertion of Regex
        private static Regex PreviousWordRegex()
        {
            return new Regex(@"(?>\w*\W*)(?!\w)");
        }

        private void CrawlUpward()
        {
            if (!listForm.Visible) return;

            // Keep the original path to be selected in the new list
            var orgCrawlPath = currentMode.IsCrawlMode() ? currentMode.CrawlMode.CrawlPath : null;

            // Try Crawl and check its return
            if (!currentMode.CrawlUp(listForm.CurrentItem()?.FullPathName)) return;
            currentMode.CrawlMode.SyncFileSetMutually(searchedFileSet);

            if (!currentMode.IsRestorePrepared()) {
                currentMode.PrepareRestore(cmdBox.Text, listForm.VirtualListIndex,
                    listForm.SortKey, listForm.ListViewItems);
            }

            cmdBox.Text = string.Empty;
            listForm.ModeCaptions = currentMode.GetCrawlCaptions();
            listForm.SetVirtualList(currentMode.GetCrawlFileSet().ToList());

            // Find index of the original path after the sort in SetVirtualList()
            var orgPathIndex = listForm.ListViewItems.FindIndex(x => x.FullPathName == orgCrawlPath);
            listForm.ShowAt(null, null, orgPathIndex);

            Activate();
        }

        private void CrawlDownward()
        {
            if (!listForm.Visible) return;

            // Try Crawl and check its return
            if (!currentMode.CrawlDown(listForm.CurrentItem()?.FullPathName)) return;
            currentMode.CrawlMode.SyncFileSetMutually(searchedFileSet);

            if (!currentMode.IsRestorePrepared()) {
                currentMode.PrepareRestore(cmdBox.Text, listForm.VirtualListIndex,
                    listForm.SortKey, listForm.ListViewItems);
            }

            cmdBox.Text = string.Empty;
            listForm.ModeCaptions = currentMode.GetCrawlCaptions();
            listForm.SetVirtualList(currentMode.GetCrawlFileSet().ToList());

            listForm.ShowAt();
            Activate();
        }

        private void ExitCrawl()
        {
            if (!listForm.Visible) return;
            if (!currentMode.IsCrawlMode()) return;

            currentMode.ExitCrawl();

            currentMode.ActivateRestore();
            listForm.ModeCaptions = (null, null);
            listForm.SortKey = currentMode.RestoreSortKey();
            listForm.SetVirtualList(currentMode.RestoreItems());
            cmdBox.Text = currentMode.RestoreCmdBoxText();
            listForm.ShowAt(null, null, currentMode.RestoreIndex());
            currentMode.ExitRestore();

            Activate();
        }
    }
}
