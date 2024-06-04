using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiLauncherFW
{
    /// <summary>
    /// Represents a main window that accept user interface and delegates tasks to other windows/classes.
    /// </summary>
    public partial class MainForm : Form
    {
        // Constant
        // TODO: Consider to make SearchedFileListDataFile configurable
        private const string searchedFileListDataFile = "SearchedFileList.dat";
        private const int CS_DROPSHADOW = 0x00020000;

        // Static Variables
        internal static Color colorPattern1 = ColorTranslator.FromHtml("#28385E");
        internal static Color colorPattern2 = ColorTranslator.FromHtml("#516C8D");
        internal static Color colorPattern3 = ColorTranslator.FromHtml("#6A91C1");
        internal static Color colorPattern4 = ColorTranslator.FromHtml("#CCCCCC");
        internal static Color colorPattern5 = ColorTranslator.FromHtml("#FF9800");

        // Variables
        private Point dragStart;
        private HotKey hotKey;
        private ListForm listForm;
        private HashSet<FileStats> searchedFileSet;
        private CancellationTokenSource tokenSource;
        private ModeController currentMode = new ModeController();


        //
        // Constructor
        //
        public MainForm()
        {
            InitializeComponent();
            pictureBox1.BackColor = colorPattern1;
        }

        // Borderless winform with shadow
        protected override CreateParams CreateParams
        {
            get {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        //
        // Event handler
        //
        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Global Hot Key
            hotKey = new HotKey(MOD_KEY.ALT | MOD_KEY.CONTROL, Keys.F);
            hotKey.HotKeyPush += new EventHandler(hotKey_HotKeyPush);

            // List Form
            listForm = new ListForm();
            listForm.ListViewKeyDown = listView_KeyDown;

            // Load File Set (HashSet<FileStats>)
            searchedFileSet = SettingManager.LoadSettings<HashSet<FileStats>>(searchedFileListDataFile) ?? new HashSet<FileStats>();

            // TODO: Make it method
            // Search Files Async
            var newSearchedFileSet = await Task.Run(FileSet.SearchAllFiles);
            searchedFileSet = newSearchedFileSet.ImportPriorityAndExecTime(searchedFileSet).ToHashSet();
            SettingManager.SaveSettings(searchedFileSet, searchedFileListDataFile);
        }

        private void listView_KeyDown(KeyEventArgs args)
        {
            ActivateMainForm();
        }

        void hotKey_HotKeyPush(object sender, EventArgs e)
        {
            ActivateMainForm();
        }

        private void ActivateMainForm()
        {
            Visible = true;
            Activate();
            BringToFront();
        }

        private async void cmdBox_TextChanged(object sender, EventArgs e)
        {
            tokenSource?.Cancel();
            tokenSource = null;

            if (currentMode.IsRestoreMode()) return;

            if (cmdBox.Text.Length == 0 && currentMode.IsPlain()) {
                listForm.Visible = false;
                return;
            }

            var filters = new FileNameFilter(cmdBox.Text);

            // Set baseFileSet depending on crawlMode or not
            var baseFileSet = currentMode.GetCrawlFileSet() ?? searchedFileSet;

            tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var filteredList = await Task.Run
                (() => baseFileSet.FilterWithCancellation(filters, token), token);
            if (token.IsCancellationRequested) return;

            listForm.SetVirtualList(filteredList);

            // TODO: CMICst
            listForm.ShowAt(Location.X - 6, Location.Y + Height - 5);

            Activate();
            return;
        }

        // Implement Ctrl- and Alt- commands in KeyDown event
        // It is because e.KeyChar of KeyPress returns a value depending on modifiers input,
        // which requires to check KeyChar of Ctrl-(char) in advance of coding
        private void cmdBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Disable default behavior for modifier keys, including beep sound
            if ((ModifierKeys & (Keys.Control | Keys.Alt)) > 0) {
                e.Handled = true;
            }
            // Disable default behavior for Enter and ESC including beep sound
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape) {
                e.Handled = true;
            }
        }

        // Implement Ctrl- and Alt- commands in KeyDown event
        // It is because e.KeyChar of KeyPress returns a value depending on modifiers input,
        // which requires to check KeyChar of Ctrl-(char) in advance of coding
        private void cmdBox_KeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Implement keymap class to make keymap configurable
            // TODO: <CAUTION> No check for unnecessary Key modifiers !!!

            // Close MainForm
            if (e.KeyCode == Keys.Escape) {
                CloseMainForm();
            }
            // Exec file with associated app
            else if ((e.KeyCode == Keys.Enter && !e.Alt) || (e.KeyCode == Keys.M && e.Control)) {
                var execFileStats = listForm.GetItem();
                if (execFileStats is null) return;

                try {
                    Process.Start("explorer.exe", execFileStats.FullPathName);
                }
                catch (FileNotFoundException) {
                    Debug.WriteLine("File Not Found");
                    return;
                }

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
                CloseMainForm();
            }
            // Open directory of item (itself or parent)
            else if (e.KeyCode == Keys.Enter && e.Alt) {
                var execFileStats = listForm.GetItem();
                if (execFileStats is null) return;

                var targetDirectoryName = (Directory.Exists(execFileStats.FullPathName))
                                        ? execFileStats.FullPathName
                                        : Path.GetDirectoryName(execFileStats.FullPathName);

                try {
                    Process.Start("explorer.exe", targetDirectoryName);
                }
                catch (FileNotFoundException) {
                    Debug.WriteLine("File Not Found");
                    return;
                }

                // TODO: CMIC priority +1
                var fileStats = searchedFileSet.FirstOrDefault(x => x.FullPathName == targetDirectoryName);
                if (fileStats != null) {
                    fileStats.Priority += 1;
                    fileStats.ExecTime = DateTime.Now;
                }
                CloseMainForm();
            }
            // Copy file path to clipboard
            else if (e.KeyCode == Keys.C && e.Control && !e.Shift) {
                Clipboard.SetText(listForm.GetItem().FullPathName);
            }
            //// Copy file path in UNC to clipboard
            //if (e.KeyCode == Keys.C && e.Control && e.Shift) {
            //    Clipboard.SetText(ConvertToUNC(listForm.GetItem().FullPathName));
            //}
            // beginning of line
            else if (e.KeyCode == Keys.A && e.Control) {
                cmdBox.SelectionStart = 0;
            }
            // end of line
            else if (e.KeyCode == Keys.E && e.Control) {
                cmdBox.SelectionStart = cmdBox.Text.Length;
            }
            // forward char
            else if (e.KeyCode == Keys.F && e.Control) {
                cmdBox.SelectionStart++;
            }
            // backward char
            else if (e.KeyCode == Keys.B && e.Control) {
                cmdBox.SelectionStart = Math.Max(0, cmdBox.SelectionStart - 1);
            }
            // backspace
            else if (e.KeyCode == Keys.H && e.Control) {
                var pos = cmdBox.SelectionStart;
                if (pos > 0) {
                    cmdBox.Text = cmdBox.Text.Remove(pos - 1, 1);
                    cmdBox.SelectionStart = pos - 1;
                }
            }
            // delete char
            else if (e.KeyCode == Keys.D && e.Control) {
                var pos = cmdBox.SelectionStart;
                if (pos < cmdBox.Text.Length) {
                    cmdBox.Text = cmdBox.Text.Remove(pos, 1);
                    cmdBox.SelectionStart = pos;
                }
            }
            // select next item
            else if (e.KeyCode == Keys.N && e.Control) {
                listForm.SelectNextItem();
            }
            // select previous item
            else if (e.KeyCode == Keys.P && e.Control) {
                listForm.SelectPreviousItem();
            }
            // forward word
            else if (e.KeyCode == Keys.F && e.Alt) {
                Regex pattern = NextWordRegex();
                Match m = pattern.Match(cmdBox.Text, cmdBox.SelectionStart);
                cmdBox.SelectionStart = Math.Max(m.Index + m.Length, cmdBox.SelectionStart);
            }
            // backward word
            else if (e.KeyCode == Keys.B && e.Alt) {
                Regex pattern = PreviousWordRegex();
                Match m = pattern.Match(cmdBox.Text.Substring(0, cmdBox.SelectionStart));
                cmdBox.SelectionStart = m.Index;
            }
            // delete word
            else if (e.KeyCode == Keys.D && e.Alt) {
                var cursorPosition = cmdBox.SelectionStart;
                Regex pattern = NextWordRegex();
                cmdBox.Text = pattern.Replace(cmdBox.Text, "", 1, cursorPosition);
                cmdBox.SelectionStart = cursorPosition;
            }
            // backward delete word
            else if (e.KeyCode == Keys.H && e.Alt) {
                // Using Non-backtracking and negative lookahead assertion of Regex
                Regex pattern = PreviousWordRegex();
                var firstHalf = pattern.Replace(cmdBox.Text.Substring(0, cmdBox.SelectionStart), "");
                cmdBox.Text = firstHalf + cmdBox.Text.Substring(cmdBox.SelectionStart);
                cmdBox.SelectionStart = firstHalf.Length;
            }
            // Cycle ListView sort key
            // Keys.Oemtilde indicates @ (at mark)
            else if (e.KeyCode == Keys.Oemtilde && e.Control) {
                if (!listForm.Visible) return;

                listForm.CycleSortKey();
                listForm.ShowAt();
            }
            // Crawl folder upwards
            else if (e.KeyCode == Keys.Oemcomma && e.Control) {
                if (!listForm.Visible) return;

                // Try Crawl and check its return
                if (!currentMode.CrawlUp(listForm.CurrentItem().FullPathName, searchedFileSet)) return;

                if (!currentMode.IsRestorePrepared()) {
                    currentMode.PrepareRestore(cmdBox.Text, listForm.VirtualListIndex,
                        listForm.SortKey, listForm.ListViewItems);
                }
                currentMode.ApplyCrawlFileSet(searchedFileSet);
                cmdBox.Text = string.Empty;
                listForm.ModeCaptions = currentMode.GetCrawlCaptions();
                listForm.SetVirtualList(currentMode.GetCrawlFileSet().ToList());

                listForm.ShowAt();
                Activate();
            }
            // Crawl folder downwards
            else if (e.KeyCode == Keys.OemPeriod && e.Control) {
                if (!listForm.Visible) return;

                // Try Crawl and check its return
                if (!currentMode.CrawlDown(listForm.CurrentItem().FullPathName, searchedFileSet)) return;

                if (!currentMode.IsRestorePrepared()) {
                    currentMode.PrepareRestore(cmdBox.Text, listForm.VirtualListIndex,
                        listForm.SortKey, listForm.ListViewItems);
                }
                currentMode.ApplyCrawlFileSet(searchedFileSet);
                cmdBox.Text = string.Empty;
                listForm.ModeCaptions = currentMode.GetCrawlCaptions();
                listForm.SetVirtualList(currentMode.GetCrawlFileSet().ToList());

                listForm.ShowAt();
                Activate();
            }
            // Exit crawl mode
            else if (e.KeyCode == Keys.G && e.Control) {
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
            // TODO: Cycle backwards ListView sort key 
            // TODO: implement search history using M-p, M-n
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            // As Sometimes Deactivate is called even if MainForm is active,
            // So check ActiveForm is null or not
            if (ActiveForm is null) {
                // TODO: consider when to save fileList
                // SettingManager.SaveSettings<FileList>(fileList, fileListDataPath);
                CloseMainForm();
            }
        }

        private void cmdBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) {
                e.IsInputKey = true;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Move MainForm by left button dragging
            if (e.Button == MouseButtons.Left) {
                dragStart = e.Location;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // Move MainForm by left button dragging
            if (e.Button == MouseButtons.Left) {
                Location = new Point(Location.X + e.Location.X - dragStart.X,
                                     Location.Y + e.Location.Y - dragStart.Y);
            }
        }

        private void CloseMainForm()
        {
            // Force to exit Crawl mode regardless of any mode
            currentMode.ExitCrawl();

            // Activate Restore mode to reset some properties
            currentMode.ActivateRestore();
            listForm.ModeCaptions = (null, null);
            listForm.SortKey = currentMode.RestoreSortKey();
            cmdBox.Text = string.Empty;
            currentMode.ExitRestore();

            Visible = false;
            listForm.Visible = false;
            SettingManager.SaveSettings(searchedFileSet, searchedFileListDataFile);
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

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
