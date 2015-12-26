using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Linq;
using System.Windows.Forms;
using tfsprod;

namespace TFSExp.ExtendedMerge
{
    /// <summary>
    /// This class inherites from UserControl class.
    /// </summary>
    public partial class MergeWIControl : UserControl
    {
        private ListViewItemTag clickedItem = null;

        /// <summary>
        /// Suppresses the ListView events when its items are changed by the inner code functions.
        /// </summary>
        public bool SuppressEvents;

        /// <summary>
        /// Gets or sets the target server path where to merge the items.
        /// </summary>
        /// <value>
        /// The name of the target server path.
        /// </value>
        public string defServerName
        {
            get
            {
                return comboBox1.Text;
            }
            set
            {
                comboBox1.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the Tag object from Listview clicked item.
        /// See <see cref="ListViewItemTag"/>.
        /// </summary>
        /// <value>
        /// The Tag object from clicked item.
        /// </value>
        private ListViewItemTag ClickedItem
        {
            get
            {
                return clickedItem;
            }
            set
            {
                clickedItem = value;
                if (clickedItem == null)
                {
                    this.toolDChangeset.Enabled = false;
                    this.toolDClipboard.Enabled = false;
                    this.toolDServerPath.Enabled = false;
                    this.toolDWorkItem.Enabled = false;
                    this.toolGetRelBranches.Enabled = false;
                }
                else
                {
                    this.toolDChangeset.Enabled = true;
                    this.toolDClipboard.Enabled = true;
                    this.toolDServerPath.Enabled = true;
                    this.toolDWorkItem.Enabled = true;
                    this.toolGetRelBranches.Enabled = true;
                }
            }
        }


        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            MergeFactory.LinkWorkItems = this.toolLinkWIs.Checked;
            MergeFactory.mergeOptions = this.toolMConservative.Checked ? MergeOptionsEx.Conservative : MergeOptionsEx.None;

            this.toolMConservative.Enabled = true;
            this.toolMNormal.Enabled = true;
            this.toolLinkWIs.Enabled = true;
            this.toolMerge.Enabled = true;
            this.toolRefresh.Enabled = true;
            this.toolResolve.Enabled = true;
            this.button1.Enabled = true;

            var tableSettings = this.toolStrip1.LayoutSettings as TableLayoutSettings;
            tableSettings.RowCount = 3;
            tableSettings.ColumnCount = 8;

            tableSettings.SetCellPosition(toolAutoCheckin, new TableLayoutPanelCellPosition(0, 0));
            tableSettings.SetRowSpan(toolAutoCheckin, 3);

            tableSettings.SetCellPosition(toolMNormal, new TableLayoutPanelCellPosition(1, 0));
            tableSettings.SetCellPosition(toolMConservative, new TableLayoutPanelCellPosition(1, 1));
            tableSettings.SetCellPosition(toolLinkWIs, new TableLayoutPanelCellPosition(1, 2));

            tableSettings.SetCellPosition(toolMerge, new TableLayoutPanelCellPosition(2, 0));
            tableSettings.SetRowSpan(toolMerge, 3);

            tableSettings.SetCellPosition(toolResolve, new TableLayoutPanelCellPosition(3, 0));
            tableSettings.SetRowSpan(toolResolve, 3);

            tableSettings.SetCellPosition(toolDChangeset, new TableLayoutPanelCellPosition(4, 0));
            tableSettings.SetRowSpan(toolDChangeset, 3);

            tableSettings.SetCellPosition(toolDWorkItem, new TableLayoutPanelCellPosition(5, 0));
            tableSettings.SetRowSpan(toolDWorkItem, 3);

            tableSettings.SetCellPosition(toolRefresh, new TableLayoutPanelCellPosition(6, 0));
            tableSettings.SetCellPosition(toolEditServerPath, new TableLayoutPanelCellPosition(6, 1));
            tableSettings.SetCellPosition(toolDServerPath, new TableLayoutPanelCellPosition(6, 2));

            tableSettings.SetCellPosition(toolDClipboard, new TableLayoutPanelCellPosition(7, 0));
            tableSettings.SetCellPosition(toolGetRelBranches, new TableLayoutPanelCellPosition(7, 1));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeWIControl"/> class.
        /// </summary>
        public MergeWIControl()
        {
            SuppressEvents = true;
            InitializeComponent();
        }

        /// <summary>
        /// Deletes all ListView items.
        /// </summary>
        public void ClearGrids()
        {
            listView1.Items.Clear();
        }

        /// <summary>
        /// Sorts the ListView items using custom comparer <see cref="MyListViewSorter"/>.
        /// </summary>
        public void SortItems()
        {
            SuppressEvents = true;
            listView1.Sorting = SortOrder.Ascending;
            listView1.ListViewItemSorter = new MyListViewSorter(2, SortOrder.Ascending);
            SuppressEvents = false;
        }

        /// <summary>
        /// Creates the new ListView item.
        /// </summary>
        /// <param name="wiID">The Work Item ID.</param>
        /// <param name="chID">The Chengeset ID.</param>
        /// <param name="chDate">The Changeset check-in date.</param>
        /// <param name="chOwner">The Changeset creator.</param>
        /// <param name="sourcePath">The Changeset root server path.</param>
        /// <param name="chchanges">The Changes types comma separated list.</param>
        /// <param name="chcomment">The Changeset comment.</param>
        /// <param name="tag">The ListView item Tag object, see <see cref="ListViewItemTag"/>.</param>
        /// <returns></returns>
        public ListViewItem AddNewGridItem(string wiID, string chID, string chDate, string chOwner, string sourcePath, string chchanges, string chcomment, ListViewItemTag tag)
        {
            ListViewItem lvitem;
            if (!listView1.Items.ContainsKey(chID))
            {
                lvitem = new ListViewItem(new string[] { "", wiID, chID, chDate, chOwner, chchanges, "none", sourcePath, chcomment });
                lvitem.Name = chID;
                lvitem.Checked = true;
                lvitem.Tag = tag;
                listView1.Items.Add(lvitem);
            }
            else
            {
                lvitem = listView1.Items[chID];
                lvitem.SubItems[1].Text += ","+wiID;
            }

            return lvitem;
        }

        public void UpdateRelatedBranchesCombo()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(listView1.Items.Cast<ListViewItem>()
                        .Select(x => (x.Tag as ListViewItemTag).sourcePath)
                        .Distinct()
                        .SelectMany(x => MergeFactory.GetRelatedBranches(x)).ToArray());
        }

        /// <summary>
        /// Updates "Merge Types" column in Listview, see <see cref="MergeFactory.SetMergeTypes"/>.
        /// </summary>
        public void SetMergeTypes()
        {
            SuppressEvents = true;
            MergeFactory.SetMergeTypes(defServerName, listView1.Items.Cast<ListViewItem>());
            SuppressEvents = false;
        }

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// Shows folder browser dialog on version control server, see <see cref="MergeFactory.ShowServerFolderDlg"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            string srvpath;

            Utilities.ShowServerFolderDlg(this, defServerName, out srvpath);
            defServerName = srvpath;
            SetMergeTypes();
        }

