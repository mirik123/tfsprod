using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSExt.ShowRevHist
{
    public class BTreeItem
    {
        public int relX;
        public int relY;
        public BTreeItem parent { get; set; }
        public string parentPath { get; set; }
        public string Path { get; set; }
        public int version { get; set; }
        public DateTime CreationDate { get; set; }
        public List<Tuple<BTreeItem, Changeset[]>> RelatedBranches { get; set; }
        public IList<BTreeItem> children { get; set; }

        public string DisplayText
        {
            get
            {
                return string.IsNullOrEmpty(Path) ? "" : string.Format("{0} ({1} - {2})", Path, version, CreationDate.ToShortDateString());
            }
        }

        public IList<Changeset> Items
        {
            get
            {
                var childNodes = new List<Changeset>();               
                foreach (var entry in this.RelatedBranches.SelectMany(x => x.Item2))
                    if (!childNodes.Any(x => x.ChangesetId == entry.ChangesetId))
                        childNodes.Add(entry);

                //if (childNodes.Count() == 0 && this.children.Count() > 0)
                 //   childNodes.Add(new BTreeChangeset { ch = null });

                //foreach (var group in this.children)
                //    childNodes.Add(group);

                return childNodes.OrderBy(x => x.CreationDate).ToList();
            }
        }

        public BTreeItem()
        {
            parent = null;
            RelatedBranches = new List<Tuple<BTreeItem, Changeset[]>>();
            children = new List<BTreeItem>();
        }
    }
}
