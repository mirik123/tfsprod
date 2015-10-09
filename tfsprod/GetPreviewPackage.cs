using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using tfsprod;

namespace TFSExt.GetPreview
{    
    public static class GetPreviewPackage
    {
        public static string lastFolderDisplayed;
        
        public static void MenuItemCallback(object sender, EventArgs e)
        {                        
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            lastFolderDisplayed = null;

            Workspace myWorkspace = Utilities.vcext.Explorer.Workspace;
            Utilities.vcsrv.Getting += new GettingEventHandler(vcServer_Getting);

            foreach (string sourcePath in Utilities.vcext.Explorer.SelectedItems.Select(x => x.SourceServerPath))
            {
                Utilities.OutputCommandString("Previewing GET command for path: " + sourcePath);
                var getreq = new GetRequest(new ItemSpec(sourcePath, RecursionType.Full), VersionSpec.Latest);
                var status = myWorkspace.Get(getreq, GetOptions.Preview);

                Utilities.OutputCommandString("HaveResolvableWarnings=" + status.HaveResolvableWarnings);
                Utilities.OutputCommandString("NumConflicts=" + status.NumConflicts);
                Utilities.OutputCommandString("NumFailures=" + status.NumFailures);
                Utilities.OutputCommandString("NumOperations=" + status.NumOperations);
                Utilities.OutputCommandString("NumUpdated=" + status.NumUpdated);
                Utilities.OutputCommandString("NumWarnings=" + status.NumWarnings);
            }

            Utilities.vcsrv.Getting -= new GettingEventHandler(vcServer_Getting);

            Cursor.Current = origCursor;
        }

        public static void vcServer_Getting(object sender, GettingEventArgs e)
        {
            string fileName = null;
            if (e.TargetLocalItem != null)
            {
                string dirName = Path.GetDirectoryName(e.TargetLocalItem);
                fileName = Path.GetFileName(e.TargetLocalItem);

                if (e.TargetLocalItem == dirName)
                {
                    fileName = e.TargetLocalItem;
                }
                else
                {
                    if (lastFolderDisplayed != dirName)
                    {
                        if (lastFolderDisplayed != null)
                        {
                            Utilities.OutputCommandString("");
                        }
                        Utilities.OutputCommandString(dirName);
                        lastFolderDisplayed = dirName;
                    }
                }
            }

            string str;
            string message = e.GetMessage(fileName, out str);
            switch (e.Status)
            {
                case OperationStatus.Conflict:
                case OperationStatus.SourceWritable:
                case OperationStatus.TargetLocalPending:
                case OperationStatus.TargetWritable:
                case OperationStatus.SourceDirectoryNotEmpty:
                case OperationStatus.TargetIsDirectory:
                case OperationStatus.UnableToRefresh:
                    Utilities.OutputCommandString("Warning: " + str);
                    return;

                case OperationStatus.Getting:
                case OperationStatus.Replacing:
                case OperationStatus.Deleting:
                    Utilities.OutputCommandString(message);
                    return;
            }
        }
    }
}