        /// <summary>
        /// Does the main merge action, see <see cref="MergeFactory.DoMerge"/>.
        /// </summary>
        private void DoMerge()
        {
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            SortItems();
            MergeFactory.DoMerge(this as UserControl, listView1.CheckedItems.Cast<ListViewItem>(), defServerName);

            Cursor.Current = origCursor;
        }

        /// <summary>
        /// Updates ListView column "Server Path".
        /// </summary>
        /// <param name="itm">The itm.</param>
        private void EditServerPath(ListViewItemTag itm)
        {
            if (itm == null) return;
            
            string res = Microsoft.VisualBasic.Interaction.InputBox("Enter new value for Source Path", Utilities.AppTitle, itm.sourcePath);
            if (string.IsNullOrWhiteSpace(res)) return;

            while (res.EndsWith("/")) res = res.Substring(0, res.Length - 1);
            while (res.EndsWith("\\")) res = res.Substring(0, res.Length - 1);

            SuppressEvents = true;
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                item.SubItems[7].Text = res;
                (item.Tag as ListViewItemTag).sourcePath = res;
            }
            UpdateRelatedBranchesCombo();
            SuppressEvents = false;
            SetMergeTypes();
        }

        /// <summary>
        /// Handles the MouseClick event of the listView1 control.
        /// Shows ListView context menu, see <see cref="ctxMenuChangesets_ItemClicked"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            ClickedItem = listView1.GetItemAt(e.X, e.Y).Tag as ListViewItemTag;
            if (e.Button == MouseButtons.Right)
            {
                ctxMenuChangesets.Show(sender as Control,e.Location);
            }
        }

        /// <summary>
        /// Handles the ItemClicked event of the ctxMenuChangesets context menu.
        /// viewChangesetDetailesToolStripMenuItem  - the same as <see cref="toolDChangeset_Click"/>
        /// viewWorkItemDetailesToolStripMenuItem   - the same as <see cref="toolDWorkItem_Click"/>
        /// copyToClipboardToolStripMenuItem        - the same as <see cref="toolDClipboard_Click"/>
        /// showServerPathToolStripMenuItem         - the same as <see cref="toolDServerPath_Click"/>
        /// editSourcePathToolStripMenuItem         - the same as <see cref="toolEditServerPath_Click"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs"/> instance containing the event data.</param>
        private void ctxMenuChangesets_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
            if (e.ClickedItem.Name == "viewChangesetDetailesToolStripMenuItem")
            {
                int ch = ClickedItem.ChangesetID;
                //show changeset details dialog
                Utilities.ShowChangesetDetailsDlg(ch);
            }
            else if (e.ClickedItem.Name == "viewWorkItemDetailesToolStripMenuItem")
            {
                int wi = ClickedItem.WorkitemID;
                //show workitem details dialog
                if (wi != 0) Utilities.ShowWorkItemDetailsDlg(wi);
            }
            else if (e.ClickedItem.Name == "copyToClipboardToolStripMenuItem")
            {
                string res = String.Format("Changeset={0}, WorkItem={1}, Date={2}, ServerPath={3}", ClickedItem.ChangesetID, ClickedItem.WorkitemID, ClickedItem.date, ClickedItem.sourcePath);

                Clipboard.Clear();
                //Clipboard.SetText(res, TextDataFormat.CommaSeparatedValue);
                Clipboard.SetDataObject(res, true);
            }
            else if (e.ClickedItem.Name == "showServerPathToolStripMenuItem")
            {
                string srvpath = ClickedItem.sourcePath;
                //goto source path
                Utilities.SelectVersionControlServerPath(srvpath);
            }
            else if (e.ClickedItem.Name == "editSourcePathToolStripMenuItem")
            {
                //edit source path
                EditServerPath(ClickedItem);
            }
        }

        /// <summary>
        /// Handles the ColumnClick event of the listView1 control.
        /// Sorts ListView items with custom comparer <see cref="MyListViewSorter"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ColumnClickEventArgs"/> instance containing the event data.</param>
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SuppressEvents = true;
            if (e.Column == 0)
            {
                if (listView1.Columns[0].Text == '\u2714'.ToString())
                {
                    listView1.Columns[0].Text = "";
                    foreach (ListViewItem item in listView1.Items) item.Checked = false;
                }
                else
                {
                    listView1.Columns[0].Text = '\u2714'.ToString();
                    foreach (ListViewItem item in listView1.Items) item.Checked = true;
                }
            }
            else
            {
                listView1.Sorting = (listView1.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
                listView1.ListViewItemSorter = new MyListViewSorter(e.Column, listView1.Sorting);
            }   
            SuppressEvents = false;
        }

        /// <summary>
        /// Handles the Click event of the toolMerge control.
        /// Does the main merge operation, see <see cref="DoMerge"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolMerge_Click(object sender, EventArgs e)
        {
            DoMerge();
        }

        /// <summary>
        /// Handles the Click event of the toolRefresh control, see <see cref="SetMergeTypes"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolRefresh_Click(object sender, EventArgs e)
        {
            SetMergeTypes();
        }

        /// <summary>
        /// Handles the Click event of the toolMDiscard control.
        /// Sets <see cref="MergeFactory.mergeOptions"/> to MergeOptionsEx.NoMerge.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolMDiscard_Click(object sender, EventArgs e)
        {
            //if (!toolMDiscard.Checked) return;

            MergeFactory.mergeOptions = MergeOptionsEx.NoMerge;

            toolMConservative.Checked = false;
            toolMNormal.Checked = false;
        }

        /// <summary>
        /// Handles the Click event of the toolMAlwaysAcceptMine control.
        /// Sets <see cref="MergeFactory.mergeOptions"/> to MergeOptionsEx.AlwaysAcceptMine.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolMAlwaysAcceptMine_Click(object sender, EventArgs e)
        {
            //if (!toolMAlwaysAcceptMine.Checked) return;

            MergeFactory.mergeOptions = MergeOptionsEx.AlwaysAcceptMine;

            toolMConservative.Checked = false;
            toolMNormal.Checked = false;
        }

        /// <summary>
        /// Handles the Click event of the toolMConservative control.
        /// Sets <see cref="MergeFactory.mergeOptions"/> to MergeOptionsEx.Conservative.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolMConservative_Click(object sender, EventArgs e)
        {
            if (!toolMConservative.Checked) return;

            MergeFactory.mergeOptions = MergeOptionsEx.Conservative;

            toolMNormal.Checked = false;
        }

        /// <summary>
        /// Handles the Click event of the toolMNormal control.
        /// Sets <see cref="MergeFactory.mergeOptions"/> to MergeOptionsEx.None.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolMNormal_Click(object sender, EventArgs e)
        {
            if (!toolMNormal.Checked) return;

            MergeFactory.mergeOptions = MergeOptionsEx.None;

            toolMConservative.Checked = false;
        }

        /// <summary>
        /// Handles the Click event of the toolLinkWIs control.
        /// Sets the <see cref="MergeFactory.LinkWorkItems"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolLinkWIs_Click(object sender, EventArgs e)
        {
            MergeFactory.LinkWorkItems = this.toolLinkWIs.Checked;
        }

        /// <summary>
        /// Handles the Click event of the toolResolve control.
        /// Shows ResolveConflicts dialog, see <see cref="MergeFactory.ShowResolveConflictsDlg"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolResolve_Click(object sender, EventArgs e)
        {
            Utilities.ShowResolveConflictsDlg();
        }

        /// <summary>
        /// Handles the Click event of the toolDChangeset control.
        /// Shows Changeset details dialog, see <see cref="MergeFactory.ShowChangesetDetailsDlg"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolDChangeset_Click(object sender, EventArgs e)
        {
            int ch = ClickedItem.ChangesetID;
            Utilities.ShowChangesetDetailsDlg(ch);
        }

        /// <summary>
        /// Handles the Click event of the toolDWorkItem control.
        /// Shows WorkItem details dialog, see <see cref="MergeFactory.ShowWorkItemDetailsDlg"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolDWorkItem_Click(object sender, EventArgs e)
        {
            int wi = ClickedItem.WorkitemID;
            if (wi != 0) Utilities.ShowWorkItemDetailsDlg(wi);
            else MessageBox.Show("No WorkItem is connected to Changeset " + ClickedItem.ChangesetID, Utilities.AppTitle);
        }

        /// <summary>
        /// Handles the Click event of the toolDServerPath control.
        /// Navigates to the selected server path in Source Control Explorer, see <see cref="MergeFactory.SelectVersionControlServerPath"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolDServerPath_Click(object sender, EventArgs e)
        {
            string srvpath = ClickedItem.sourcePath;
            Utilities.SelectVersionControlServerPath(srvpath);
        }

        /// <summary>
        /// Handles the Click event of the toolDClipboard control.
        /// Copies selected ListView item data contained in Tag object to Clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolDClipboard_Click(object sender, EventArgs e)
        {
            string res = String.Format("Changeset={0}, WorkItem={1}, Date={2}, ServerPath={3}", ClickedItem.ChangesetID, ClickedItem.WorkitemID, ClickedItem.date, ClickedItem.sourcePath);

            Clipboard.Clear();
            Clipboard.SetDataObject(res, true);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the listView1 control.
        /// Updates <see cref="ClickedItem"/> object to currently selected ListView item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                ClickedItem = null;
            }
            else
            {
                ClickedItem = listView1.SelectedItems[0].Tag as ListViewItemTag;
            }
        }

        /// <summary>
        /// Handles the Click event of the toolEditServerPath control.
        /// Updates ListView column "Server Path" for selected items, see <see cref="EditServerPath"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolEditServerPath_Click(object sender, EventArgs e)
        {
            EditServerPath(ClickedItem);
        }

        /// <summary>
        /// Handles the Click event of the toolGetRelBranches control.
        /// Shows MessageBox with branches that have child/parent relationship for source and target paths.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolGetRelBranches_Click(object sender, EventArgs e)
        {
            string str1 = MergeFactory.GetRelatedBranches(defServerName).Aggregate((x, y) => x + Environment.NewLine + "---" + y);
            string str2 = MergeFactory.GetRelatedBranches(ClickedItem.sourcePath).Aggregate((x, y) => x + Environment.NewLine + "---" + y);

            MessageBox.Show("Related branches for merge target path:" + Environment.NewLine + "---" + str1 + Environment.NewLine + "Related branches for merge source path:" + Environment.NewLine + "---" + str2, Utilities.AppTitle);
        }

        private void toolAutoCheckin_Click(object sender, EventArgs e)
        {
            MergeFactory.AutomaticCheckin = this.toolAutoCheckin.Checked;
            if (!this.toolAutoCheckin.Checked)
                MergeFactory.LinkWorkItems = this.toolLinkWIs.Checked = false;
        }
    }
}
