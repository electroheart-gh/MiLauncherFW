using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private KeyMapController keyMapController;

        // Property
        private int _runningSearchCount;
        public int RunningSearchCount
        {
            get { return _runningSearchCount; }
            set {
                _runningSearchCount = value;
                statusPictureBox.BackColor = (value == 0) ? colorPattern1 : colorPattern5;
            }
        }

        //
        // Constructor
        //
        public MainForm()
        {
            InitializeComponent();
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
            // TODO: CMIC by keymap.json
            hotKey = new HotKey(MOD_KEY.ALT | MOD_KEY.CONTROL, Keys.F);
            hotKey.HotKeyPush += new EventHandler(hotKey_HotKeyPush);

            // Set color, font and size
            basePictureBox.BackColor = colorPattern1;
            var fontName = Program.appSettings.CmdBoxFontName;
            var fontSize = Program.appSettings.CmdBoxFontSize;
            cmdBox.Font = new Font(fontName, fontSize);

            // List Form
            listForm = new ListForm();
            listForm.ListViewKeyDown = listView_KeyDown;

            // Load File Set (HashSet<FileStats>)
            searchedFileSet = SettingManager.LoadSettings<HashSet<FileStats>>(searchedFileListDataFile) ?? new HashSet<FileStats>();

            // Key Mapping
            keyMapController = new KeyMapController();
            keyMapController.AttachTo(cmdBox);

            keyMapController.RegisterAction("HideMainForm", HideMainForm);
            keyMapController.RegisterAction("ExecItem", ExecItem);
            keyMapController.RegisterAction("OpenDirectory", OpenDirectory);
            keyMapController.RegisterAction("BeginningLine", BeginningLine);
            keyMapController.RegisterAction("EndLine", EndLine);
            keyMapController.RegisterAction("ForwardChar", ForwardChar);
            keyMapController.RegisterAction("BackwardChar", BackwardChar);
            keyMapController.RegisterAction("DeleteChar", DeleteChar);
            keyMapController.RegisterAction("BackSpace", BackSpace);
            keyMapController.RegisterAction("ForwardWord", ForwardWord);
            keyMapController.RegisterAction("BackwardWord", BackwardWord);
            keyMapController.RegisterAction("DeleteWord", DeleteWord);
            keyMapController.RegisterAction("BackwardDeleteWord", BackwardDeleteWord);
            keyMapController.RegisterAction("CrawlUpward", CrawlUpward);
            keyMapController.RegisterAction("CrawlDownward", CrawlDownward);
            keyMapController.RegisterAction("ExitCrawl", ExitCrawl);

            keyMapController.RegisterAction("SelectNextItem", listForm.SelectNextItem);
            keyMapController.RegisterAction("SelectPrevItem", listForm.SelectPreviousItem);
            keyMapController.RegisterAction("SortByPriority", () => listForm.SortBy(SortKeyOption.Priority));
            keyMapController.RegisterAction("SortByExecTime", () => listForm.SortBy(SortKeyOption.ExecTime));
            keyMapController.RegisterAction("SortByPath", () => listForm.SortBy(SortKeyOption.FullPathName));
            keyMapController.RegisterAction("SortByUpdateTime", () => listForm.SortBy(SortKeyOption.UpdateTime));
            keyMapController.RegisterAction("CopyPath", listForm.CopyPath);
            keyMapController.RegisterAction("IncrementPriority", () => listForm.IncrementPriority(1));
            keyMapController.RegisterAction("IncrementMorePriority", () => listForm.IncrementPriority(5));
            keyMapController.RegisterAction("DecrementPriority", () => listForm.IncrementPriority(-1));
            keyMapController.RegisterAction("DecrementMorePriority", () => listForm.IncrementPriority(-5));

            keyMapController.LoadKeyMapFromFile("keymap.json");

            // Search Files Async
            await SearchAllFilesAsync();
        }

        private void listView_KeyDown(KeyEventArgs args)
        {
            ActivateMainForm();
        }

        void hotKey_HotKeyPush(object sender, EventArgs e)
        {
            ActivateMainForm();
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

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            // As Sometimes Deactivate is called even if MainForm is active,
            // So check ActiveForm is null or not
            if (ActiveForm is null) {
                listForm.Visible = false;
            }
        }

        private void cmdBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) {
                e.IsInputKey = true;
            }
        }

        private void basePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Move MainForm by left button dragging
            if (e.Button == MouseButtons.Left) {
                dragStart = e.Location;
            }
        }

        private void basePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Move MainForm by left button dragging
            if (e.Button == MouseButtons.Left) {
                Location = new Point(Location.X + e.Location.X - dragStart.X,
                                     Location.Y + e.Location.Y - dragStart.Y);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SearchAllFilesAsync();
        }

        //
        // Methods
        //
        private void ActivateMainForm()
        {
            // Show() does not working
            Visible = true;
            Activate();
            BringToFront();
        }

        private void HideMainForm()
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

            if (Program.appSettings.SaveFileListWhenHide) {
                SettingManager.SaveSettings(searchedFileSet, searchedFileListDataFile);
            }
        }

        private async Task SearchAllFilesAsync()
        {
            RunningSearchCount += 1;
            try {
                var newSearchedFileSet = await Task.Run(FileSet.SearchAllFiles);
                searchedFileSet = newSearchedFileSet.ImportPriorityAndExecTime(searchedFileSet).ToHashSet();
                SettingManager.SaveSettings(searchedFileSet, searchedFileListDataFile);
                RunningSearchCount -= 1;
            }
            catch (DirectoryNotFoundException) {
                RunningSearchCount -= 1;
                statusPictureBox.BackColor = Color.Red;
            }
            catch (IOException) {
                RunningSearchCount -= 1;
                statusPictureBox.BackColor = Color.Red;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingManager.SaveSettings(searchedFileSet, searchedFileListDataFile);
        }
    }
}

