using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using tfsprod;

namespace TFSExp.ExtendedMerge
{
    /// <summary>
    /// This class inherites form ToolWindowPane and adds to it <see cref="MergeWIControl"/>.
    /// </summary>
    [Guid("EFD47DA9-9346-4af5-84F9-EADB588FF4FD")]
    public class MergeWIPane : ToolWindowPane
    {
        // Control that will be hosted in the tool window
        private MergeWIControl _control = null;

        // Caching our output window pane
        //private IVsOutputWindowPane outputWindowPane = null;

        /// <summary>
        /// Constructor for ToolWindowPane.
        /// Initialization that depends on the package or that requires access
        /// to VS services should be done in OnToolWindowCreated.
        /// </summary>
        public MergeWIPane()
            : base(null)
        {
            Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Entering constructor for class {0}.", this.GetType().Name));

            // Creating the user control that will be displayed in the window
            _control = new MergeWIControl();
        }

        /// <summary>
        /// This property returns the handle to the user control that should
        /// be hosted in the Tool Window.
        /// </summary>
        public override IWin32Window Window
        {
            get { return (IWin32Window)_control; }
        }

        /// <summary>
        /// This is called after our control has been created and sited.
        /// This is a good place to initialize the control with data gathered
        /// from Visual Studio services.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            this.Caption = Utilities.AppTitle;
            this.BitmapResourceID = 700;
            this.BitmapIndex = 1;
            ((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_BitmapResource, 700);
            ((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_BitmapIndex, 1);
            ((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);


            // Display the pane as the first tab inside the VS IDE.
            //int result = ((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_MdiChild);
            //if (result != VSConstants.S_OK)
            //{
            //MessageBox.Show("Failed to dock Work Item Merger under Visual Studio IDE window.", "Work Item Merger");
            //}
        }

        /// <summary>
        /// Called when pane is closed.
        /// </summary>
        protected override void OnClose()
        {
            base.OnClose();

            _control.Dispose();
            _control = null;
        }

        /// <summary>
        /// Gets or sets the UserControl object.
        /// </summary>
        /// <value>
        /// The <see cref="MergeWIControl"/> control object.
        /// </value>
        public MergeWIControl control
        {
            get
            {
                return _control;
            }
            set
            {
                _control = value;
            }
        }
    }
}
