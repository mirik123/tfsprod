using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TFSExp.ExtendedMerge;
using tfsprod;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Collections.ObjectModel;

namespace TFSExt.ShowRevHist
{
    public class ShowRevHistPackage
    {
        public static void ShowRevHistCallback(object sender, EventArgs e)
        {
            var srvitems = Utilities.vcext.Explorer.SelectedItems.Select(x => x.SourceServerPath);
            if (srvitems == null || srvitems.Count() != 1)
            {
                MessageBox.Show("Select only one path.", Utilities.AppTitle);
                return;
            }
            
            IVsWindowFrame frame = Utilities.paneRevhist.Frame as IVsWindowFrame;
            if (frame == null)
            {
                MessageBox.Show("Failed to initialize " + Utilities.AppTitle + ": Frame was null.", Utilities.AppTitle);
                return;
            }

            (Utilities.paneRevhist.Content as WpfRevisionHistoryControl).btreeview.ItemsSource = ShowRevHistPackage.BuildBTree().children;

            // Bring the tool window to the front and give it focus
            ErrorHandler.ThrowOnFailure(frame.Show());
        }

        static BTreeItem BuildBTree()
        {
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            IVsThreadedWaitDialog2 dlg = null;
            bool bcanceled;
            int icanceled;
            int idx = 0;
            
            dlg = Utilities.CreateThreadedWaitDialog("Collecting information about changesets", "Starting to process changesets...", "status", 100);
            dlg.UpdateProgress("Collecting information about changesets", "Starting to process changesets...", "status", 0, 100, true, out bcanceled);

            var btree = new BTreeItem();
            foreach (var item in Utilities.vcsrv.QueryRootBranchObjects(RecursionType.Full))
            {
                var itm = new BTreeItem
                {
                    Path = item.Properties.RootItem.Item,
                    DateCreated = item.DateCreated,
                    parent = item.Properties.ParentBranch == null ? null : item.Properties.ParentBranch.Item,
                    RelatedBranches = item.RelatedBranches.Select(x => x.Item).ToDictionary(x => x, y => (Changeset[])null)
                };

                var itemSpec = new ItemSpec(itm.Path, RecursionType.Full);

                foreach (var key in itm.RelatedBranches.Keys)
                {
                    var cands = Utilities.vcsrv.GetMergeCandidates(itemSpec, key, MergeOptionsEx.NoMerge);
                    itm.RelatedBranches[key] = cands.Select(x => x.Changeset).ToArray();
                }

                if (itm.parent == null)
                    btree.children.Add(itm);
                else
                {
                    var newparent = ShowRevHistPackage.TreeDescendants(btree).FirstOrDefault(x => x.Path == itm.parent);
                    if (newparent != null)
                        newparent.children.Add(itm);
                    else
                        btree.children.Add(itm);
                }

                dlg.UpdateProgress("Collecting information about changesets", "Processing changeset: " + "ch.ChangesetId", "status", idx++, 100, false, out bcanceled);
                if (bcanceled) break;
            }

            Cursor.Current = origCursor;
            dlg.EndWaitDialog(out icanceled);

            return btree;
        }

        static IEnumerable<BTreeItem> TreeDescendants(BTreeItem root)
        {
            yield return root;
            foreach (var node in root.children)
            {
                foreach (var child in ShowRevHistPackage.TreeDescendants(node))
                {
                    yield return child;
                }
            }
        }
        
        static void ShowRevHistCallback_obsolete(object sender, EventArgs e)
        {
            var srvitems = Utilities.vcext.Explorer.SelectedItems.Select(x => x.SourceServerPath);
            if (srvitems == null || srvitems.Count() != 1)
            {
                MessageBox.Show("Select only one path.", Utilities.AppTitle);
                return;
            }

            var allbranches = Utilities.vcsrv.QueryRootBranchObjects(RecursionType.Full).Select(x => x.Properties.RootItem.Item);

            var branchHistory = Utilities.vcsrv.GetBranchHistory(srvitems.Select(x => new ItemSpec(x, RecursionType.None)).ToArray(), VersionSpec.Latest);
            foreach (BranchHistoryTreeItem item in branchHistory[0][0].Children)
            {
                ShowChildren(item);
            }

            var txtlst = new List<string>();
            branchHistory = Utilities.vcsrv.GetBranchHistory(srvitems.Select(x => new ItemSpec(x, RecursionType.Full)).ToArray(), VersionSpec.Latest);
            foreach (var arritem in branchHistory)
            {
                foreach (var item in arritem)
                {
                    var txt = string.Format("Branch level {0}", item.Level);

                    if (item.Relative.BranchFromItem != null)
                    {
                        var br = allbranches.FirstOrDefault(x => item.Relative.BranchFromItem.ServerItem.Contains(x));
                        txt += ", "+string.Format("Branched from {0}, chid {1}", br, item.Relative.BranchFromItem.ChangesetId);
                    }

                    if (item.Relative.BranchToItem != null)
                    {
                        var br = allbranches.FirstOrDefault(x => item.Relative.BranchToItem.ServerItem.Contains(x));
                        txt += ", "+string.Format("Branched to {0}, chid {1}", br, item.Relative.BranchToItem.ChangesetId);
                    }

                    txtlst.Add(txt);
                }
            }

            txtlst.Distinct().ToList().ForEach(x => Utilities.OutputCommandString(x));
        }

        static void ShowChildren(BranchHistoryTreeItem parent)
        {
            foreach (BranchHistoryTreeItem item in parent.Children)
            {
                Utilities.OutputCommandString(string.Format("Root branched to {0}", item.Relative.BranchToItem.ServerItem));

                if (item.Children.Count > 0)
                {
                    foreach (BranchHistoryTreeItem child in item.Children)
                    {
                        ShowChildren(child);
                    }
                }
            }
        }
    }

    [Guid("36CCAA7C-9C60-4EE0-8A2C-A9B0DDBA46A2")]
    public class RevisionHistoryPane : ToolWindowPane
    {
        public RevisionHistoryPane()
            : base(null)
        {
            Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Entering constructor for class {0}.", this.GetType().Name));

            this.Caption = Utilities.AppTitle;
            this.BitmapResourceID = 700;
            this.BitmapIndex = 1;
            base.Content = new WpfRevisionHistoryControl();
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            ((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_BitmapResource, 700);
            ((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_BitmapIndex, 1);
            ((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
        }
    }
}
