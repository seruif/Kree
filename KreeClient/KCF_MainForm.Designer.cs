namespace KreeClient
{
    partial class KCF_MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KCF_MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelConnected = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelAction = new System.Windows.Forms.ToolStripStatusLabel();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.listView1 = new System.Windows.Forms.ListView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.uploadFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.downloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listView2 = new System.Windows.Forms.ListView();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.pathlabel1 = new System.Windows.Forms.Label();
            this.pathlabel2 = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.restartToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1072, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.Click += new System.EventHandler(this.restartToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelConnected,
            this.toolStripStatusLabelAction});
            this.statusStrip1.Location = new System.Drawing.Point(0, 580);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1072, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelConnected
            // 
            this.toolStripStatusLabelConnected.Name = "toolStripStatusLabelConnected";
            this.toolStripStatusLabelConnected.Size = new System.Drawing.Size(91, 17);
            this.toolStripStatusLabelConnected.Text = "Disconnected";
            // 
            // toolStripStatusLabelAction
            // 
            this.toolStripStatusLabelAction.Name = "toolStripStatusLabelAction";
            this.toolStripStatusLabelAction.Size = new System.Drawing.Size(119, 17);
            this.toolStripStatusLabelAction.Text = "TFK Not Recieved";
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listView1
            // 
            this.listView1.AllowDrop = true;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.ContextMenuStrip = this.contextMenuStrip;
            this.listView1.Enabled = false;
            this.listView1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listView1.LargeImageList = this.imageList;
            this.listView1.Location = new System.Drawing.Point(14, 24);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1052, 457);
            this.listView1.StateImageList = this.imageList;
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createDirectoryToolStripMenuItem,
            this.toolStripSeparator4,
            this.deleteDirectoryToolStripMenuItem,
            this.toolStripSeparator1,
            this.uploadFilesToolStripMenuItem,
            this.uploadDirectoryToolStripMenuItem,
            this.toolStripSeparator2,
            this.downloadToolStripMenuItem,
            this.toolStripSeparator3,
            this.copyToolStripMenuItem,
            this.cutToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator5,
            this.propertiesToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(164, 232);
            // 
            // createDirectoryToolStripMenuItem
            // 
            this.createDirectoryToolStripMenuItem.Name = "createDirectoryToolStripMenuItem";
            this.createDirectoryToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.createDirectoryToolStripMenuItem.Text = "Create Path";
            this.createDirectoryToolStripMenuItem.Click += new System.EventHandler(this.createDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(160, 6);
            // 
            // deleteDirectoryToolStripMenuItem
            // 
            this.deleteDirectoryToolStripMenuItem.Name = "deleteDirectoryToolStripMenuItem";
            this.deleteDirectoryToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.deleteDirectoryToolStripMenuItem.Text = "Delete";
            this.deleteDirectoryToolStripMenuItem.Click += new System.EventHandler(this.deleteItemsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(160, 6);
            // 
            // uploadFilesToolStripMenuItem
            // 
            this.uploadFilesToolStripMenuItem.Name = "uploadFilesToolStripMenuItem";
            this.uploadFilesToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.uploadFilesToolStripMenuItem.Text = "Upload Files";
            this.uploadFilesToolStripMenuItem.Click += new System.EventHandler(this.uploadFilesToolStripMenuItem_Click);
            // 
            // uploadDirectoryToolStripMenuItem
            // 
            this.uploadDirectoryToolStripMenuItem.Name = "uploadDirectoryToolStripMenuItem";
            this.uploadDirectoryToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.uploadDirectoryToolStripMenuItem.Text = "Upload Directory";
            this.uploadDirectoryToolStripMenuItem.Click += new System.EventHandler(this.uploadDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(160, 6);
            // 
            // downloadToolStripMenuItem
            // 
            this.downloadToolStripMenuItem.Name = "downloadToolStripMenuItem";
            this.downloadToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.downloadToolStripMenuItem.Text = "Download";
            this.downloadToolStripMenuItem.Click += new System.EventHandler(this.downloadToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(160, 6);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(160, 6);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // listView2
            // 
            this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView2.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listView2.LargeImageList = this.imageList;
            this.listView2.Location = new System.Drawing.Point(3, 24);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(600, 401);
            this.listView2.StateImageList = this.imageList;
            this.listView2.TabIndex = 3;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.List;
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer.Location = new System.Drawing.Point(0, 29);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.pathlabel1);
            this.splitContainer.Panel1.Controls.Add(this.listView1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.pathlabel2);
            this.splitContainer.Panel2.Controls.Add(this.listView2);
            this.splitContainer.Panel2Collapsed = true;
            this.splitContainer.Size = new System.Drawing.Size(1072, 518);
            this.splitContainer.SplitterDistance = 576;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 4;
            // 
            // pathlabel1
            // 
            this.pathlabel1.AutoSize = true;
            this.pathlabel1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pathlabel1.Location = new System.Drawing.Point(14, 6);
            this.pathlabel1.Name = "pathlabel1";
            this.pathlabel1.Size = new System.Drawing.Size(42, 14);
            this.pathlabel1.TabIndex = 4;
            this.pathlabel1.Text = "root\\";
            // 
            // pathlabel2
            // 
            this.pathlabel2.AutoSize = true;
            this.pathlabel2.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pathlabel2.Location = new System.Drawing.Point(3, 5);
            this.pathlabel2.Name = "pathlabel2";
            this.pathlabel2.Size = new System.Drawing.Size(49, 14);
            this.pathlabel2.TabIndex = 5;
            this.pathlabel2.Text = "label1";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Multiselect = true;
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // KCF_MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1072, 602);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitContainer);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "KCF_MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Kree Client";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KCF_MainForm_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelConnected;
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelAction;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label pathlabel1;
        private System.Windows.Forms.Label pathlabel2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem createDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem uploadFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadDirectoryToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem downloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
    }
}

