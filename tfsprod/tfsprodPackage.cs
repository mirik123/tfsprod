using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation;
using Microsoft.VisualStudio.TeamFoundation.VersionControl;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using TFSExp.ExtendedMerge;

namespace tfsprod
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideLoadKey("Standard", "1.0", "TFS Productivity Tools - Change Link Types", "Mark Babayev (Experis)", 114)]
    [ProvideToolWindow(typeof(MergeWIPane), MultiInstances = false, Style = VsDockStyle.Tabbed, PositionX = 0, PositionY = 0, Width = 900, Height = 750, Transient = false,
        Orientation = ToolWindowOrientation.Left, Window = "9DDABE98-1D02-11D3-89A1-00C04F688DDE")]
    [ProvideOptionPage(typeof(OptionPageGrid), "TFS Productivity Tools", "General", 0, 0, true)]
    //[ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    [Guid(GuidList.guidtfsprodPkgString)]
    public sealed class tfsprodPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public tfsprodPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            InitializeVariables();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                var menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.cmdidQueryLinkTypes);
                var menuItem = new OleMenuCommand(TFSExt.ChangeLinkTypes.ChangeLinkTypesPackage.QueryLinkTypesCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.cmdidDestroyWI);
                menuItem = new OleMenuCommand(TFSExt.DestroyWorkItems.DestroyWorkItemsPackage.MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.cmdidExportWord);
                menuItem = new OleMenuCommand(TFSExt.ExportWordDoc.ExportWordDocPackage.TeamExpQueryCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.MyWIExpMergeID);
                menuItem = new OleMenuCommand(TFSExp.ExtendedMerge.ExtendedMergePackage.MyWIExpMergeIDCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.MySCQuMergeID);
                menuItem = new OleMenuCommand(TFSExp.ExtendedMerge.ExtendedMergePackage.MySCQuBuildIDCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.cmdidGetPreview);
                menuItem = new OleMenuCommand(TFSExt.GetPreview.GetPreviewPackage.MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.MySCQuModifyTimeID);
                menuItem = new OleMenuCommand(TFSExt.ModifyCheckinDate.ModifyCheckinDatePackage.MySCQuModifyTimeIDCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.MyHistModifyTimeID);
                menuItem = new OleMenuCommand(TFSExt.ModifyCheckinDate.ModifyCheckinDatePackage.MyHistModifyTimeCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.MySCHistModifyTimeID);
                menuItem = new OleMenuCommand(TFSExt.ModifyCheckinDate.ModifyCheckinDatePackage.MySCHistModifyTimeCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.MyHistLinkWI);
                menuItem = new OleMenuCommand(TFSExt.ChangeLinkTypes.ChangeLinkTypesPackage.LinkChangesetsToWICallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;

                menuCommandID = new CommandID(GuidList.guidtfsprodCmdSet, (int)PkgCmdIDList.cmdidCopyComment);
                menuItem = new OleMenuCommand(TFSExt.ChangeLinkTypes.ChangeLinkTypesPackage.CopyChangesetComments, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;   
            }
        }

        void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            string cmdName;
            var item = sender as OleMenuCommand;
            switch (item.CommandID.ID)
            {
                case (int)PkgCmdIDList.cmdidQueryLinkTypes: cmdName = "cmdidQueryLinkTypes"; break;
                case (int)PkgCmdIDList.cmdidExportWord: cmdName = "cmdidExportWord"; break;
                case (int)PkgCmdIDList.cmdidDestroyWI: cmdName = "cmdidDestroy"; break;
                case (int)PkgCmdIDList.MyWIExpMergeID: cmdName = "cmdidExpMerge"; break;
                case (int)PkgCmdIDList.MySCQuMergeID: cmdName = "cmdidExpMerge"; break;
                case (int)PkgCmdIDList.cmdidGetPreview: cmdName = "cmdidGetPreview"; break;
                case (int)PkgCmdIDList.MySCQuModifyTimeID: cmdName = "cmdidHistModifyTime"; break;
                case (int)PkgCmdIDList.MyHistModifyTimeID: cmdName = "cmdidHistModifyTime"; break;
                case (int)PkgCmdIDList.MySCHistModifyTimeID: cmdName = "cmdidHistModifyTime"; break;
                case (int)PkgCmdIDList.MyHistLinkWI: cmdName = "cmdidHistLinkWI"; break;
                case (int)PkgCmdIDList.cmdidCopyComment: cmdName = "cmdidCopyComment"; break;
                default: return;
            }

            item.Enabled = (bool)Utilities.dte.Properties["TFS Productivity Tools", "General"].Item(cmdName).Value;    
        }
        #endregion

        private void InitializeVariables()
        {
            var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            //IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));

            var TFSExt = dte.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt") as TeamFoundationServerExt;
            var vcext = dte.GetObject("Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExt") as VersionControlExt;

            var tfscoll = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TFSExt.ActiveProjectContext.DomainUri));
            tfscoll.Connect(Microsoft.TeamFoundation.Framework.Common.ConnectOptions.None);
            tfscoll.EnsureAuthenticated();

            var wistore = tfscoll.GetService<WorkItemStore>();
            var vcsrv = tfscoll.GetService<VersionControlServer>();

            //var vsTeamExp = GetService(typeof(IVsTeamExplorer)) as IVsTeamExplorer;
            var docsrv2 = GetService(typeof(DocumentService)) as DocumentService;
            var outputWindow = GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var dialogFactory = GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
            var dataconnfc = GetService(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
            var vsTeamCtxMan = GetService(typeof(ITeamFoundationContextManager)) as ITeamFoundationContextManager;
            var teamExplorer = GetService(typeof(ITeamExplorer)) as ITeamExplorer;

            MergeWIPane pane = (MergeWIPane)this.FindToolWindow(typeof(MergeWIPane), 0, true);

            Utilities.dialogFactory = dialogFactory;
            Utilities.outputWindow = outputWindow;
            Utilities.AppTitle = Resources.ResourceManager.GetString("AppTitle");
            Utilities.wistore = wistore;
            Utilities.teamExplorer = teamExplorer;
            Utilities.vsTeamCtxMan = vsTeamCtxMan;
            Utilities.docsrv2 = docsrv2;
            Utilities.tfscoll = tfscoll;
            Utilities.dte = dte;
            Utilities.vcext = vcext;
            Utilities.vcsrv = vcsrv;
            Utilities.dataconnfc = dataconnfc;
            Utilities.pane = pane;
        }
    }
}
