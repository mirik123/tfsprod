using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TFSExp.ExtendedMerge;
using tfsprod;

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
}
