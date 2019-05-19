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

namespace Testing_Reloaded_Client {
    public partial class frmTest : Form {
        private TestManager testManager;


        public frmTest(Server selectedServer, User me) {
            InitializeComponent();

            testManager = new TestManager(selectedServer, me);
            testManager.ReloadUI += ReloadUi;
        }

        private void ReloadUi() {
            lblTestName.Text = testManager.CurrentTest.TestName;
            lblTestDuration.Text = testManager.CurrentTest.Time.ToString();
            lblRemainingTime.Text = testManager.CurrentTest.Time.ToString();

            if (testManager.CurrentTest.State != Test.TestState.NotStarted)
                lblTestDir.Text = testManager.ResolvedTestPath;

            lblStatus.Text = testManager.CurrentTest.State.ToString();
            lblStatus.BackColor = testManager.CurrentTest.State.TestStateToColor();
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

        private void TestTimer_Tick(object sender, EventArgs e) {
            testManager.TimeElapsed((uint) (testTimer.Interval / 1000));
            lblRemainingTime.Text = testManager.TestState.RemainingTime.ToString();
        }

        private void BtnOpenTestDir_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(testManager.ResolvedTestPath);
        }
    }
}