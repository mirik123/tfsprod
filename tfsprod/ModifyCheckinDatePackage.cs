using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using tfsprod;

namespace TFSExt.ModifyCheckinDate
{    
    public static class ModifyCheckinDatePackage
    {
        /// <summary>
        /// Menu callback event handler that updates files date/time attribute in local workspace to their latest check-in date/time.
        /// See <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlHistoryExt</b>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void MySCHistModifyTimeCallback(object sender, EventArgs e)
        {
            //MessageBox.Show("Update Modification Time");
            Workspace wrkspc = Utilities.vcext.Explorer.Workspace;
            int[] chids = Utilities.vcext.History.ActiveWindow.SelectedChangesets.Select(x => x.ChangesetId).ToArray();

            foreach (int chid in chids)
            {
                Changeset ch = Utilities.vcsrv.GetChangeset(chid);

                foreach (var itm in ch.Changes.Select(x => x.Item))
                {
                    if (itm.DeletionId > 0 || itm.ItemType != ItemType.File) continue;

                    string localitm = wrkspc.GetLocalItemForServerItem(itm.ServerItem, true);
                    if (!File.Exists(localitm)) continue;

                    File.SetAttributes(localitm, File.GetAttributes(localitm) & ~FileAttributes.ReadOnly);
                    File.SetLastWriteTime(localitm, itm.CheckinDate);

                    Utilities.OutputCommandString("Set modification date for file: " + localitm + " to " + itm.CheckinDate.ToShortDateString());
                }
            }


            return;
        }

        /// <summary>
        /// Menu callback event handler that updates files date/time attribute in local workspace to their latest check-in date/time.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void MySCQuModifyTimeIDCallback(object sender, EventArgs e)
        {
            Workspace wrkspc = Utilities.vcext.Explorer.Workspace;

            foreach (string serverPath in Utilities.vcext.Explorer.SelectedItems.Select(x => x.SourceServerPath))
            {
                foreach (Item itm in Utilities.vcsrv.GetItems(serverPath, RecursionType.Full).Items)
                {
                    if (itm.DeletionId > 0 || itm.ItemType != ItemType.File) continue;

                    string localitm = wrkspc.GetLocalItemForServerItem(itm.ServerItem, true);
                    if (!File.Exists(localitm)) continue;

                    File.SetAttributes(localitm, File.GetAttributes(localitm) & ~FileAttributes.ReadOnly);
                    File.SetLastWriteTime(localitm, itm.CheckinDate);

                    Utilities.OutputCommandString("Set modification date for file: " + localitm + " to " + itm.CheckinDate.ToShortDateString());
                }
            }
        }

        /// <summary>
        /// Menu callback event handler that updates Changesets check-in date/time attribute in TFS database.
        /// See <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlHistoryExt</b>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void MyHistModifyTimeCallback(object sender, EventArgs e)
        {
            //MessageBox.Show("Update Changeset Time");
            int[] chids = Utilities.vcext.History.ActiveWindow.SelectedChangesets.Select(x => x.ChangesetId).ToArray();
            if (chids == null || chids.Length == 0) return;

            string datevalue = Microsoft.VisualBasic.Interaction.InputBox("Enter new value for Source Path", Utilities.AppTitle, DateTime.Now.ToString("G"));
            if (string.IsNullOrWhiteSpace(datevalue)) return;

            DateTime dt;

            try
            {
                dt = DateTime.Parse(datevalue);
            }
            catch
            {
                MessageBox.Show("Invalid Date/Time specified.");
                return;
            }

            //connect to TFS database
            //using [DefaultCollection]
            //update table: UPDATE tbl_Changeset SET CreationDate=’?’ WHERE ChangeSetId=‘?’

            var connstr = (string)Utilities.dte.Properties["TFS Productivity Tools", "General"].Item("TFSConnString").Value;
            if (string.IsNullOrEmpty(connstr))
            {
                Utilities.OutputCommandString("Connection string to TFS database is empty. Set it in Visual Studio Tools -> Options -> TFS Productivity Tools");
            }
            else
            {
                
                var sqlconn = new SqlConnection(connstr);
                try
                {
                    sqlconn.Open();
                    var sqlcmd = sqlconn.CreateCommand();
                    sqlcmd.CommandText = "UPDATE tbl_Changeset SET CreationDate=@date WHERE ChangeSetId in (" + chids.Aggregate("0", (x, y) => x + "," + y) + ")";
                    sqlcmd.Parameters.AddWithValue("@date", dt);
                    int res = sqlcmd.ExecuteNonQuery();
                    Utilities.OutputCommandString("Set Check-in date for Changesets: " + chids.Aggregate("", (x, y) => x + "," + y) + " to " + dt.ToShortDateString() + ", affected records: " + res);
                }
                catch(Exception ex)
                {
                    Utilities.OutputCommandString("Set Check-in date for Changeset: " + chids + " failed. Error: " + ex.ToString());
                    MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message, Utilities.AppTitle, MessageBoxButtons.OK);
                }
                finally
                {
                    sqlconn.Close();
                }
            }
            
            return;
        }        
    }
}
