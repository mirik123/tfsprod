using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSExt.ShowRevHist
{
    public class BTreeItem
    {
        public string parent { get; set; }
        public string Path { get; set; }
        public DateTime DateCreated { get; set; }
        public Dictionary<string, Changeset[]> RelatedBranches { get; set; }
        public IList<BTreeItem> children { get; set; }

        public IList<object> Items
        {
            get
            {
                IList<object> childNodes = new List<object>();               
                foreach (var entry in this.RelatedBranches.Values.SelectMany(x => x))
                    if (!childNodes.Any(x => (x as Changeset).ChangesetId == entry.ChangesetId)) childNodes.Add(entry);

                foreach (var group in this.children)
                    childNodes.Add(group);

                return childNodes;
            }
        }

        public BTreeItem()
        {
            parent = null;
            RelatedBranches = new Dictionary<string, Changeset[]>();
            children = new List<BTreeItem>();
        }
    }
}
