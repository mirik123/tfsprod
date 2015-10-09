using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using tfsprod;

namespace TFSExp.ExtendedMerge
{
    static class MergeFactory
    {
        internal const int MergeTypeIndex = 6;
        static internal MergeOptionsEx mergeOpt;
        static internal bool bLinkWIs;

        /// <summary>
        /// Gets or sets the merge options enum, see <b>Microsoft.TeamFoundation.VersionControl.Common.MergeOptionsEx</b>.
        /// </summary>
        /// <value>
        /// The merge options.
        /// </value>
        static public MergeOptionsEx mergeOptions
        {
            get { return mergeOpt; }
            set { mergeOpt = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether WorkItems should be linked also to new Changesets created after merge operation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [link work items]; otherwise, <c>false</c>.
        /// </value>
        static public bool LinkWorkItems
        {
            get { return bLinkWIs; }
            set { bLinkWIs = value; }
        }

        /// <summary>
        /// Shows the work item details dialog, see <b>Microsoft.VisualStudio.TeamFoundation.WorkItemTracking.DocumentService.ShowWorkItem</b>.
        /// </summary>
        /// <param name="wi">The wi.</param>
        static internal void ShowWorkItemDetailsDlg(int wi)
        {
            object _lockToken1 = new object();

            IWorkItemDocument widoc = Utilities.docsrv2.GetWorkItem(Utilities.tfscoll, wi, _lockToken1);
            Utilities.docsrv2.ShowWorkItem(widoc);

            widoc.Release(_lockToken1);
        }

        static internal string FindCommonSharedPath(string[] str)
        {
            return str.Aggregate(delegate(string x, string y)
            {
                if (String.IsNullOrEmpty(x)) return x;

                string[] arrx = x.Split('/');
                string[] arry = y.Split('/');
                StringBuilder strb = new StringBuilder();

                for (int i = 0; i < (arrx.Length < arry.Length ? arrx.Length : arry.Length); i++)
                {
                    if (arrx[i] == arry[i])
                    {
                        strb.Append(arrx[i]);
                        strb.Append("/");
                    }
                    else break;
                }

                return (strb.ToString() == "$" ? "" : strb.ToString());
            });
        }

        static internal string[] QueryBranchObjectOwnership(int chid)
        {
            //split "chid" to collections of 64 items???
            //int i;
            //var branchown = new List<string>();

            /*for (i = 0; i < chid.Length; i+=64 )
            {
                branchown.Union(vcsrv.QueryBranchObjectOwnership(chid.Skip(i).Take(64).ToArray()).Select(x => x.RootItem.Item));
            }*/
            var branchown = Utilities.vcsrv.QueryBranchObjectOwnership(new int[] { chid }).Select(x => x.RootItem.Item);

            return branchown.Distinct().ToArray();
        }

        /// <summary>
        /// Shows the changeset details dilaog, see <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExplorerExt.ViewChangesetDetails</b>.
        /// </summary>
        /// <param name="ch">The ch.</param>
        static internal void ShowChangesetDetailsDlg(int ch)
        {
            Utilities.vcext.ViewChangesetDetails(ch);
        }

        /// <summary>
        /// Navigates to the server path in Source Control Explorer, see <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExplorerExt.Navigate</b> and <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExplorerExt.VsWindowFrame.Show</b>.
        /// </summary>
        /// <param name="srvpath">The srvpath.</param>
        static internal void SelectVersionControlServerPath(string srvpath)
        {
            Utilities.vcext.Explorer.Navigate(srvpath);
            Utilities.vcext.Explorer.VsWindowFrame.Show();
        }

        /// <summary>
        /// Shows the server folder browser dialog, see <b>Microsoft.TeamFoundation.Build.Controls.VersionControlHelper.ShowServerFolderBrowser</b>.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="defaultServerPath">The default server path.</param>
        /// <param name="srvpath">The srvpath.</param>
        static internal void ShowServerFolderDlg(IWin32Window parent, string defaultServerPath, out string srvpath)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.Build.Controls.BuildPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.Build.Controls.VersionControlHelper");

            var parmod = new ParameterModifier(4);
            parmod[3] = true;
            MethodInfo methodInfo = type.GetMethod("ShowServerFolderBrowser",
                                                       new Type[] { typeof(IWin32Window), 
                                                            typeof(VersionControlServer), 
                                                            typeof(string), 
                                                            typeof(string).MakeByRefType() },
                                                        new ParameterModifier[] { parmod });

            object[] obj = new object[] { parent, Utilities.vcsrv, defaultServerPath, null };
            DialogResult res = (DialogResult)methodInfo.Invoke(null, obj);

            srvpath = (res == DialogResult.OK) ? (string)obj[3] : defaultServerPath;
        }

        /// <summary>
        /// Gets the parent/child branches for argument branch path.
        /// </summary>
        /// <param name="trg">The server branch path.</param>
        /// <returns></returns>
        static internal string[] GetRelatedBranches(string trg)
        {
            string[] itmids = null;
            try
            {
                itmids = Utilities.vcsrv.QueryMergeRelationships(trg).Select(x => x.Item).ToArray();
                foreach (var itm in itmids)
                    Utilities.OutputCommandString("GetRelatedBranches: " + itm, "MergeItems");
            }
            finally
            {
                if (itmids == null || itmids.Length == 0) itmids = new string[] { "" };
            }


            return itmids;
        }

        /// <summary>
        /// Updates ListView column "Merge Types" with available merge type from <b>Microsoft.TeamFoundation.VersionControl.Common.MergeOptionsEx</b> enum.
        /// </summary>
        /// <param name="targetPath">The server path of merge target.</param>
        /// <param name="lvcoll">The ListView items collection.</param>
        static internal void SetMergeTypes(string targetPath, IEnumerable<ListViewItem> lvcoll)
        {
            while (targetPath.EndsWith("/"))
                targetPath = targetPath.Substring(0, targetPath.Length-1);
            
            if (!ServerItemExists(targetPath))
            {
                foreach (var item in lvcoll)
                    item.SubItems[MergeTypeIndex].Text = "none";
                return;
            }

            string[] branchesList = GetRelatedBranches(targetPath);

            foreach (var group in lvcoll.GroupBy(x => (x.Tag as ListViewItemTag).sourcePath))
            {
                string mergeType = (group.Key == targetPath || group.Key.Contains(targetPath + "/") || targetPath.Contains(group.Key + "/") ? "none" : 
                                        branchesList == null || !branchesList.Contains(group.Key) ? "baseless" : "force");
                int[] mergeCandidates = null;

                if (mergeType == "force")
                {
                    try
                    {
                        mergeCandidates = Utilities.vcsrv.GetMergeCandidates(group.Key, targetPath, RecursionType.Full)
                            .Select(delegate(MergeCandidate x)
                            {
                                //OutputCommandString("Found Merge Candidate " + x.Changeset.ChangesetId + " for merging from: " + group.Key, AppTitle);
                                return x.Changeset.ChangesetId;
                            }).ToArray();
                    }
                    catch { }
                }

                foreach (var elem in group)
                {
                    elem.SubItems[MergeTypeIndex].Text = (mergeCandidates != null && mergeCandidates.Contains((elem.Tag as ListViewItemTag).ChangesetID) ? "candidate" : mergeType);
                }
            }
        }

        /// <summary>
        /// Checks whether server path exists.
        /// </summary>
        /// <param name="trg">The server path.</param>
        /// <returns>Set to <c>true</c> if server path exists.</returns>
        static internal bool ServerItemExists(string trg)
        {
            try
            {
                var wrkfld = Utilities.vcsrv.GetTeamProjectForServerPath(trg);
                return (wrkfld != null);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Does the main merge operation for selected items and checks-in every changeset.
        /// </summary>
        /// <param name="mfrm">The UserControl.</param>
        /// <param name="lvcoll">The Collection of ListViewItems.</param>
        /// <param name="trg">The target path.</param>
        static internal void DoMerge(UserControl mfrm, IEnumerable<ListViewItem> lvcoll, string trg)
        {
            Workspace wrkspc = Utilities.vcext.Explorer.Workspace;
            if (!ServerItemExists(trg))
            {
                MessageBox.Show("Target server path is cloacked or doesn't exist.");
                return;
            }

            if (wrkspc.GetPendingChanges().Length > 0)
            {
                MessageBox.Show("Please resolve all pending changes before going on.");
                return;
            }

            //object prgmerge = CreateProgressMerge(vcsrv);
            //ShowProgressMerge(prgmerge, mfrm);

            int idx = 0;
            bool bcanceled = false;
            int icanceled;
            IVsThreadedWaitDialog2 dlg = Utilities.CreateThreadedWaitDialog("Merging changesets", "Stating changesets merge...", "status", 100);
            ErrorHandler.ThrowOnFailure(dlg.UpdateProgress("Merging changesets", "Stating changesets merge...", "status", idx++, 100, false, out bcanceled));
            if (bcanceled) return;

            foreach (var lvItem in lvcoll)
            {
                string MergeTypeText = lvItem.SubItems[MergeTypeIndex].Text;
                if (MergeTypeText == "none") continue;

                Changeset ch = Utilities.vcsrv.GetChangeset((lvItem.Tag as ListViewItemTag).ChangesetID);
                Utilities.OutputCommandString("Preparing for merge changeset: " + ch.ChangesetId);

                if (LinkWorkItems)
                {
                    foreach (var workItm in ch.WorkItems)
                        Utilities.OutputCommandString("Associated WorkItem: " + workItm.Id);
                }

                ErrorHandler.ThrowOnFailure(dlg.UpdateProgress("Merging changesets", "Merging changeset: " + ch.ChangesetId, "status", idx++, 100, false, out bcanceled));
                if (bcanceled) return;

                var chverspc = new ChangesetVersionSpec(ch.ChangesetId);
                Utilities.OutputCommandString("Changeset Version: " + chverspc.DisplayString);

                MergeOptionsEx mergeType = (MergeTypeText == "baseless" ? MergeOptionsEx.Baseless : MergeTypeText == "force" ? MergeOptionsEx.ForceMerge : MergeOptionsEx.None);
                mergeType |= mergeOptions;

                GetStatus sts = wrkspc.Merge((lvItem.Tag as ListViewItemTag).sourcePath, trg, chverspc, chverspc, LockLevel.Unchanged, RecursionType.Full, mergeType);
                Utilities.OutputCommandString("Merge summary: MergeOptionsEx=" + mergeType + ", NoActionNeeded=" + sts.NoActionNeeded + ", NumFailures=" + sts.NumFailures + ", NumOperations=" + sts.NumOperations + ", NumUpdated=" + sts.NumUpdated + ", NumWarnings=" + sts.NumWarnings);

                if (mergeOptions != MergeOptionsEx.NoMerge)
                {
                    while (wrkspc.QueryConflicts(ch.Changes.Select(x => x.Item.ServerItem).ToArray(), true).Length > 0)
                    {
                        //foreach (var conflict in conflicts) wrkspc.ResolveConflict(conflict);
                        DialogResult res = MessageBox.Show("Merge conflicts where found. Resolve them or click Cancel to break merge operaton.", "Merge conflicts", MessageBoxButtons.OKCancel);
                        if (res == DialogResult.Cancel)
                        {
                            ErrorHandler.ThrowOnFailure(dlg.EndWaitDialog(out icanceled));
                            return;
                        }

                        if (mfrm.InvokeRequired)
                        {
                            var asynchres = mfrm.BeginInvoke(new MethodInvoker(ShowResolveConflictsDlg));
                            mfrm.EndInvoke(asynchres);
                        }
                        else
                        {
                            ShowResolveConflictsDlg();
                        }
                    }

                    var pendingCh = wrkspc.GetPendingChanges();//ch.Changes.Select(x => new ItemSpec(x.Item.ServerItem, RecursionType.Full, x.Item.DeletionId)).ToArray(), true).ToArray();
                    if (pendingCh.Length == 0)
                    {
                        Utilities.OutputCommandString("No pending changes found.");
                        continue;
                    }

                    var wrkitmch = (LinkWorkItems ? ch.WorkItems.Select(x => new WorkItemCheckinInfo(x, WorkItemCheckinAction.Associate)).ToArray() : null);

                    foreach (var pendChItem in pendingCh)
                        Utilities.OutputCommandString("Found pending change " + pendChItem.ChangeTypeName + ": " + pendChItem.SourceServerItem);

                    int newchid = wrkspc.CheckIn(pendingCh, ch.Committer, ch.Comment, ch.CheckinNote, wrkitmch, null, CheckinOptions.SuppressEvent);
                    Utilities.OutputCommandString("Created new changeset: " + newchid);
                }
                if (idx == 100) idx = 0;
            }

            ErrorHandler.ThrowOnFailure(dlg.EndWaitDialog(out icanceled));
            //CloseProgressMerge(prgmerge);
        }

        /// <summary>
        /// Shows the resolve conflicts dialog, see <b>Microsoft.TeamFoundation.VersionControl.CommandLine.CommandResolve.DisplayResolveDialog</b>.
        /// </summary>
        static internal void ShowResolveConflictsDlg()
        {
            Workspace wrkspc = Utilities.vcext.Explorer.Workspace;

            try
            {
                Assembly assres = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.ControlAddItemsExclude));
                var dlgResolveConflicts = assres.GetTypes().FirstOrDefault(x => x.Name == "DialogResolveConflicts");
                dlgResolveConflicts.InvokeMember("ResolveConflicts", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, new object[] { wrkspc, null, true, null });
            }
            catch(Exception ex)
            {
                Utilities.OutputCommandString(ex.ToString());
                MessageBox.Show(ex.ToString());
            }

            /*var IDEPath = (string)dte.Properties["TFS Productivity Tools", "General"].Item("IDEPath").Value;
            if (string.IsNullOrEmpty(IDEPath))
            {
                Utilities.OutputCommandString("Path to TF.exe is undefined");
                MessageBox.Show("Path to TF.exe is undefined in TOOLS->OPTIONS->TFS Productivity Tools->IDEPath ");
            }
            else if (!Directory.Exists(IDEPath))
            {
                Utilities.OutputCommandString("Path to TF.exe is invalid in TOOLS->OPTIONS->TFS Productivity Tools->IDEPath");
            }
            else
            {
                try
                {
                    Assembly asm = Assembly.LoadFrom(IDEPath + "\\TF.exe");
                    Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.CommandLine.CommandResolve");

                    var objArguments = new Microsoft.TeamFoundation.Client.CommandLine.Arguments("resolve");
                    objArguments.FreeArguments.Add(wrkspc.Folders[0].LocalItem);

                    object res = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] { objArguments });

                    type.InvokeMember("DisplayResolveDialog", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, res, new object[] { wrkspc, new string[] { wrkspc.Folders[0].LocalItem } });
                }
                catch (Exception ex)
                {
                    Utilities.OutputCommandString(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }*/
        }

        #region ProgressMerge Dialog
        /// <summary>
        /// Creates the progress merge dialog, see <b>Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge</b>.
        /// This feature is not implemented.
        /// </summary>
        /// <param name="vcsrv">The VersionControlServer.</param>
        /// <returns></returns>
        static internal object CreateProgressMerge(VersionControlServer vcsrv)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.WorkItemPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge");

            object res = type.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance, null, null, new object[] { vcsrv });
            return res;
        }

