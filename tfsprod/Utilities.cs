using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.VersionControl;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TFSExp.ExtendedMerge;
using TFSExt.ShowRevHist;

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
        static internal MergeWIPane paneMerge;
        static internal RevisionHistoryPane paneRevhist;

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
            //    MessageBox.Show(dlg.ConnectionString, Utilities.AppTitle);
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

        /// <summary>
        /// Shows the server folder browser dialog, see <b>Microsoft.TeamFoundation.Build.Controls.VersionControlHelper.ShowServerFolderBrowser</b>.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="defaultServerPath">The default server path.</param>
        /// <param name="srvpath">The srvpath.</param>
        static internal void ShowServerFolderDlg(IWin32Window parent, string defaultServerPath, out string srvpath)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.Build.Controls.BuildPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.Build.Controls.VersionControlHelper");

            var parmod = new ParameterModifier(4);
            parmod[3] = true;
            MethodInfo methodInfo = type.GetMethod("ShowServerFolderBrowser",
                                                       new Type[] { typeof(IWin32Window), 
                                                            typeof(VersionControlServer), 
                                                            typeof(string), 
                                                            typeof(string).MakeByRefType() },
                                                        new ParameterModifier[] { parmod });

            object[] obj = new object[] { parent, vcsrv, defaultServerPath, null };
            DialogResult res = (DialogResult)methodInfo.Invoke(null, obj);

            srvpath = (res == DialogResult.OK) ? (string)obj[3] : defaultServerPath;
        }

        /// <summary>
        /// Shows the work item details dialog, see <b>Microsoft.VisualStudio.TeamFoundation.WorkItemTracking.DocumentService.ShowWorkItem</b>.
        /// </summary>
        /// <param name="wi">The wi.</param>
        static internal void ShowWorkItemDetailsDlg(int wi)
        {
            object _lockToken1 = new object();

            IWorkItemDocument widoc = docsrv2.GetWorkItem(tfscoll, wi, _lockToken1);
            docsrv2.ShowWorkItem(widoc);

            widoc.Release(_lockToken1);
        }

        /// <summary>
        /// Shows the resolve conflicts dialog, see <b>Microsoft.TeamFoundation.VersionControl.CommandLine.CommandResolve.DisplayResolveDialog</b>.
        /// </summary>
        static internal void ShowResolveConflictsDlg()
        {
            Workspace wrkspc = vcext.Explorer.Workspace;

            try
            {
                Assembly assres = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.ControlAddItemsExclude));
                var dlgResolveConflicts = assres.GetTypes().FirstOrDefault(x => x.Name == "DialogResolveConflicts");
                dlgResolveConflicts.InvokeMember("ResolveConflicts", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, new object[] { wrkspc, null, true, null });
            }
            catch (Exception ex)
            {
                OutputCommandString(ex.ToString());
                MessageBox.Show(ex.Message, Utilities.AppTitle);
            }

            /*var IDEPath = (string)dte.Properties["TFS Productivity Tools", "General"].Item("IDEPath").Value;
            if (string.IsNullOrEmpty(IDEPath))
            {
                OutputCommandString("Path to TF.exe is undefined");
                MessageBox.Show("Path to TF.exe is undefined in TOOLS->OPTIONS->TFS Productivity Tools->IDEPath ", Utilities.AppTitle);
            }
            else if (!Directory.Exists(IDEPath))
            {
                OutputCommandString("Path to TF.exe is invalid in TOOLS->OPTIONS->TFS Productivity Tools->IDEPath");
            }
            else
            {
                try
                {
                    Assembly asm = Assembly.LoadFrom(IDEPath + "\\TF.exe");
                    Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.CommandLine.CommandResolve");

                    var objArguments = new Microsoft.TeamFoundation.Client.CommandLine.Arguments("resolve");
                    objArguments.FreeArguments.Add(wrkspc.Folders[0].LocalItem);

                    object res = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] { objArguments });

                    type.InvokeMember("DisplayResolveDialog", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, res, new object[] { wrkspc, new string[] { wrkspc.Folders[0].LocalItem } });
                }
                catch (Exception ex)
                {
                    OutputCommandString(ex.ToString());
                    MessageBox.Show(ex.ToString(), Utilities.AppTitle);
                }
            }*/
        }

        /// <summary>
        /// Shows the changeset details dilaog, see <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExplorerExt.ViewChangesetDetails</b>.
        /// </summary>
        /// <param name="ch">The ch.</param>
        static internal void ShowChangesetDetailsDlg(int ch)
        {
            vcext.ViewChangesetDetails(ch);
        }

        /// <summary>
        /// Navigates to the server path in Source Control Explorer, see <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExplorerExt.Navigate</b> and <b>Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExplorerExt.VsWindowFrame.Show</b>.
        /// </summary>
        /// <param name="srvpath">The srvpath.</param>
        static internal void SelectVersionControlServerPath(string srvpath)
        {
            vcext.Explorer.Navigate(srvpath);
            vcext.Explorer.VsWindowFrame.Show();
        }

        static internal List<WorkItem> ShowWorkItemPickerDialog()
        {
            try
            {
                Assembly asm = Assembly.GetAssembly(typeof(IWorkItemControlHost));
                Type type = asm.GetType("Microsoft.TeamFoundation.WorkItemTracking.Controls.WorkItemPickerDialog");
                var inst = type.GetConstructor(new[] { typeof(WorkItemStore) }).Invoke(new[] { wistore });
                type.GetProperty("PortfolioDisplayName").SetValue(inst, vsTeamCtxMan.CurrentContext.TeamProjectName);
                
                var res = (DialogResult)type.GetMethod("ShowDialog", new Type[0]).Invoke(inst, null);
                if (res == DialogResult.OK)
                {
                    var selworkitems = (List<WorkItem>)type.GetMethod("SelectedWorkItems", new Type[0]).Invoke(inst, null);
                    return selworkitems;
                }
                
                return null;

                //var dlg = new LinkDialogRelatedLinkControl();
                //dlg.Show();
            }
            catch (Exception ex)
            {
                OutputCommandString(ex.ToString());
                MessageBox.Show(ex.Message, Utilities.AppTitle);
                return null;
            }
        }

        #region ProgressMerge Dialog
        /// <summary>
        /// Creates the progress merge dialog, see <b>Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge</b>.
        /// This feature is not implemented.
        /// </summary>
        /// <param name="vcsrv">The VersionControlServer.</param>
        /// <returns></returns>
        static internal object CreateProgressMerge(VersionControlServer vcsrv)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.WorkItemPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge");

            object res = type.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance, null, null, new object[] { vcsrv });
            return res;
        }

        /// <summary>
        /// Shows the progress merge dialog.
        /// This feature is not implemented.
        /// </summary>
        /// <param name="prgmerge">The prgmerge.</param>
        /// <param name="owner">The owner.</param>
        static internal void ShowProgressMerge(object prgmerge, IWin32Window owner)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.WorkItemPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge");

            type.InvokeMember("Show", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, prgmerge, new object[] { owner });
        }

        /// <summary>
        /// Closes the progress merge dialog.
        /// This feature is not implemented.
        /// </summary>
        /// <param name="prgmerge">The prgmerge.</param>
        static internal void CloseProgressMerge(object prgmerge)
        {
            Assembly asm = Assembly.GetAssembly(typeof(Microsoft.TeamFoundation.VersionControl.Controls.WorkItemPolicy));
            Type type = asm.GetType("Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge");

            type.InvokeMember("Close", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, prgmerge, null);
        }

        #endregion
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
