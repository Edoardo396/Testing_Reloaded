using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedLibrary;

namespace Testing_Reloaded_Server {
    public partial class TestForm : Form {
        private TestManager testManager;

        public TestForm(TestManager manager) {
            InitializeComponent();
            this.testManager = manager;
        }

        protected override async void OnLoad(EventArgs e) {
            base.OnLoad(e);

            lvClients.View = View.Details;

            int width = lvClients.Width / 5;

            lvClients.Columns.Add(new ColumnHeader("clmId") { Text = "ClientID", Width = width });
            lvClients.Columns.Add(new ColumnHeader("clmName") {Text = "Nome", Width = width});
            lvClients.Columns.Add(new ColumnHeader("clmPC") {Text = "Computer", Width = width});
            lvClients.Columns.Add(new ColumnHeader("clmTime") {Text = "Tempo", Width = width});
            lvClients.Columns.Add(new ColumnHeader("clmState") {Text = "Stato", Width = width});

            this.testManager.ClientStatusUpdated += TestManagerOnClientStatusUpdated;

            await testManager.StartTest();
        }

        // run as Client's thread
        private void TestManagerOnClientStatusUpdated(Client c) {
            lvClients.Invoke(new Action(() => {
                lvClients.Items.Clear();
                foreach (Client client in testManager.ConnectedClients) {
                    var item = new ListViewItem(client.Id.ToString());

                    string state = client.TestState?.State.ToString();
                    string rtime = client.TestState?.RemainingTime.ToString();

                    item.SubItems.Add(client.ToString());
                    item.SubItems.Add(client.PCHostname);
                    item.SubItems.Add(rtime ?? "N/A");
                    item.SubItems.Add(state ?? "N/A");

                    if (client.TestState != null)
                        item.BackColor = client.TestState.State.UserStateToColor();
                    lvClients.Items.Add(item);
                    //  lvClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }));
        }

        private void LvClients_SelectedIndexChanged(object sender, EventArgs e) {
            if (lvClients.SelectedIndices.Count == 0) {
                grpClientControls.Enabled = false;
                return;
            }

            grpClientControls.Enabled = true;
            lblSelectedClient.Text = lvClients.SelectedItems[0].SubItems[1].Text;
        }
    }
}