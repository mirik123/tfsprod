using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking.Extensibility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using tfsprod;

namespace TFSExt.ChangeLinkTypes
{
    public static class ChangeLinkTypesPackage
    {
        public static void QueryLinkTypesCallback(object sender, EventArgs e)
        {
            //IntPtr hier;
            //uint itemid;
            //IVsMultiItemSelect dummy;
            //string canonicalName;
            bool bcanceled;
            int icanceled;
            string OperationCaption = "Change Work Items Link Types";
            IVsThreadedWaitDialog2 dlg = null;

            var lkdlg = new EditLinkTypeDialog(Utilities.wistore);
            var dlgResult = lkdlg.ShowDialog();
            if (dlgResult != DialogResult.OK) return;

            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                dlg = Utilities.CreateThreadedWaitDialog(OperationCaption, "Parsing query...", "status", 100);
                dlg.UpdateProgress(OperationCaption, "Parsing query...", "status", 1, 100, true, out bcanceled);

                var WIQueriesPageExt = Utilities.teamExplorer.CurrentPage.GetService<IWorkItemQueriesExt>(); 
                var qItem = WIQueriesPageExt.SelectedQueryItems.First();

                int[] qdata;
                int changedlinks = 0;
                bcanceled = ExecuteQueryLinkTypes(qItem as QueryDefinition, qItem.Project.Name, out qdata);
                dlg.UpdateProgress(OperationCaption, "Changing Link Types...", "status", 1, 100, false, out bcanceled);

                if (!bcanceled) changedlinks = ChangeLinkTypes(qdata, lkdlg.fromlink, lkdlg.tolink);

                dlg.EndWaitDialog(out icanceled);

                if (!bcanceled) MessageBox.Show(changedlinks + " links were changed.", Utilities.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.EndWaitDialog(out icanceled);
                Cursor.Current = origCursor;
            }
            catch (Exception ex)
            {
                if(dlg != null) dlg.EndWaitDialog(out icanceled);
                Cursor.Current = origCursor;
                Utilities.OutputCommandString(ex.ToString());
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message, Utilities.AppTitle, MessageBoxButtons.OK);
            }
        }

        public static int ChangeLinkTypes(int[] qdata, WorkItemLinkType fromlink, WorkItemLinkType tolink)
        {
            int changedlinks = 0;

            foreach(int itm in qdata)
            {
                if (itm == 0) continue;

                WorkItem wi = Utilities.wistore.GetWorkItem(itm);
                foreach (var link in wi.Links.OfType<RelatedLink>().Where(x => x.LinkTypeEnd.LinkType.ReferenceName == fromlink.ReferenceName).ToList())
                {
                    WorkItemLinkTypeEnd linkTypeEnd = Utilities.wistore.WorkItemLinkTypes.LinkTypeEnds[(link.LinkTypeEnd.IsForwardLink ? tolink.ForwardEnd.Name : tolink.ReverseEnd.Name)];
                    Utilities.OutputCommandString(string.Format("Updated WorkItemID={0}, OriginalLink={1}, NewLink={2}", wi.Id, link.LinkTypeEnd.Name, linkTypeEnd.Name));

                    if (wi.IsDirty)
                    {
                        try
                        {
                            wi.Save();
                        }
                        catch (Exception ex)
                        {
                            var result = MessageBox.Show(ex.Message, "Failed to save dirty Work Item #" + wi.Id, MessageBoxButtons.AbortRetryIgnore);
                            Utilities.OutputCommandString(ex.ToString());
                            if (result == DialogResult.Abort) return changedlinks;
                            else continue;
                        }
                    }
                    try
                    {
                        wi.Links.Add(new RelatedLink(linkTypeEnd, link.RelatedWorkItemId));
                        wi.Save();
                        changedlinks++;
                    }
                    catch (Exception ex)
                    {
                        var result = MessageBox.Show(ex.Message,"Failed to add new link to Work Item #"+wi.Id,MessageBoxButtons.AbortRetryIgnore);
                        Utilities.OutputCommandString(ex.ToString());
                        if (result == DialogResult.Abort) return changedlinks;
                        else continue;
                    }
                    try
                    {
                        wi.Links.Remove(link);
                        wi.Save();
                    }
                    catch (Exception ex)
                    {
                        var result = MessageBox.Show(ex.Message, "Failed to remove original link from Work Item #" + wi.Id, MessageBoxButtons.AbortRetryIgnore);
                        Utilities.OutputCommandString(ex.ToString());
                        if (result == DialogResult.Abort) return changedlinks;
                        else continue;
                    }
                }
            }

            return changedlinks;
        }

        public static bool ExecuteQueryLinkTypes(QueryDefinition qdef, string ProjectName, out int[] qdata)
        {
            Hashtable context = new Hashtable();
            StringBuilder strb = new StringBuilder();
            List<int> lqdata = new List<int>();

            context.Add("project", ProjectName); //@me, @today are filled automatically
            var query = new Query(Utilities.wistore, qdef.QueryText, context);

            if (query.IsLinkQuery)
            {
                foreach (var wilnk in query.RunLinkQuery())
                {
                    lqdata.Add(wilnk.TargetId);
                    lqdata.Add(wilnk.SourceId);

                    Utilities.OutputCommandString(string.Format("ParentID={0}, WorkItemID={1}", wilnk.SourceId, wilnk.TargetId));
                }
                lqdata = lqdata.Distinct().ToList();
            }
            else
            {
                foreach (WorkItem wi in query.RunQuery())
                {
                    lqdata.Add(wi.Id);

                    Utilities.OutputCommandString(string.Format("WorkItemID={0}, Title={1}", wi.Id, wi.Title));
                }
            }

            qdata = lqdata.ToArray();

            return false;
        }
    }
}
