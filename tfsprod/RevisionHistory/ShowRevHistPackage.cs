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

            var data = ShowRevHistPackage.BuildBTree(new ItemIdentifier(srvitems.First()));
            (Utilities.paneRevhist.Content as WpfRevisionHistoryControl).data = data;

            // Bring the tool window to the front and give it focus
            ErrorHandler.ThrowOnFailure(frame.Show());
        }

        static dynamic BuildBTree(ItemIdentifier root)
        {
            Cursor origCursor = Cursor.Current;            
            IVsThreadedWaitDialog2 dlg = null;
            bool bcanceled;
            int icanceled, idx = 0;
            DateTime minDate = DateTime.MaxValue, maxDate = DateTime.MinValue;
            
            Cursor.Current = Cursors.WaitCursor;

            dlg = Utilities.CreateThreadedWaitDialog("Collecting information about changesets", "Starting to process changesets...", "status", 100);
            dlg.UpdateProgress("Collecting information about changesets", "Starting to process changesets...", "status", 0, 100, true, out bcanceled);

            var branches = new List<BTreeItem>();
            foreach (var item in Utilities.vcsrv.QueryRootBranchObjects(RecursionType.Full))
            {
                var itm = new BTreeItem
                {
                    Path = item.Properties.RootItem.Item,
                    CreationDate = item.DateCreated,
                    parentPath = item.Properties.ParentBranch == null ? null : item.Properties.ParentBranch.Item,
                    version = (item.Properties.RootItem.Version as ChangesetVersionSpec).ChangesetId,
                    RelatedBranches = Utilities.vcsrv
                        .QueryMergeRelationships(item.Properties.RootItem.Item)
                        .Select(x => new Tuple<BTreeItem, Changeset[]>(new BTreeItem { Path = x.Item }, Utilities.vcsrv
                                .GetMergeCandidates(item.Properties.RootItem.Item, x.Item, RecursionType.Full)
                                .Select(z => z.Changeset).ToArray()))
                        .ToList()
                };

                if (itm.CreationDate < minDate) minDate = itm.CreationDate;
                if (itm.CreationDate > maxDate) maxDate = itm.CreationDate;
                branches.Add(itm);

                dlg.UpdateProgress("Collecting information about changesets", "Processing branch: " + itm.Path, "status", idx++, 100, false, out bcanceled);
                if (bcanceled) break;                
            }
                
            var broot = new BTreeItem();
            foreach(var itm in branches)
            {
                if (itm.parentPath == null)
                {
                    broot.children.Add(itm);
                    itm.parent = broot;
                }
                else
                {
                    itm.parent = branches.FirstOrDefault(x => x.Path == itm.parentPath);
                    if (itm.parent != null)
                        itm.parent.children.Add(itm);
                    else
                        broot.children.Add(itm);
                }

                itm.RelatedBranches = itm.RelatedBranches
                    .Select(x => new Tuple<BTreeItem, Changeset[]>(branches.FirstOrDefault(z => z.Path == x.Item1.Path), x.Item2))
                    .ToList();

                itm.relY = (int)itm.CreationDate.Subtract(minDate).TotalDays;

                foreach (var ch in itm.RelatedBranches.SelectMany(x => x.Item2))
                    if (ch.CreationDate > maxDate) maxDate = ch.CreationDate;
            }

            idx = 0;
            var res = ShowRevHistPackage.TreeDescendants2(broot, ref idx).OrderBy(x => x.relX).ToList();
            res.ForEach(x => Utilities.OutputCommandString(x.relX + " " + (x.parentPath ?? "") + "=" + x.DisplayText));

            Cursor.Current = origCursor;
            dlg.EndWaitDialog(out icanceled);

            return new {
                ylines = (int)(maxDate.Subtract(minDate).TotalDays * 1.1) + 1,
                xlines = idx+1,
                branches = res
            };
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

        static IEnumerable<BTreeItem> TreeDescendants2(BTreeItem root, ref int idx)
        {
            var locbtree = new List<BTreeItem>();
            root.relX = idx++;

            locbtree.Add(root);
            foreach (var node in root.children.OrderByDescending(x => x.CreationDate))
            {
                locbtree.AddRange(ShowRevHistPackage.TreeDescendants2(node, ref idx));
            }

            return locbtree;
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
