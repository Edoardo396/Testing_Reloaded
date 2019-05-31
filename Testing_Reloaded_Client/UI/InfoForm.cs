using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Testing_Reloaded_Client.UI
{
    public partial class InfoForm : Form
    {
        public InfoForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            lblVersion.Text += SharedLibrary.Statics.Constants.APPLICATION_VERSION.ToString();
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("https://github.com/edofullin/Testing_Reloaded");
        }
    }
}
