using System;
using System.Windows.Forms;
using SharedLibrary.Models;
using Testing_Reloaded_Server.Models;
using System.IO;
using System.Threading.Tasks;
using SharedLibrary;
using Testing_Reloaded_Server.Networking;

namespace Testing_Reloaded_Server.UI {
    public partial class StartTestForm : Form {
        private TestManager testManager;
        private ReleaseChecker updater = new ReleaseChecker("testing-reloaded-server");

        public StartTestForm() {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            Task.Run(new Action(async () => {
                var latestRelease = await updater.GetLatestRelease();

                if (latestRelease == null) return;

                var latestVersion = await updater.GetLatestVersion();

                if (latestVersion <= SharedLibrary.Statics.Constants.APPLICATION_VERSION) return;

                this.Invoke(new Action(() => {
                    MessageBox.Show($"Nuova versione {latestVersion.ToString()} disponibile. Si prega di aggiornare",
                        "Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));

            }));
        }

        private void BtnStartTest_Click(object sender, EventArgs e) {
            if (txtDocsDir.Text != "" && !Directory.Exists(txtDocsDir.Text)) {
                MessageBox.Show(
                    "La directory della documentazione non esiste, controlla il percorso. Se non vuoi fornire documentazione puoi lasciarlo vuoto",
                    "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            var test = new ServerTest() {
                ClientTestPath = txtDataDownloadPath.Text,
                DeleteFilesAfterEnd = chbDelete.Checked,
                ReclaimTestImmediately = chbRitira.Checked,
                TestName = txtTestName.Text,
                Time = TimeSpan.Parse(chbTime.Text),
                State = Test.TestState.NotStarted,
                DocumentationDirectory = txtDocsDir.Text,
                HandoverDirectory = txtConsegneDir.Text
            };

            testManager = new TestManager(test);

            var testForm = new TestForm(testManager);
            testForm.Closed += (o, args) => this.Close();

            testForm.Show();
            this.Hide();
        }

        private void ChbDelete_CheckedChanged(object sender, EventArgs e) {
            if (chbDelete.Checked)
                MessageBox.Show(
                    "Visto che il programma è ancora in beta consiglio di non cambiare questa impostazione.",
                    "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}