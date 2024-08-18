using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MiLauncherFW
{
    /// <summary>
    /// Represents a window of the file list that has responsibilities
    /// to control selected list item and display information for user.
    /// </summary>
    internal partial class ListForm : Form
    {
        //
        // Properties
        //
        internal List<FileStats> ListViewItems { get; private set; } = new List<FileStats>();
        internal SortKeyOption SortKey { get; set; } = SortKeyOption.Priority;
        internal (string, string) ModeCaptions { get; set; }

        private int _virtualListIndex;
        internal int VirtualListIndex
        {
            get {
                return _virtualListIndex;
            }
            set {
                _virtualListIndex = PositiveModulo(value, listView.VirtualListSize);

                // With MultiSelect false, adding a new index automatically removes old one
                listView.SelectedIndices.Add(_virtualListIndex);
            }
        }

        //
        // Delegate
        //
        public Action<KeyEventArgs> ListViewKeyDown;

        //
        // Constructor
        //
        public ListForm()
        {
            InitializeComponent();
            var fontName = Program.appSettings.ListViewFontName;
            var fontSize = Program.appSettings.ListViewFontSize;
            listView.Font = new Font(fontName, fontSize);
        }

        //
        // Event handler
        //
        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (VirtualListIndex == e.Item.Index) {
                // TODO: CMIC
                e.Graphics.FillRectangle(new SolidBrush(MainForm.colorPattern4), e.Bounds);
            }

            // e.Item.Text may contain DirectorySeparation space, but it does not affect for now
            var match = Regex.Match(e.Item.Text, @"^(?<Directory>.*\\)(?<File>[^\\]+\\?)$");

            var directoryName = match.Groups["Directory"].Value;
            var fileName = match.Groups["File"].Value;
            var fileNameColor = fileName.EndsWith("\\") ? MainForm.colorPattern3 : MainForm.colorPattern5;

            Rectangle bounds = e.Bounds;
            TextRenderer.DrawText(e.Graphics, directoryName, e.Item.Font, bounds, e.Item.ForeColor,
                                  TextFormatFlags.NoPadding);
            bounds.X += TextRenderer.MeasureText(e.Graphics, directoryName, e.Item.Font, Size.Empty,
                                                 TextFormatFlags.NoPadding).Width;
            TextRenderer.DrawText(e.Graphics, fileName, e.Item.Font, bounds, fileNameColor, TextFormatFlags.NoPadding);
        }

        private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var pathName = ListViewItems[e.ItemIndex].ShortPathName
                           ?? ListViewItems[e.ItemIndex].FullPathName;
            var itemText = Program.appSettings.DirectorySeparation
                           ? Regex.Replace(pathName, @"\\(?=.)", "\\ ")
                           : pathName;
            e.Item = new ListViewItem(itemText);
        }

        private void listView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            // TODO: CMIC
            e.Graphics.FillRectangle(new SolidBrush(MainForm.colorPattern2), e.Bounds);

            Rectangle bounds = e.Bounds;

            // Capture SortKey name and value
            var sortKeyMatch = Regex.Match(e.Header.Text, @"^(<[^>]*>)([^<]*)");
            // Display SortKey name
            TextRenderer.DrawText(e.Graphics, sortKeyMatch.Groups[1].Value, e.Font, bounds, Color.White,
                                  TextFormatFlags.NoPadding);
            bounds.X += TextRenderer.MeasureText(e.Graphics, sortKeyMatch.Groups[1].Value, e.Font,
                                                 Size.Empty, TextFormatFlags.NoPadding).Width;
            // Display SortKey value with specified color
            TextRenderer.DrawText(e.Graphics, sortKeyMatch.Groups[2].Value, e.Font, bounds,
                                  MainForm.colorPattern5, TextFormatFlags.NoPadding);
            bounds.X += TextRenderer.MeasureText(e.Graphics, sortKeyMatch.Groups[2].Value, e.Font,
                                                 Size.Empty, TextFormatFlags.NoPadding).Width;

            // Capture CrawlMode name and CrawlPath
            var crawlModeMatch = Regex.Match(e.Header.Text, @"(<CrawlMode> .*\\)(.*)");
            if (crawlModeMatch.Success) {
                // Display SortKey name
                TextRenderer.DrawText(e.Graphics, crawlModeMatch.Groups[1].Value, e.Font, bounds, Color.White,
                                      TextFormatFlags.NoPadding);
                bounds.X += TextRenderer.MeasureText(e.Graphics, crawlModeMatch.Groups[1].Value, e.Font,
                                                     Size.Empty, TextFormatFlags.NoPadding).Width;
                // Display SortKey value with specified color
                TextRenderer.DrawText(e.Graphics, crawlModeMatch.Groups[2].Value, e.Font, bounds,
                                      MainForm.colorPattern5, TextFormatFlags.NoPadding);
                bounds.X += TextRenderer.MeasureText(e.Graphics, crawlModeMatch.Groups[2].Value, e.Font,
                                                     Size.Empty, TextFormatFlags.NoPadding).Width;
            }
        }

        // Key down event in listView makes focus on MainForm
        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            ListViewKeyDown?.Invoke(e);
        }

        //
        // Methods
        //
        internal void SetVirtualList(List<FileStats> sourceItems)
        {
            ListViewItems = sourceItems.OrderByDescending(x => x.SortValue(SortKey)).ToList();
            listView.VirtualListSize = ListViewItems.Count;
        }

        internal FileStats CurrentItem()
        {
            return (Visible & listView.VirtualListSize > 0) ? ListViewItems[VirtualListIndex] : null;
        }

        internal void ShowAt(int? x = null, int? y = null, int index = 0)
        {
            Location = new Point(x ?? Location.X, y ?? Location.Y);
            Visible = true;

            if (ListViewItems.Any()) {
                // To add() SelectedIndices, listView requires focus on, which is mentioned by MSDN
                // Changing height in AdjustHeight() seems to focus on list view
                AdjustHeight();
                VirtualListIndex = index;
                SetColumnHeader(VirtualListIndex);
                listView.EnsureVisible(VirtualListIndex);
                AdjustWidth();
            }
            else {
                // TODO: CMICst for Column header height; 30
                Height = 30;
                SetColumnHeader();
                int headerWidth = TextRenderer.MeasureText(Header.Text, listView.Font).Width;

                // TODO: CMICst for allowance between ListView and ListForm; 40
                Width = headerWidth + 40;
                // Width might be greater than headerWidth + 40 due to OS and FormBorderStyle restriction
                listView.Columns[0].Width = Width - 40;
            }
            listView.Refresh();
        }

        internal void AdjustHeight()
        {
            var heightPerLine = listView.GetItemRect(0).Height;
            var lineCount = Math.Min(Program.appSettings.MaxListLine, listView.VirtualListSize + 1);

            // TODO: CMICst, 30 is Column header height
            Height = heightPerLine * lineCount + 30;
        }

        internal void AdjustWidth()
        {
            int headerWidth = TextRenderer.MeasureText(Header.Text, listView.Font).Width;
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            int columnContentWidth = listView.GetItemRect(0).Width;
            var maxWidth = Math.Max(columnContentWidth, headerWidth);

            // TODO: CMICst, adding 10 in order to have enough space for the actual width,
            // which might be caused by MeasureText is not taking e.Graphics as argument
            listView.Columns[0].Width = maxWidth + 10;
            // TODO: CMICst
            Width = maxWidth + 40;
        }

        private void SetColumnHeader(int? index = null)
        {
            var sortKeyString = (SortKey == SortKeyOption.FullPathName) ? "Path" : SortKey.ToString();
            Header.Text = "<" + sortKeyString + "> ";

            Header.Text += (SortKey != SortKeyOption.FullPathName && index != null)
                ? ListViewItems[(int)index].SortValue(SortKey)
                : "--";

            // If any, displays additional information defined by ModeCaptions in column header
            if (ModeCaptions == (null, null)) return;

            Header.Text += "  <" + ModeCaptions.Item1 + "> ";
            var baseWidth = TextRenderer.MeasureText(Header.Text, listView.Font).Width;
            Header.Text += FileStats.GetShortenedString(ModeCaptions.Item2, baseWidth) ?? ModeCaptions.Item2;
        }

        private static int PositiveModulo(int x, int y)
        {
            int z = x % y;
            return (z >= 0) ? z : z + y;
        }

        //
        // Methods for commands
        //
        internal void SelectNextItem()
        {
            if (listView.VirtualListSize == 0) return;

            VirtualListIndex++;
            SetColumnHeader(VirtualListIndex);

            var originalScrollPosition = listView.GetItemRect(0).Y;
            listView.EnsureVisible(VirtualListIndex);

            // Resize column width only if displaying initially or scrolling list in order to reduce flickers
            // If GetItemRect(0).Y changes after EnsureVisible(), list view scrolls. Then, resize column
            if (VirtualListIndex == 0 || originalScrollPosition != listView.GetItemRect(0).Y)
                AdjustWidth();
        }

        internal void SelectPreviousItem()
        {
            if (listView.VirtualListSize == 0) return;

            VirtualListIndex--;
            SetColumnHeader(VirtualListIndex);

            var originalScrollPosition = listView.GetItemRect(0).Y;
            listView.EnsureVisible(VirtualListIndex);

            // Resize column width only if displaying initially or scrolling list in order to reduce flickers
            // If GetItemRect(0).Y changes after EnsureVisible(), list view scrolls. Then, resize column
            if (VirtualListIndex == 0 || originalScrollPosition != listView.GetItemRect(0).Y)
                AdjustWidth();
        }

        internal void SortBy(SortKeyOption? option = null)
        {
            if (!Visible) return;

            SortKey = option ?? SortKey;
            ListViewItems = ListViewItems.OrderByDescending(x => x.SortValue(SortKey)).ToList();
            ShowAt();
        }

        internal void CopyPath()
        {
            Clipboard.SetText(CurrentItem().FullPathName);
        }

        internal void IncrementPriority(int delta)
        {
            if (CurrentItem() == null) return;

            var orgPathName = CurrentItem().FullPathName;
            CurrentItem().Priority += delta;
            ListViewItems = ListViewItems.OrderByDescending(x => x.SortValue(SortKey)).ToList();
            var orgPathIndex = ListViewItems.FindIndex(x => x.FullPathName == orgPathName);
            ShowAt(null, null, orgPathIndex);
        }
    }
}
