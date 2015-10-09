using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.VersionControl;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
using TFSExp.ExtendedMerge;

namespace tfsprod
{
    class Utilities
    {
        static internal IVsOutputWindow outputWindow;
        static internal IVsThreadedWaitDialogFactory dialogFactory;
        static internal string AppTitle;
        static internal WorkItemStore wistore;
        static internal ITeamExplorer teamExplorer;
        static internal DocumentService docsrv2;
        static internal TfsTeamProjectCollection tfscoll;
        static internal EnvDTE.DTE dte;
        static internal VersionControlExt vcext;
        static internal VersionControlServer vcsrv;
        static internal IVsDataConnectionDialogFactory dataconnfc;
        static internal ITeamFoundationContextManager vsTeamCtxMan;
        static internal MergeWIPane pane;

        /// <summary>
        /// Outputs the command string.
        /// Writes logs to OutputWindow pane, see <b>Microsoft.VisualStudio.Shell.Interop.SVsOutputWindow</b>
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        static internal void OutputCommandString(string text, string caption)
        {
            // --- Build the string to write on the debugger and Output window. 
            var outputText = new StringBuilder();
            outputText.AppendFormat("{0}: {1} ", caption, text);
            outputText.AppendLine();
            // --- Get a reference to IVsOutputWindow. 
            if (outputWindow == null) return;

            // --- Get the window pane for the general output. 
            var guidGeneral = Microsoft.VisualStudio.VSConstants.GUID_OutWindowDebugPane;
            IVsOutputWindowPane windowPane;

            if (Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.GetPane(ref guidGeneral, out windowPane)))
            {
                return;
            }
            // --- As the last step, write to the output window pane 
            windowPane.OutputString(outputText.ToString());
            windowPane.Activate();
        }

        static internal void OutputCommandString(string text)
        {
            OutputCommandString(text, AppTitle);
        }

        /// <summary>
        /// Creates the threaded wait dialog, see <b>Microsoft.VisualStudio.Shell.Interop.IVsThreadedWaitDialog2</b>
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="statustext">The statustext.</param>
        /// <param name="total">The total.</param>
        /// <returns>"Microsoft.VisualStudio.Shell.Interop.IVsThreadedWaitDialog2" instance</returns>
        static internal IVsThreadedWaitDialog2 CreateThreadedWaitDialog(string message, string progress, string statustext, int total)
        {
            IVsThreadedWaitDialog2 dlg = null;
            ErrorHandler.ThrowOnFailure(dialogFactory.CreateInstance(out dlg));

            ErrorHandler.ThrowOnFailure(
                dlg.StartWaitDialogWithPercentageProgress(AppTitle, message, progress, null, statustext, true, 0, total, 0));

            return dlg;
        }

        public static void ShowSQLConnectionDialog()
        {
            //var dlg = new DataConnectionDialog();
            //DataSource.AddStandardDataSources(dlg);
            //dlg.SelectedDataSource = DataSource.SqlDataSource;
            //if (DataConnectionDialog.Show(dlg) == System.Windows.Forms.DialogResult.OK)
            //    MessageBox.Show(dlg.ConnectionString);
            if (dataconnfc == null) return;
            
            var dlg = dataconnfc.CreateConnectionDialog();
            dlg.AddAllSources();
            dlg.SelectedSource = new Guid("{067ea0d9-ba62-43f7-9106-34930c60c528}");
            dlg.SelectedProvider = new Guid("{91510608-8809-4020-8897-fba057e22d54}");
            dlg.SafeConnectionString = (string)dte.Properties["TFS Productivity Tools", "General"].Item("TFSConnString").Value;

            if (dlg.ShowDialog())
            {
                dte.Properties["TFS Productivity Tools", "General"].Item("TFSConnString").Value = dlg.SafeConnectionString;
            }

            dlg.Dispose();
        }
    }

    public class DataConnectionDialogEditor : UITypeEditor
    {
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((context == null) || (provider == null) || (context.Instance == null))
            {
                return base.EditValue(context, provider, value);
            }

            /*EnvDTE.DTE dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            Guid objSIDGuid = typeof(IVsDataConnectionDialogFactory).GUID;
            Guid objIIDGuid = objSIDGuid;
            IntPtr objIntPtr;
            var objIServiceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte;
            int hr = objIServiceProvider.QueryService(ref objSIDGuid, ref objIIDGuid, out objIntPtr);
            var objService = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(objIntPtr) as IVsDataConnectionDialogFactory;
            //...
            System.Runtime.InteropServices.Marshal.Release(objIntPtr);
            */
            //IWindowsFormsEditorService wfes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            //if (wfes != null)
            //{
            var dataconnfc = provider.GetService(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
            var dlg = dataconnfc.CreateConnectionDialog();
            dlg.AddAllSources();
            dlg.SelectedSource = new Guid("{067ea0d9-ba62-43f7-9106-34930c60c528}");
            dlg.SelectedProvider = new Guid("{91510608-8809-4020-8897-fba057e22d54}");
            dlg.SafeConnectionString = (string)value;

            if (dlg.ShowDialog())
            {
                value = dlg.SafeConnectionString;
            }

            //var dlgres = wfes.ShowDialog(dlg);

            dlg.Dispose();
            //}
            return value;
        }
    }
}
