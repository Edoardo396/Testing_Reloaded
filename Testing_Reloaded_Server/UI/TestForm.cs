using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharedLibrary.Models;
using SharedLibrary.Statics;
using Testing_Reloaded_Server.Models;
using Testing_Reloaded_Server.Networking;

namespace Testing_Reloaded_Server.UI {
    public partial class TestForm : Form {
        private ServerPublishingManager publishingManager;
        private TestManager testManager;

        private int? selectedId = null;

        public TestForm(TestManager manager) {
            InitializeComponent();
            this.testManager = manager;
        }

        protected override async void OnLoad(EventArgs e) {
            base.OnLoad(e);

            publishingManager = new ServerPublishingManager(testManager.CurrentTest) {AllowClientsOnHold = true};

            lvClients.View = View.Details;

            int width = lvClients.Width / 5;

            lvClients.Columns.Add(new ColumnHeader("clmId") {Text = "ClientID", Width = width});
            lvClients.Columns.Add(new ColumnHeader("clmName") {Text = "Nome", Width = width});
            lvClients.Columns.Add(new ColumnHeader("clmPC") {Text = "Computer", Width = width});
            lvClients.Columns.Add(new ColumnHeader("clmTime") {Text = "Tempo", Width = width});
            lvClients.Columns.Add(new ColumnHeader("clmState") {Text = "Stato", Width = width});

            this.testManager.ClientStatusUpdated += TestManagerOnClientStatusUpdated;
            grpClientControls.Enabled = false;
        }

        // run as Client's thread
        private void TestManagerOnClientStatusUpdated(Client c) {
            lvClients.Invoke(new Action(() => {
                if (!lvClients.Items.ContainsKey(c.Id.ToString()))
                    ListViewAddClient(c);

                foreach (ListViewItem lvClient in lvClients.Items)
                    if (lvClient.Name == c.Id.ToString()) {

                        lvClient.SubItems[1].Text = c.ToString();
                        lvClient.SubItems[2].Text = c.PCHostname;
                        lvClient.SubItems[3].Text = c.TestState?.RemainingTime.ToString() ?? "N/A";
                        lvClient.SubItems[4].Text = c.TestState?.State.ToString() ?? "N/A";

                        if (c.TestState != null)
                            lvClient.BackColor = c.TestState.State.UserStateToColor();
                    }
            }));
        }

        private void ReloadClientsList() {
            lvClients.Items.Clear();
            foreach (Client client in testManager.ConnectedClients) {
                ListViewAddClient(client);
            }
        }

        private void ListViewAddClient(Client client) {
            var item = new ListViewItem(client.Id.ToString());

            item.Name = client.Id.ToString();

            string state = client.TestState?.State.ToString();
            string rtime = client.TestState?.RemainingTime.ToString();

            item.SubItems.Add(client.ToString());
            item.SubItems.Add(client.PCHostname);
            item.SubItems.Add(rtime ?? "N/A");
            item.SubItems.Add(state ?? "N/A");

            if (client.TestState != null)
                item.BackColor = client.TestState.State.UserStateToColor();
            lvClients.Items.Add(item);
        }

        private void LvClients_SelectedIndexChanged(object sender, EventArgs e) {
                if (lvClients.SelectedIndices.Count == 0) {
                    grpClientControls.Enabled = false;
                    return;
                }

                grpClientControls.Enabled = true;
                lblSelectedClient.Text = lvClients.SelectedItems[0].SubItems[1].Text;
            }

            private async void BtnTestStart_Click(object sender, EventArgs e) {
                try {
                    await testManager.StartTest();
                    btnTestStart.Enabled = false;
                    MessageBox.Show("Test avviato", "Test Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } catch (Exception ex) {
                    MessageBox.Show("Error sending command, error: " + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            private async void Button4_Click(object sender, EventArgs e) {
                try {
                    if (testManager.CurrentTest.State == Test.TestState.Started) {
                        await testManager.SetTestState(Test.TestState.OnHold);
                        btnTestPause.Text = "Riavvia Test";
                    } else if (testManager.CurrentTest.State == Test.TestState.OnHold) {
                        await testManager.SetTestState(Test.TestState.Started);
                        btnTestPause.Text = "Pausa Test";
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Error sending command, error: " + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            private async void BtnTimeAdd_Click(object sender, EventArgs e) {
                try {
                    if (!TimeSpan.TryParse(txtTime.Text, out TimeSpan times)) return;
                    await testManager.AddTime(times,
                        c => lvClients.SelectedItems.ContainsKey(c.Id.ToString()));
                } catch (Exception ex) {
                    MessageBox.Show("Error sending command, error: " + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            private async void BtnToggleStateClient_Click(object sender, EventArgs e) {
                try {
                    await testManager.ToggleStateForClients(c => lvClients.SelectedItems.ContainsKey(c.Id.ToString()));
                    btnToggleStateClient.Text = btnToggleStateClient.Text == "Pausa" ? "Riavvia" : "Pausa";
                } catch (Exception ex) {
                    MessageBox.Show("Error sending command, error: " + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

        private async void BtnForceHandover_Click(object sender, EventArgs e) {
            try {
                await testManager.ForceHandover(c => lvClients.SelectedItems.ContainsKey(c.Id.ToString()));
                btnToggleStateClient.Text = btnToggleStateClient.Text == "Pausa" ? "Riavvia" : "Pausa";
            } catch (Exception ex) {
                MessageBox.Show("Error sending command, error: " + ex.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        } 
    }
    }