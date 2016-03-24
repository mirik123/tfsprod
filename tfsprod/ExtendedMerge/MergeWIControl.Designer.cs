namespace TFSExp.ExtendedMerge
{
    /// <summary>
    /// This class inherites from UserControl.
    /// </summary>
    partial class MergeWIControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MergeWIControl));
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuChangesets = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewChangesetDetailesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewWorkItemDetailesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showServerPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSourcePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolLinkWIs = new System.Windows.Forms.ToolStripButton();
            this.toolMNormal = new System.Windows.Forms.ToolStripButton();
            this.toolMConservative = new System.Windows.Forms.ToolStripButton();
            this.toolResolve = new System.Windows.Forms.ToolStripButton();
            this.toolRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolMerge = new System.Windows.Forms.ToolStripButton();
            this.toolDChangeset = new System.Windows.Forms.ToolStripButton();
            this.toolDWorkItem = new System.Windows.Forms.ToolStripButton();
            this.toolDServerPath = new System.Windows.Forms.ToolStripButton();
            this.toolEditServerPath = new System.Windows.Forms.ToolStripButton();
            this.toolDClipboard = new System.Windows.Forms.ToolStripButton();
            this.toolGetRelBranches = new System.Windows.Forms.ToolStripButton();
            this.toolAutoCheckin = new System.Windows.Forms.ToolStripButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.chChecked = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chWorkitemID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chChangesetID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chOwner = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chChanges = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMergeType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSourcePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.ctxMenuChangesets.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.copyToClipboardToolStripMenuItem.Text = "Copy to Clipboard";
            // 
            // ctxMenuChangesets
            // 
            this.ctxMenuChangesets.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewChangesetDetailesToolStripMenuItem,
            this.viewWorkItemDetailesToolStripMenuItem,
            this.copyToClipboardToolStripMenuItem,
            this.showServerPathToolStripMenuItem,
            this.editSourcePathToolStripMenuItem});
            this.ctxMenuChangesets.Name = "ctxMenuChangesets";
            this.ctxMenuChangesets.Size = new System.Drawing.Size(203, 114);
            this.ctxMenuChangesets.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ctxMenuChangesets_ItemClicked);
            // 
            // viewChangesetDetailesToolStripMenuItem
            // 
            this.viewChangesetDetailesToolStripMenuItem.Name = "viewChangesetDetailesToolStripMenuItem";
            this.viewChangesetDetailesToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.viewChangesetDetailesToolStripMenuItem.Text = "View Changeset Detailes";
            // 
            // viewWorkItemDetailesToolStripMenuItem
            // 
            this.viewWorkItemDetailesToolStripMenuItem.Name = "viewWorkItemDetailesToolStripMenuItem";
            this.viewWorkItemDetailesToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.viewWorkItemDetailesToolStripMenuItem.Text = "View Work Item Detailes";
            // 
            // showServerPathToolStripMenuItem
            // 
            this.showServerPathToolStripMenuItem.Name = "showServerPathToolStripMenuItem";
            this.showServerPathToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.showServerPathToolStripMenuItem.Text = "Navigate to Source Path";
            // 
            // editSourcePathToolStripMenuItem
            // 
            this.editSourcePathToolStripMenuItem.Name = "editSourcePathToolStripMenuItem";
            this.editSourcePathToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.editSourcePathToolStripMenuItem.Text = "Edit Source Path";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.toolStrip1);
            this.groupBox1.Controls.Add(this.listView1);
            this.groupBox1.Location = new System.Drawing.Point(5, 57);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(984, 575);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge Changesets";
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolLinkWIs,
            this.toolMNormal,
            this.toolMConservative,
            this.toolResolve,
            this.toolRefresh,
            this.toolMerge,
            this.toolDChangeset,
            this.toolDWorkItem,
            this.toolDServerPath,
            this.toolEditServerPath,
            this.toolDClipboard,
            this.toolGetRelBranches,
            this.toolAutoCheckin});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.toolStrip1.Location = new System.Drawing.Point(3, 15);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(974, 65);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolLinkWIs
            // 
            this.toolLinkWIs.CheckOnClick = true;
            this.toolLinkWIs.Enabled = false;
            this.toolLinkWIs.Image = ((System.Drawing.Image)(resources.GetObject("toolLinkWIs.Image")));
            this.toolLinkWIs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolLinkWIs.Name = "toolLinkWIs";
            this.toolLinkWIs.Size = new System.Drawing.Size(126, 20);
            this.toolLinkWIs.Text = "Link to Work Items";
            this.toolLinkWIs.Click += new System.EventHandler(this.toolLinkWIs_Click);
            // 
            // toolMNormal
            // 
            this.toolMNormal.Checked = true;
            this.toolMNormal.CheckOnClick = true;
            this.toolMNormal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolMNormal.Enabled = false;
            this.toolMNormal.Image = ((System.Drawing.Image)(resources.GetObject("toolMNormal.Image")));
            this.toolMNormal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolMNormal.Name = "toolMNormal";
            this.toolMNormal.Size = new System.Drawing.Size(104, 20);
            this.toolMNormal.Text = "Normal Merge";
            this.toolMNormal.Click += new System.EventHandler(this.toolMNormal_Click);
            // 
            // toolMConservative
            // 
            this.toolMConservative.CheckOnClick = true;
            this.toolMConservative.Enabled = false;
            this.toolMConservative.Image = ((System.Drawing.Image)(resources.GetObject("toolMConservative.Image")));
            this.toolMConservative.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolMConservative.Name = "toolMConservative";
            this.toolMConservative.Size = new System.Drawing.Size(132, 20);
            this.toolMConservative.Text = "Conservative Merge";
            this.toolMConservative.Click += new System.EventHandler(this.toolMConservative_Click);
            // 
            // toolResolve
            // 
            this.toolResolve.Enabled = false;
            this.toolResolve.Image = ((System.Drawing.Image)(resources.GetObject("toolResolve.Image")));
            this.toolResolve.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolResolve.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolResolve.Name = "toolResolve";
            this.toolResolve.Size = new System.Drawing.Size(101, 67);
            this.toolResolve.Text = "Resolve Conflicts";
            this.toolResolve.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolResolve.Click += new System.EventHandler(this.toolResolve_Click);
            // 
            // toolRefresh
            // 
            this.toolRefresh.Enabled = false;
            this.toolRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolRefresh.Image")));
            this.toolRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRefresh.Name = "toolRefresh";
            this.toolRefresh.Size = new System.Drawing.Size(137, 20);
            this.toolRefresh.Text = "Merge Types Refresh";
            this.toolRefresh.Click += new System.EventHandler(this.toolRefresh_Click);
            // 
            // toolMerge
            // 
            this.toolMerge.Enabled = false;
            this.toolMerge.Image = ((System.Drawing.Image)(resources.GetObject("toolMerge.Image")));
            this.toolMerge.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolMerge.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolMerge.Name = "toolMerge";
            this.toolMerge.Size = new System.Drawing.Size(52, 67);
            this.toolMerge.Text = "Merge";
            this.toolMerge.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolMerge.Click += new System.EventHandler(this.toolMerge_Click);
            // 
            // toolDChangeset
            // 
            this.toolDChangeset.Enabled = false;
            this.toolDChangeset.Image = ((System.Drawing.Image)(resources.GetObject("toolDChangeset.Image")));
            this.toolDChangeset.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolDChangeset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDChangeset.Name = "toolDChangeset";
            this.toolDChangeset.Size = new System.Drawing.Size(105, 67);
            this.toolDChangeset.Text = "Changeset Details";
            this.toolDChangeset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolDChangeset.Click += new System.EventHandler(this.toolDChangeset_Click);
            // 
            // toolDWorkItem
            // 
            this.toolDWorkItem.Enabled = false;
            this.toolDWorkItem.Image = ((System.Drawing.Image)(resources.GetObject("toolDWorkItem.Image")));
            this.toolDWorkItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolDWorkItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDWorkItem.Name = "toolDWorkItem";
            this.toolDWorkItem.Size = new System.Drawing.Size(104, 67);
            this.toolDWorkItem.Text = "Work Item Details";
            this.toolDWorkItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolDWorkItem.Click += new System.EventHandler(this.toolDWorkItem_Click);
            // 
            // toolDServerPath
            // 
            this.toolDServerPath.Enabled = false;
            this.toolDServerPath.Image = ((System.Drawing.Image)(resources.GetObject("toolDServerPath.Image")));
            this.toolDServerPath.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDServerPath.Name = "toolDServerPath";
            this.toolDServerPath.Size = new System.Drawing.Size(150, 20);
            this.toolDServerPath.Text = "Navigate to Server Path";
            this.toolDServerPath.Click += new System.EventHandler(this.toolDServerPath_Click);
            // 
            // toolEditServerPath
            // 
            this.toolEditServerPath.Image = ((System.Drawing.Image)(resources.GetObject("toolEditServerPath.Image")));
            this.toolEditServerPath.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolEditServerPath.Name = "toolEditServerPath";
            this.toolEditServerPath.Size = new System.Drawing.Size(109, 20);
            this.toolEditServerPath.Text = "Edit Server Path";
            this.toolEditServerPath.Click += new System.EventHandler(this.toolEditServerPath_Click);
            // 
            // toolDClipboard
            // 
            this.toolDClipboard.Enabled = false;
            this.toolDClipboard.Image = ((System.Drawing.Image)(resources.GetObject("toolDClipboard.Image")));
            this.toolDClipboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDClipboard.Name = "toolDClipboard";
            this.toolDClipboard.Size = new System.Drawing.Size(151, 20);
            this.toolDClipboard.Text = "Copy Info To Clipboard";
            this.toolDClipboard.Click += new System.EventHandler(this.toolDClipboard_Click);
            // 
            // toolGetRelBranches
            // 
            this.toolGetRelBranches.Enabled = false;
            this.toolGetRelBranches.Image = ((System.Drawing.Image)(resources.GetObject("toolGetRelBranches.Image")));
            this.toolGetRelBranches.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolGetRelBranches.Name = "toolGetRelBranches";
            this.toolGetRelBranches.Size = new System.Drawing.Size(138, 20);
            this.toolGetRelBranches.Text = "Get Related Branches";
            this.toolGetRelBranches.Click += new System.EventHandler(this.toolGetRelBranches_Click);
            // 
            // toolAutoCheckin
            // 
            this.toolAutoCheckin.CheckOnClick = true;
            this.toolAutoCheckin.Image = ((System.Drawing.Image)(resources.GetObject("toolAutoCheckin.Image")));
            this.toolAutoCheckin.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolAutoCheckin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolAutoCheckin.Name = "toolAutoCheckin";
            this.toolAutoCheckin.Size = new System.Drawing.Size(113, 67);
            this.toolAutoCheckin.Text = "Automatic Checkin";
            this.toolAutoCheckin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolAutoCheckin.Click += new System.EventHandler(this.toolAutoCheckin_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chChecked,
            this.chWorkitemID,
            this.chChangesetID,
            this.chDate,
            this.chOwner,
            this.chChanges,
            this.chMergeType,
            this.chSourcePath,
            this.chComment});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.LabelWrap = false;
            this.listView1.Location = new System.Drawing.Point(4, 82);
            this.listView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(974, 488);
            this.listView1.TabIndex = 25;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
            // 
            // chChecked
            // 
            this.chChecked.Text = "";
            this.chChecked.Width = 25;
            // 
            // chWorkitemID
            // 
            this.chWorkitemID.Text = "Work Item";
            this.chWorkitemID.Width = 80;
            // 
            // chChangesetID
            // 
            this.chChangesetID.Text = "Changeset";
            this.chChangesetID.Width = 80;
            // 
            // chDate
            // 
            this.chDate.Text = "Date";
            this.chDate.Width = 80;
            // 
            // chOwner
            // 
            this.chOwner.Text = "Owner";
            this.chOwner.Width = 120;
            // 
            // chChanges
            // 
            this.chChanges.Text = "Changes";
            this.chChanges.Width = 150;
            // 
            // chMergeType
            // 
            this.chMergeType.Text = "Merge Type";
            this.chMergeType.Width = 120;
            // 
            // chSourcePath
            // 
            this.chSourcePath.Text = "Source Path";
            this.chSourcePath.Width = 400;
            // 
            // chComment
            // 
            this.chComment.Text = "Comment";
            this.chComment.Width = 600;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Location = new System.Drawing.Point(5, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(984, 51);
            this.groupBox2.TabIndex = 29;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Merge Target Location";
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(6, 20);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(891, 21);
            this.comboBox1.TabIndex = 31;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(903, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 30;
            this.button1.Text = "Browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MergeWIControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MergeWIControl";
            this.Size = new System.Drawing.Size(989, 635);
            this.ctxMenuChangesets.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ctxMenuChangesets;
        private System.Windows.Forms.ToolStripMenuItem viewChangesetDetailesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewWorkItemDetailesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showServerPathToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader chWorkitemID;
        private System.Windows.Forms.ColumnHeader chChangesetID;
        private System.Windows.Forms.ColumnHeader chDate;
        private System.Windows.Forms.ColumnHeader chOwner;
        private System.Windows.Forms.ColumnHeader chChanges;
        private System.Windows.Forms.ColumnHeader chMergeType;
        private System.Windows.Forms.ColumnHeader chSourcePath;
        private System.Windows.Forms.ColumnHeader chComment;
        private System.Windows.Forms.ToolStripButton toolLinkWIs;
        private System.Windows.Forms.ToolStripButton toolMNormal;
        private System.Windows.Forms.ToolStripButton toolMConservative;
        private System.Windows.Forms.ToolStripButton toolRefresh;
        private System.Windows.Forms.ToolStripButton toolMerge;
        private System.Windows.Forms.ToolStripButton toolResolve;
        private System.Windows.Forms.ToolStripButton toolDChangeset;
        private System.Windows.Forms.ToolStripButton toolDWorkItem;
        private System.Windows.Forms.ToolStripButton toolDServerPath;
        private System.Windows.Forms.ToolStripButton toolDClipboard;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader chChecked;
        private System.Windows.Forms.ToolStripMenuItem editSourcePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolEditServerPath;
        private System.Windows.Forms.ToolStripButton toolGetRelBranches;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ToolStripButton toolAutoCheckin;
    }
}
