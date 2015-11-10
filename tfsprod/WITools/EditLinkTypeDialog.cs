using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Linq;
using System.Windows.Forms;

namespace TFSExt.ChangeLinkTypes
{
    public partial class EditLinkTypeDialog : Form
    {
        public WorkItemLinkType fromlink;
        public WorkItemLinkType tolink;
        
        public EditLinkTypeDialog(WorkItemStore wistore)
        {
            InitializeComponent();

            this.comboBox1.Items.AddRange(wistore.WorkItemLinkTypes.ToArray());
            this.comboBox2.Items.AddRange(wistore.WorkItemLinkTypes.ToArray());

            this.comboBox1.ValueMember = "ReferenceName";
            this.comboBox2.ValueMember = "ReferenceName";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fromlink = this.comboBox1.SelectedItem as WorkItemLinkType;
            tolink = this.comboBox2.SelectedItem as WorkItemLinkType;

            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = (comboBox1.SelectedIndex != comboBox2.SelectedIndex && comboBox1.SelectedIndex > -1 && comboBox2.SelectedIndex > -1);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = (comboBox1.SelectedIndex != comboBox2.SelectedIndex && comboBox1.SelectedIndex > -1 && comboBox2.SelectedIndex > -1);
        }
    }
}
