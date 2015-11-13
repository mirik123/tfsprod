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
        public int version { get; set; }
        public DateTime CreationDate { get; set; }
        public Dictionary<string, Changeset[]> RelatedBranches { get; set; }
        public IList<BTreeItem> children { get; set; }

        public string DisplayText
        {
            get
            {
                return string.Format("{0} ({1} - {2})", Path, version, CreationDate.ToShortDateString());
            }
        }

        public IList<object> Items
        {
            get
            {
                IList<object> childNodes = new List<object>();               
                foreach (var entry in this.RelatedBranches.Values.SelectMany(x => x))
                    if (!childNodes.Any(x => (x as BTreeChangeset).ch.ChangesetId == entry.ChangesetId)) 
                        childNodes.Add(new BTreeChangeset { ch = entry });

                if (childNodes.Count() == 0 && this.children.Count() > 0)
                    childNodes.Add(new BTreeChangeset { ch = null });

                foreach (var group in this.children)
                    childNodes.Add(group);

                return childNodes.OrderBy(x => (x as dynamic).CreationDate).ToList();
            }
        }

        public BTreeItem()
        {
            parent = null;
            RelatedBranches = new Dictionary<string, Changeset[]>();
            children = new List<BTreeItem>();
        }
    }

    public class BTreeChangeset
    {
        public Changeset ch;

        public DateTime CreationDate
        {
            get
            {
                return ch != null ? ch.CreationDate : DateTime.MinValue;
            }
        }

        public string DisplayText
        {
            get
            {
                if (ch == null)
                    return "<base>";

                return string.Format("{0} ({1})", ch.ChangesetId, ch.CreationDate.ToShortDateString());
            }
        }
    }
}
