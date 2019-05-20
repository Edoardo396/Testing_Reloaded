using System;
using System.Windows.Forms;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Statics;
using Testing_Reloaded_Client.Networking;

namespace Testing_Reloaded_Client.UI {
    public partial class TestForm : Form {
        private TestManager testManager;


        public TestForm(Server selectedServer, User me) {
            InitializeComponent();

            testManager = new TestManager(selectedServer, me);
            testManager.ReloadUI += ReloadUi;
        }

        private void ReloadUi() {
            this.Invoke(new Action(() => {
                lblTestName.Text = testManager.CurrentTest.TestName;
                lblTestDuration.Text = testManager.CurrentTest.Time.ToString();
                lblRemainingTime.Text = testManager.CurrentTest.Time.ToString();

                if (testManager.CurrentTest.State != Test.TestState.NotStarted)
                    lblTestDir.Text = testManager.ResolvedTestPath;

                lblStatus.Text = testManager.TestState.State.ToString();
                lblStatus.BackColor = testManager.TestState.State.UserStateToColor();

                btnConsegna.Enabled = testManager.TestState.State == UserTestState.UserState.Testing;
            }));
        }


        protected override async void OnLoad(EventArgs e) {
            base.OnLoad(e);
            progressBar1.Enabled = true;
            progressBar1.Style = ProgressBarStyle.Marquee;

            lblCurrentOperation.Text = "Connessione";
            await testManager.Connect();

            lblCurrentOperation.Text = "Download Dati Test";

            await testManager.DownloadTestData();
            ReloadUi();

            lblTestDir.Text = "Attendo inizio del test";
            lblCurrentOperation.Text = "Attendo Inizio";

            await testManager.WaitForTestStart();
            ReloadUi();

            lblCurrentOperation.Text = "Download documentazione";
            await testManager.DownloadTestDocumentation();

            lblCurrentOperation.Visible = false;
            progressBar1.Visible = false;

            string message =
                $"Il test è iniziato.\r\nLa cartella del test è {testManager.ResolvedTestPath} puoi trovare la documentazione del test nella sottocartella Documentation se è disponibile. Quando consegnerai veraano inviati tutti i file che si trovano nella cartella del test. in bocca al lupo!";

            MessageBox.Show(message, "Test iniziato", MessageBoxButtons.OK, MessageBoxIcon.Information);

            System.Diagnostics.Process.Start(testManager.ResolvedTestPath);


            testManager.TestState.State = UserTestState.UserState.Testing;
            await testManager.SendStateUpdate();

            testTimer.Start();
            ReloadUi();
        }

        private async void TestTimer_Tick(object sender, EventArgs e) {
            testManager.TimeElapsed((uint) (testTimer.Interval / 1000));
            lblRemainingTime.Text = testManager.TestState.RemainingTime.ToString();

            TimeSpan remainingTime = testManager.TestState.RemainingTime;

            if (Math.Abs(remainingTime.TotalSeconds) < 1) {
                MessageBox.Show(
                    $"Il tempo è scaduto, hai un minuto per salvare e chiudere tutti i programmi che stanno usando la cartella {testManager.ResolvedTestPath}, dopodichè il file verrà inviato al server e tutte le modifiche andranno inevitabilmente perse",
                    "Tempo Scaduto", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (Math.Abs(remainingTime.TotalSeconds + 30.0) < 1) {
                progressBar1.Visible = true;
                lblCurrentOperation.Visible = true;
                lblCurrentOperation.Text = "Consegna in corso";

                testTimer.Stop();
                MessageBox.Show("Tempo scaduto, invio in corso...", "Fine della prova", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                testManager.TestState.State = UserTestState.UserState.Finished;
                
                await testManager.Handover();

                ReloadUi();

                progressBar1.Visible = false;
                lblCurrentOperation.Visible = false;

                MessageBox.Show("Consegnato, ora si può chiudere RTesting", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnOpenTestDir_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(testManager.ResolvedTestPath);
        }

        private async void BtnConsegna_Click(object sender, EventArgs e) {

            progressBar1.Visible = true;
            lblCurrentOperation.Visible = true;
            lblCurrentOperation.Text = "Consegna in corso";

            if (MessageBox.Show(
                    $"ATTENZIONE: SALVARE E CHIUDERE TUTTI I PROGRAMMI CHE STANNO USANDO LA DIRECTORY {testManager.ResolvedTestPath} ALTREMENTI LE MODIFICHE NON VERRANNO SALVATE. PREMERE OK PER CONTINUARE. VUOI CONTINUARE?",
                    "Waiting Closure", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) {
                return;
            }

            testTimer.Stop();
            testManager.TestState.State = UserTestState.UserState.Finished;
            await testManager.Handover();
            ReloadUi();

            progressBar1.Visible = false;
            lblCurrentOperation.Visible = false;

            MessageBox.Show("Consegnato, ora si può chiudere RTesting", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}