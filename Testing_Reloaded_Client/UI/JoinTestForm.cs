using System;
using System.Windows.Forms;
using SharedLibrary.Models;
using Testing_Reloaded_Client.Networking;

namespace Testing_Reloaded_Client.UI {
    public partial class JoinTestForm : Form {
        ServerManager serverManager = new ServerManager();

        public JoinTestForm() {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
           
            UpdateServerList();
        }


        private void UpdateServerList() {
            btnRefresh.Enabled = false;

            serverManager.GetServers().ContinueWith(task => {
                cmbServers.Invoke(new Action(() => {
                    cmbServers.DataSource = null;
                    cmbServers.DisplayMember = "Hostname";
                    cmbServers.ValueMember = "IP";

                    cmbServers.DataSource = serverManager.Servers;
                    cmbServers.Enabled = true;
                    btnRefresh.Enabled = true;

                    if (cmbServers.Items.Count > 0)
                        cmbServers.SelectedIndex = 0;
                }));
            });
        }

        private void BtnUpdateServers_Click(object sender, EventArgs e) {
            UpdateServerList();
        }

        private void btnJoin_Click(object sender, EventArgs e) {
            if (cmbServers.SelectedIndex == -1 || txtName.Text == "" || txtSurname.Text == "")
                return;

            var mainForm = new TestForm(cmbServers.SelectedItem as Server, new User(txtName.Text, txtSurname.Text, Environment.MachineName));
            mainForm.Show();
            mainForm.FormClosed += (o, args) => this.Close();
            this.Hide();

        }
    }
}