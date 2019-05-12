using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Testing_Reloaded_Client {
    public partial class frmJoinTest : Form {
        ServerManager serverManager = new ServerManager();

        public frmJoinTest() {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            cmbServers.DisplayMember = "Hostname";
            cmbServers.ValueMember = "IP";

            UpdateServerList();
        }


        private void UpdateServerList() {
            serverManager.GetServers().ContinueWith(task => {
                cmbServers.Invoke(new Action(() => {
                    cmbServers.DataSource = serverManager.Servers;
                    cmbServers.Enabled = true;
                }));
            });
        }

        private void BtnUpdateServers_Click(object sender, EventArgs e) {
            UpdateServerList();
        }
    }
}