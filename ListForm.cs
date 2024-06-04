﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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
        // Constants
        //
        //const int maxLineListView = 30;

        //
        // Constructor
        //
        public ListForm()
        {
            InitializeComponent();
        }

        //
        // Event handler
        //
        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (VirtualListIndex == e.Item.Index) {
                // TODO: CMIC
                //e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                e.Graphics.FillRectangle(new SolidBrush(MainForm.colorPattern4), e.Bounds);
            }
            //e.DrawText();


            // by GDI, insert blank between directory name
            string directoryName = Path.GetDirectoryName(e.Item.Text) + "\\";
            string fileName = Path.GetFileName(e.Item.Text);

            Rectangle bounds = e.Bounds;
            TextRenderer.DrawText(e.Graphics, directoryName, e.Item.Font, bounds, e.Item.ForeColor, TextFormatFlags.NoPadding);
            bounds.X += TextRenderer.MeasureText(e.Graphics, directoryName, e.Item.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            TextRenderer.DrawText(e.Graphics, fileName, e.Item.Font, bounds, MainForm.colorPattern5, TextFormatFlags.NoPadding);

            // by GDI, change color of yen sign
            //var directoryList = Path.GetDirectoryName(e.Item.Text).Split(Path.DirectorySeparatorChar);
            //Rectangle bounds = e.Bounds;
            //foreach (var directoryName in directoryList) {
            //    TextRenderer.DrawText(e.Graphics, directoryName, e.Item.Font, bounds, e.Item.ForeColor, TextFormatFlags.NoPadding);
            //    bounds.X += TextRenderer.MeasureText(e.Graphics, directoryName, e.Item.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            //    TextRenderer.DrawText(e.Graphics, "\\", e.Item.Font, bounds, Color.Yellow, TextFormatFlags.NoPadding);
            //    bounds.X += TextRenderer.MeasureText(e.Graphics, "\\", e.Item.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            //}
            //string fileName = Path.GetFileName(e.Item.Text);
            //TextRenderer.DrawText(e.Graphics, fileName, e.Item.Font, bounds, MainForm.colorPattern5, TextFormatFlags.NoPadding);

            // GDI+
            //var defaultBrush = new SolidBrush(e.Item.ForeColor);
            //var coloredBrush = new SolidBrush(MainForm.colorPattern5);
            // 描画の開始位置
            //float x = e.Bounds.X;
            //var charSize = e.Graphics.MeasureString(c.ToString(), font);
            //e.Graphics.DrawString(directoryName, e.Item.Font, defaultBrush, x, e.Bounds.Y);
            //SizeF proposedSize = new Size(int.MaxValue, int.MaxValue);
            //StringFormat format = new StringFormat();
            //format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
            //var stringWidth = e.Graphics.MeasureString(directoryName, e.Item.Font, PointF.Empty, format).Width;
            //e.Graphics.DrawString(fileName, e.Item.Font, coloredBrush, x + stringWidth, e.Bounds.Y);
        }

        private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // Insert blank between directory name
            var displayString = (ListViewItems[e.ItemIndex].ShortPathName ?? ListViewItems[e.ItemIndex].FullPathName).Replace("\\", "\\ ");
            e.Item = new ListViewItem(displayString);

            //// Nothing to do to change color of yen sign
            //e.Item = new ListViewItem(ListViewItems[e.ItemIndex].ShortPathName ?? ListViewItems[e.ItemIndex].FullPathName);
        }

        private void listView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            // TODO: CMIC
            e.Graphics.FillRectangle(new SolidBrush(MainForm.colorPattern2), e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.White, TextFormatFlags.Left);
            //TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.LightGray, TextFormatFlags.Left);
            //using (Pen pen = new Pen(Color.Gray)) {
            //    e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            //}
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

        internal FileStats GetItem()
        {
            if (Visible & listView.VirtualListSize > 0) {
                FileStats selectedFileStats = ListViewItems[VirtualListIndex];
                return selectedFileStats;
            }
            return null;
        }
        
        // TODO: Consider to use GetItem() instead of CurrentItem()
        internal FileStats CurrentItem()
        {
            return listView.VirtualListSize == 0 ? null : ListViewItems[VirtualListIndex];
        }

        internal void SelectNextItem()
        {
            if (listView.VirtualListSize == 0) return;

            VirtualListIndex++;
            DisplayColumnHeader(VirtualListIndex);

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
            DisplayColumnHeader(VirtualListIndex);

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
                DisplayColumnHeader(VirtualListIndex);
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

            listView.Columns[0].Width = maxWidth;
            // TODO: CMICst
            Width = maxWidth + 40;
        }

        private void DisplayColumnHeader(int index)
        {
            if (SortKey == SortKeyOption.FullPathName) {
                Header.Text = "Path";
            }
            else {
                Header.Text = string.Format("{0}: {1}", SortKey.ToString(), ListViewItems[index].SortValue(SortKey));
            }

            // If any, display additional information in column header
            if (ModeCaptions != (null, null)) {
                Header.Text += "  " + ModeCaptions.Item1;
                var baseWidth = TextRenderer.MeasureText(Header.Text, listView.Font).Width;
                Header.Text += FileStats.GetShortenedString(ModeCaptions.Item2, baseWidth) ?? ModeCaptions.Item2;
            }
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

        private static int PositiveModulo(int x, int y)
        {
            int z = x % y;
            return (z >= 0) ? z : z + y;
        }
    }
}
