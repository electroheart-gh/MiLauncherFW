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
        internal List<FileStats> ListViewItems { get; private set; }
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

            // by GDI, insert blank between directory name
            string directoryName = Path.GetDirectoryName(e.Item.Text) + "\\";
            string fileName = Path.GetFileName(e.Item.Text);

            Rectangle bounds = e.Bounds;
            TextRenderer.DrawText(e.Graphics, directoryName, e.Item.Font, bounds, e.Item.ForeColor, TextFormatFlags.NoPadding);
            bounds.X += TextRenderer.MeasureText(e.Graphics, directoryName, e.Item.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            TextRenderer.DrawText(e.Graphics, fileName, e.Item.Font, bounds, MainForm.colorPattern5, TextFormatFlags.NoPadding);
        }

        private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // Insert blank between directory name
            var displayString = (ListViewItems[e.ItemIndex].ShortPathName ?? ListViewItems[e.ItemIndex].FullPathName).Replace("\\", "\\ ");
            e.Item = new ListViewItem(displayString);
        }

        private void listView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            // TODO: CMIC
            e.Graphics.FillRectangle(new SolidBrush(MainForm.colorPattern2), e.Bounds);

            Rectangle bounds = e.Bounds;

            // Captures SortKey name and value
            var sortKeyMatch = Regex.Match(e.Header.Text, @"^(<[^>]*>)([^<]*)");
            // Display SortKey name
            TextRenderer.DrawText(e.Graphics, sortKeyMatch.Groups[1].Value, e.Font, bounds, Color.White, TextFormatFlags.NoPadding);
            bounds.X += TextRenderer.MeasureText(e.Graphics, sortKeyMatch.Groups[1].Value, e.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            // Display SortKey value with specified color
            TextRenderer.DrawText(e.Graphics, sortKeyMatch.Groups[2].Value, e.Font, bounds, MainForm.colorPattern5, TextFormatFlags.NoPadding);
            bounds.X += TextRenderer.MeasureText(e.Graphics, sortKeyMatch.Groups[2].Value, e.Font, Size.Empty, TextFormatFlags.NoPadding).Width;

            // Captures CrawlMode name and CrawlPath
            var crawlModeMatch = Regex.Match(e.Header.Text, @"(<CrawlMode> .*\\)(.*)");
            if (crawlModeMatch.Success) {
                // Display SortKey name
                TextRenderer.DrawText(e.Graphics, crawlModeMatch.Groups[1].Value, e.Font, bounds, Color.White, TextFormatFlags.NoPadding);
                bounds.X += TextRenderer.MeasureText(e.Graphics, crawlModeMatch.Groups[1].Value, e.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
                // Display SortKey value with specified color
                TextRenderer.DrawText(e.Graphics, crawlModeMatch.Groups[2].Value, e.Font, bounds, MainForm.colorPattern5, TextFormatFlags.NoPadding);
                bounds.X += TextRenderer.MeasureText(e.Graphics, crawlModeMatch.Groups[2].Value, e.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
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
        internal void SetVirtualList(List<FileStats> sourceItems = null)
        {
            sourceItems = sourceItems ?? ListViewItems;
            ListViewItems = sourceItems.OrderByDescending(x => x.SortValue(SortKey)).ToList();
            listView.VirtualListSize = ListViewItems.Count;
        }

        internal FileStats CurrentItem()
        {
            return (Visible & listView.VirtualListSize > 0) ? ListViewItems[VirtualListIndex] : null;
        }

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
                Height = 0;
                listView.Columns[0].Width = 0;
                // TODO: CMICst
                Width = 100;
            }
            listView.Refresh();
        }

        internal void AdjustHeight()
        {
            // TODO: CMICst
            Height = listView.GetItemRect(0).Height * Math.Min(Program.appSettings.MaxListLine, listView.VirtualListSize + 1) + 30;
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

        private void SetColumnHeader(int index)
        {
            Header.Text = (SortKey == SortKeyOption.FullPathName)
                ? "<Path>"
                : string.Format("<{0}> {1}", SortKey.ToString(), ListViewItems[index].SortValue(SortKey));

            // If any, displays additional information defined by ModeCaptions in column header
            if (ModeCaptions == (null, null)) return;

            Header.Text += "  <" + ModeCaptions.Item1 + "> ";
            var baseWidth = TextRenderer.MeasureText(Header.Text, listView.Font).Width;
            Header.Text += FileStats.GetShortenedString(ModeCaptions.Item2, baseWidth) ?? ModeCaptions.Item2;
        }

        internal void CycleSortKey()
        {
            switch (SortKey) {
                case SortKeyOption.Priority:
                    SortKey = SortKeyOption.ExecTime;
                    break;
                case SortKeyOption.ExecTime:
                    SortKey = SortKeyOption.UpdateTime;
                    break;
                case SortKeyOption.UpdateTime:
                    SortKey = SortKeyOption.FullPathName;
                    break;
                default:
                    SortKey = SortKeyOption.Priority;
                    break;
            }
            SetVirtualList();
        }

        internal void ChangeSortKey(SortKeyOption sortKey)
        {
            SortKey = sortKey;
            SetVirtualList();
        }

        private static int PositiveModulo(int x, int y)
        {
            int z = x % y;
            return (z >= 0) ? z : z + y;
        }

    }
}
