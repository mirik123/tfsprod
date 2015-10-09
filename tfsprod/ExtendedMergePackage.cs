using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using tfsprod;

namespace TFSExp.ExtendedMerge
{
    public static class ExtendedMergePackage
    {
        /// <summary>
        /// Menu callback event handler that activates the merge pane after selecting paths in Source Control Explorer.
        /// See <see cref="MergeWIControl"/>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void MySCQuBuildIDCallback(object sender, EventArgs e)
        {
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            IVsThreadedWaitDialog2 dlg = null;
            bool nolinks = true;
            bool bcanceled = false;
            int icanceled;

            try
            {
                if (Utilities.vcext.Explorer.Workspace.Folders.Length == 0)
                {
                    Cursor.Current = origCursor;
                    MessageBox.Show("The workspace is not mapped to any local folder.", Utilities.AppTitle);
                    return;
                }

                string folderPath = Utilities.vcext.Explorer.CurrentFolderItem.SourceServerPath;
                if (!MergeFactory.ServerItemExists(folderPath))
                {
                    Cursor.Current = origCursor;
                    MessageBox.Show("Target server path is cloacked or doesn't exist.", Utilities.AppTitle);
                    return;
                }

                if (Utilities.pane == null)
                {
                    Cursor.Current = origCursor;
                    MessageBox.Show("Failed to initialize " + Utilities.AppTitle + ": Pane was null.", Utilities.AppTitle);
                    return;
                }

                var mfrm = Utilities.pane.control;
                mfrm.SuppressEvents = true;
                mfrm.defServerName = folderPath;
                mfrm.Initialize();
                mfrm.ClearGrids();

                IVsWindowFrame frame = Utilities.pane.Frame as IVsWindowFrame;
                if (frame == null)
                {
                    Cursor.Current = origCursor;
                    MessageBox.Show("Failed to initialize " + Utilities.AppTitle + ": Frame was null.", Utilities.AppTitle);
                    return;
                }

                // Bring the tool window to the front and give it focus
                ErrorHandler.ThrowOnFailure(frame.Show());

                dlg = Utilities.CreateThreadedWaitDialog("Collecting information about changesets", "Starting to process changests...", "status", 100);
                dlg.UpdateProgress("Collecting information about changesets", "Starting to process changesets...", "status", 0, 100, true, out bcanceled);

                foreach (string sourcePath in Utilities.vcext.Explorer.SelectedItems.Select(x => x.SourceServerPath))
                {
                    int idx = 0;
                    foreach (Changeset ch in Utilities.vcsrv.QueryHistory(sourcePath, VersionSpec.Latest, 0, RecursionType.Full, null, null, null,
                                                                    Int32.MaxValue, true, true, false, false))
                    {
                        if (ch.Changes == null || ch.Changes.Length == 0) continue;

                        string change = ch.Changes.Take(5).Select(x => x.ChangeType.ToString()).Aggregate((x, y) => x + "," + y);
                        if (ch.Changes.Length > 5) change += "...";

                        string wrkitemstr = "";
                        int wrkitemid = 0;
                        if (ch.WorkItems != null && ch.WorkItems.Length > 0)
                        {
                            wrkitemstr = ch.WorkItems.Select(x => x.Id.ToString()).Aggregate((x, y) => x + "," + y);
                            wrkitemid = ch.WorkItems[0].Id;
                        }

                        ListViewItem itm = mfrm.AddNewGridItem(wrkitemstr, ch.ChangesetId.ToString(),
                                                ch.CreationDate.ToShortDateString(), ch.Owner, sourcePath, change, ch.Comment,
                                                new ListViewItemTag(ch.ChangesetId, wrkitemid, sourcePath, ch.CreationDate));

                        Utilities.OutputCommandString("Adding Changeset: " + itm.Name + " => " + sourcePath);
                        nolinks = false;

                        dlg.UpdateProgress("Collecting information about changesets", "Processing changeset: " + ch.ChangesetId, "status", idx++, 100, false, out bcanceled);
                        if (bcanceled)
                        {
                            mfrm.SortItems();
                            mfrm.SuppressEvents = false;
                            Cursor.Current = origCursor;
                            return;
                        }
                        if (idx == 100) idx = 0;
                    }
                }

                mfrm.SortItems();
                mfrm.UpdateRelatedBranchesCombo();
                mfrm.SuppressEvents = false;
                Cursor.Current = origCursor;
                dlg.EndWaitDialog(out icanceled);

                if (nolinks) MessageBox.Show("No Changesets where found in selected path.", Utilities.AppTitle);
            }
            catch (Exception ex)
            {
                Cursor.Current = origCursor;
                if(dlg != null) dlg.EndWaitDialog(out icanceled);
                Utilities.OutputCommandString(ex.ToString());
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message, Utilities.AppTitle, MessageBoxButtons.OK);
            }

            return;
        }

