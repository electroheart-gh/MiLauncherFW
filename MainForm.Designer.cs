namespace MiLauncherFW
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.basePictureBox = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdBox = new System.Windows.Forms.TextBox();
            this.statusPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.basePictureBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statusPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // basePictureBox
            // 
            this.basePictureBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.basePictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.basePictureBox.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.basePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.basePictureBox.Location = new System.Drawing.Point(0, 0);
            this.basePictureBox.Margin = new System.Windows.Forms.Padding(4);
            this.basePictureBox.Name = "basePictureBox";
            this.basePictureBox.Size = new System.Drawing.Size(302, 34);
            this.basePictureBox.TabIndex = 0;
            this.basePictureBox.TabStop = false;
            this.basePictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.basePictureBox_MouseDown);
            this.basePictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.basePictureBox_MouseMove);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem,
            this.searchFilesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(136, 48);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // searchFilesToolStripMenuItem
            // 
            this.searchFilesToolStripMenuItem.Name = "searchFilesToolStripMenuItem";
            this.searchFilesToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.searchFilesToolStripMenuItem.Text = "Search Files";
            this.searchFilesToolStripMenuItem.Click += new System.EventHandler(this.searchToolStripMenuItem_Click);
            // 
            // cmdBox
            // 
            this.cmdBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cmdBox.ContextMenuStrip = this.contextMenuStrip1;
            this.cmdBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdBox.Font = new System.Drawing.Font("Meiryo UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cmdBox.Location = new System.Drawing.Point(17, 3);
            this.cmdBox.Margin = new System.Windows.Forms.Padding(4);
            this.cmdBox.Name = "cmdBox";
            this.cmdBox.Size = new System.Drawing.Size(282, 28);
            this.cmdBox.TabIndex = 1;
            this.cmdBox.TextChanged += new System.EventHandler(this.cmdBox_TextChanged);
            this.cmdBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cmdBox_KeyDown);
            this.cmdBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmdBox_KeyPress);
            this.cmdBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.cmdBox_PreviewKeyDown);
            // 
            // statusPictureBox
            // 
            this.statusPictureBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.statusPictureBox.Location = new System.Drawing.Point(16, 2);
            this.statusPictureBox.Name = "statusPictureBox";
            this.statusPictureBox.Size = new System.Drawing.Size(284, 30);
            this.statusPictureBox.TabIndex = 2;
            this.statusPictureBox.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(302, 34);
            this.ControlBox = false;
            this.Controls.Add(this.cmdBox);
            this.Controls.Add(this.statusPictureBox);
            this.Controls.Add(this.basePictureBox);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.basePictureBox)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.statusPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox basePictureBox;
        private System.Windows.Forms.TextBox cmdBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.PictureBox statusPictureBox;
        private System.Windows.Forms.ToolStripMenuItem searchFilesToolStripMenuItem;
    }
}