        /// <summary>
        /// Shows the progress merge dialog.
        /// This feature is not implemented.
        /// </summary>
        /// <param name="prgmerge">The prgmerge.</param>
        /// <param name="owner">The owner.</param>
        static internal void ShowProgressMerge(object prgmerge, IWin32Window owner)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.WorkItemPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge");

            type.InvokeMember("Show", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, prgmerge, new object[] { owner });
        }

        /// <summary>
        /// Closes the progress merge dialog.
        /// This feature is not implemented.
        /// </summary>
        /// <param name="prgmerge">The prgmerge.</param>
        static internal void CloseProgressMerge(object prgmerge)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.WorkItemPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge");

            type.InvokeMember("Close", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, prgmerge, null);
        }

        #endregion
    }

    /// <summary>
    /// Tag class for keeping custom properties inside ListView rows.
    /// </summary>
    public class ListViewItemTag
    {
        /// <summary>
        /// Changeset ID.
        /// </summary>
        public int ChangesetID;
        /// <summary>
        /// Work Item ID.
        /// </summary>
        public int WorkitemID;
        /// <summary>
        /// Root server path for Changeset items.
        /// </summary>
        public string sourcePath;
        /// <summary>
        /// Changeset check-in date.
        /// </summary>
        public DateTime date;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewItemTag"/> class.
        /// </summary>
        /// <param name="chObject">The <see cref="ChangesetID"/> parameter.</param>
        /// <param name="wiObject">The <see cref="WorkitemID"/> parameter.</param>
        /// <param name="strPath">The <see cref="sourcePath"/> parameter.</param>
        /// <param name="dt">The <see cref="date"/> parameter.</param>
        public ListViewItemTag(int chObject, int wiObject, string strPath, DateTime dt)
        {
            WorkitemID = wiObject;
            ChangesetID = chObject;
            sourcePath = strPath;
            date = dt;
        }
    }

    /// <summary>
    /// Custom comparer for Listview items for different columns and types: string, number, date.
    /// See <see cref="ListViewItemTag"/> for compared fields list.
    /// </summary>
    public class MyListViewSorter : IComparer
    {
        private int sortcol = 2;
        private SortOrder sortord = SortOrder.Ascending;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyListViewSorter"/> class.
        /// </summary>
        public MyListViewSorter() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MyListViewSorter"/> class.
        /// </summary>
        /// <param name="coll">The collumn number</param>
        /// <param name="sort">The sort order, see <see cref="System.Windows.Forms.SortOrder"/>.</param>
        public MyListViewSorter(int coll, SortOrder sort)
        {
            sortcol = coll;
            sortord = sort;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>      
        public int Compare(object x, object y)
        {
            return Compare(x as ListViewItem, y as ListViewItem);
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// Optimized for <see cref="ListViewItem"/> objects.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>      
        public int Compare(ListViewItem x, ListViewItem y)
        {
            int result;

            if (sortcol == 0)
                result = (x.Checked == y.Checked ? 0 : x.Checked ? 1 : -1);
            else if (sortcol == 1)
                result = (x.Tag as ListViewItemTag).WorkitemID.CompareTo((y.Tag as ListViewItemTag).WorkitemID);
            else if (sortcol == 2)
                result = (x.Tag as ListViewItemTag).ChangesetID.CompareTo((y.Tag as ListViewItemTag).ChangesetID);
            else if (sortcol == 3)
                result = (x.Tag as ListViewItemTag).date.CompareTo((y.Tag as ListViewItemTag).date);
            else
                result = String.Compare(x.SubItems[sortcol].Text, y.SubItems[sortcol].Text);

            if (sortord == SortOrder.Descending) result = -result;

            return result;
        }
    }
}