        /// <summary>
        /// Menu callback event handler that activates the merge pane after selecting work items in Work Item Query view.
        /// See <see cref="MergeWIControl"/>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void MyWIExpMergeIDCallback(object sender, EventArgs e)
        {
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            IVsThreadedWaitDialog2 dlg = null;
            int icanceled;

            try
            {

                if (Utilities.pane == null)
                {
                    Cursor.Current = origCursor;
                    MessageBox.Show("Failed to initialize " + Utilities.AppTitle + ": Pane was null.", Utilities.AppTitle);
                    return;
                }

                var mfrm = Utilities.pane.control;
                mfrm.SuppressEvents = true;
                mfrm.defServerName = "$/" + Utilities.vsTeamCtxMan.CurrentContext.TeamProjectName;
                mfrm.Initialize();
                mfrm.ClearGrids();

                IVsWindowFrame frame = Utilities.pane.Frame as IVsWindowFrame;
                if (frame == null)
                {
                    Cursor.Current = origCursor;
                    MessageBox.Show("Failed to initialize " + Utilities.AppTitle + ": Frame was null.", Utilities.AppTitle);
                    return;
                }

                // Bring the tool window to the front and give it focus
                ErrorHandler.ThrowOnFailure(frame.Show());

                int idx = 0;
                int iddx = 0;
                bool nolinks = true;
                bool bcanceled = false;
                
                object _lockToken1 = new object();
                object _lockToken2 = new object();
                string sourcePath;

                dlg = Utilities.CreateThreadedWaitDialog("Collecting information about changesets", "Starting to process changesets...", "status", 100);
                dlg.UpdateProgress("Collecting information about changesets", "Starting to process changesets...", "status", 0, 100, true, out bcanceled);

                IWorkItemTrackingDocument doc2 = Utilities.docsrv2.FindDocument(Utilities.dte.ActiveDocument.FullName, _lockToken2);
                if (doc2 == null)
                {
                    Cursor.Current = origCursor;
                    return;
                }

                foreach (int i in (doc2 as IResultsDocument).SelectedItemIds)
                {
                    IWorkItemDocument widoc = Utilities.docsrv2.GetWorkItem(Utilities.tfscoll, i, _lockToken1);
                    if (widoc == null) continue;

                    if (!widoc.IsLoaded) widoc.Load();
                    iddx = 0;
                    while (!widoc.IsLoaded && ++iddx < 10)
                    {
                        Thread.Sleep(1000);
                        if (widoc.ItemState == WorkItemState.Error)
                        {
                            Utilities.OutputCommandString(widoc.Exception != null ? widoc.Exception.ToString() : "WorkItem cannot be loaded " + i);
                            iddx = i;
                            break;
                        }
                    }
                    if (iddx == 10)
                    {
                        Utilities.OutputCommandString("WorkItem cannot be loaded " + i + ". Please open it manually and then try to load again.");
                        iddx = i;
                        break;
                    }
                    iddx = 0;

                    foreach (Link lnk in widoc.Item.Links.Cast<Link>().Where(x => x.ArtifactLinkType.Name == "Fixed in Changeset"))
                    {
                        Changeset ch = Utilities.vcsrv.ArtifactProvider.GetChangeset(new Uri((lnk as ExternalLink).LinkedArtifactUri));
                        if (ch.Changes == null || ch.Changes.Length == 0) continue;

                        string change = ch.Changes.Take(5).Select(x => x.ChangeType.ToString()).Aggregate((x, y) => x + "," + y);
                        if (ch.Changes.Length > 5) change += "...";

                        string[] relbranches = MergeFactory.QueryBranchObjectOwnership(ch.ChangesetId);
                        if (relbranches.Length == 0)
                        {
                            sourcePath = "$/" + Utilities.vsTeamCtxMan.CurrentContext.TeamProjectName;
                        }
                        else
                            sourcePath = relbranches[0];
                        //}

                        ListViewItem itm = mfrm.AddNewGridItem(widoc.Item.Id.ToString(), ch.ChangesetId.ToString(),
                                                ch.CreationDate.ToShortDateString(), ch.Owner, sourcePath, change, ch.Comment,
                                                new ListViewItemTag(ch.ChangesetId, widoc.Item.Id, sourcePath, ch.CreationDate));

                        Utilities.OutputCommandString("Adding Changeset: " + itm.Name + " => " + sourcePath);
                        nolinks = false;

                        dlg.UpdateProgress("Collecting information about changesets", "Processing changeset: " + ch.ChangesetId, "status", idx++, 100, false, out bcanceled);
                        if (bcanceled)
                        {
                            widoc.Release(_lockToken1);
                            doc2.Release(_lockToken2);
                            mfrm.SortItems();
                            mfrm.SuppressEvents = false;
                            Cursor.Current = origCursor;
                            return;
                        }
                        if (idx == 100) idx = 0;
                    }
                    widoc.Release(_lockToken1);
                }
                doc2.Release(_lockToken2);

                mfrm.SortItems();
                mfrm.UpdateRelatedBranchesCombo();
                mfrm.SuppressEvents = false;
                Cursor.Current = origCursor;
                dlg.EndWaitDialog(out icanceled);

                if (iddx > 0) MessageBox.Show("WorkItem cannot be loaded " + iddx + ". Please open it manually and then try to load again.", Utilities.AppTitle);
                else if (nolinks) MessageBox.Show("No linked Changesets where found in selected Work Items.", Utilities.AppTitle);
            }
            catch (Exception ex)
            {
                Cursor.Current = origCursor;
                if (dlg != null) dlg.EndWaitDialog(out icanceled);
                Utilities.OutputCommandString(ex.ToString());
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message, Utilities.AppTitle, MessageBoxButtons.OK);
            }

            return;
        }
    }
}
