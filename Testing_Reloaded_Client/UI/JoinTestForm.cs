using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedLibrary;
using SharedLibrary.Models;
using Testing_Reloaded_Client.Networking;

namespace Testing_Reloaded_Client.UI {
    public partial class JoinTestForm : Form {
        ServerManager serverManager = new ServerManager();
        private ReleaseChecker updater;

        public JoinTestForm() {
            InitializeComponent();
            updater = new ReleaseChecker("testing-reloaded-client");
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            Task.Run(new Action(async () => {
                var latestRelease = await updater.GetLatestRelease();

                if (latestRelease == null) return;

                var latestVersion = await updater.GetLatestVersion();

                if (latestVersion <= SharedLibrary.Statics.Constants.APPLICATION_VERSION) return;

                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"Nuova versione {latestVersion.ToString()} disponibile. Si prega di aggiornare",
                    "Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));

            }));
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
            Server server = null;

            if ((cmbServers.SelectedIndex == -1 && cmbServers.Text == "" )|| txtName.Text == "" || txtSurname.Text == "")
                return;

            if (IPAddress.TryParse(cmbServers.Text, out IPAddress address)) {
                server = new Server() {IP = address};
            }

            if (cmbServers.SelectedIndex != -1) {
                server = cmbServers.SelectedItem as Server;
            }

            var mainForm = new TestForm(server, new User(txtName.Text, txtSurname.Text, Environment.MachineName));
            mainForm.Show();
            mainForm.FormClosed += (o, args) => this.Close();
            this.Hide();

        }
    }
}