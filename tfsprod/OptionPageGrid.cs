using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using tfsprod;

namespace TFSExp.ExtendedMerge
{

    /// <summary>
    /// Extensions DialogPage
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class OptionPageGrid : DialogPage
    {
        //private string pIDEPath = "";
        private string connString = "";

        [Category("Update Changeset Time")]
        [DisplayName("TFS Connection String")]
        [Description("Set connection string to TFS database")]
        [Editor(typeof(DataConnectionDialogEditor), typeof(UITypeEditor))]
        public string TFSConnString
        {
            get { return connString; }
            set { connString = value; }
        }

        [Category("Enable Extensions")]
        [DisplayName("Change Work Item link types")]
        public bool cmdidQueryLinkTypes { get; set;}

        [Category("Enable Extensions")]
        [DisplayName("Export to Microsoft Word")]
        public bool cmdidExportWord { get; set;}

        [Category("Enable Extensions")]
        [DisplayName("Destroy Work Items")]
        public bool cmdidDestroy { get; set;}

        [Category("Enable Extensions")]
        [DisplayName("Merge Work Items")]
        public bool cmdidExpMerge { get; set;}

        [Category("Enable Extensions")]
        [DisplayName("Emulate Get-Preview")]
        public bool cmdidGetPreview { get; set;}

        [Category("Enable Extensions")]
        [DisplayName("Update Changeset Time")]
        public bool cmdidHistModifyTime { get; set;}

        [Category("Enable Extensions")]
        [DisplayName("Link Changesets to Work Item")]
        public bool cmdidHistLinkWI { get; set; }

        /**[Category("IDE Settings")]
         [DisplayName("Set Visual Studio IDE Path")]
         [Description("Visual Studio IDE Path")]
         [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
         public string IDEPath
         {
             get { return pIDEPath; }
             set
             {
                 if (!System.IO.File.Exists(value + "\\tf.exe"))
                 {
                     MessageBox.Show("Path to Visual Studio IDE folder is incorrect.", Utilities.AppTitle);
                     pIDEPath = "";
                 }
                 else
                 {
                     pIDEPath = value;
                 }
             }
         */

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionPageGrid"/> class.
        /// </summary>
        public OptionPageGrid()
            : base()
        {
           //string dtpath = Assembly.GetAssembly(typeof(Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExt)).Location;
           //dtpath = dtpath.Substring(0,dtpath.IndexOf("\\IDE\\")+5);
            //if (File.Exists(dtpath + "\\tf.exe")) 
           //pIDEPath = dtpath;
           //

            cmdidDestroy = cmdidExpMerge = cmdidExportWord = cmdidGetPreview = true;
            cmdidHistLinkWI = cmdidHistModifyTime = cmdidQueryLinkTypes = true;
        }
    }
}
