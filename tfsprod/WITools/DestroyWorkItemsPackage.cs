using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using tfsprod;

namespace TFSExt.DestroyWorkItems
{
    public static class DestroyWorkItemsPackage
    {
        public static void MenuItemCallback(object sender, EventArgs e)
        {
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                object _lockToken1 = new object();
                object _lockToken2 = new object();

                IWorkItemTrackingDocument doc2 = Utilities.docsrv2.FindDocument(Utilities.dte.ActiveDocument.FullName, _lockToken2);
                if (doc2 == null) return;

                var wicoll = new List<int>();

                foreach (int i in (doc2 as IResultsDocument).SelectedItemIds)
                {
                    IWorkItemDocument widoc = Utilities.docsrv2.GetWorkItem(Utilities.tfscoll, i, _lockToken1);
                    if (widoc == null) continue;
                    widoc.Load();

                    if (widoc.Item != null)
                    {
                        wicoll.Add(widoc.Item.Id);
                        Utilities.OutputCommandString("Preparing for destroy: " + widoc.Item.Id + ", " + widoc.Item.Title);
                    }
                    else
                        Utilities.OutputCommandString("WorkItem cannot be destroyed: " + i);

                    widoc.Release(_lockToken1);
                }
                doc2.Release(_lockToken2);

                foreach (var result in Utilities.wistore.DestroyWorkItems(wicoll))
                {
                    Utilities.OutputCommandString("Exception destroying Work Item #" + result.Id + " :" + result.Exception.Message);
                }

                Cursor.Current = origCursor;

                if (wicoll.Count > 0)
                    MessageBox.Show("Destroyed " + wicoll.Count + " Work Items", Utilities.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor.Current = origCursor;
                Utilities.OutputCommandString(ex.ToString());
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message, Utilities.AppTitle, MessageBoxButtons.OK);
            }
        }
    }
}
