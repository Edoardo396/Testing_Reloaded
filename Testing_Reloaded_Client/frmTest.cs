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
        }


        protected override async void OnLoad(EventArgs e) {
            base.OnLoad(e);
            progressBar1.Enabled = true;
            progressBar1.Style = ProgressBarStyle.Marquee;

            lblCurrentOperation.Text = "Connessione";
            await testManager.Connect();

            lblCurrentOperation.Text = "Download Dati Test";

            await testManager.DownloadTestData();
            lblTestName.Text = testManager.CurrentTest.TestName;
            lblTestDuration.Text = testManager.CurrentTest.Time.ToString();
            lblRemainingTime.Text = testManager.CurrentTest.Time.ToString();

            lblTestDir.Text = "Attendo inizio del test";
            lblCurrentOperation.Text = "Attendo Inizio";

            await testManager.WaitForTestStart();

            lblTestDir.Text = testManager.ResolvePath(testManager.CurrentTest.DataDownloadPath);
            lblCurrentOperation.Text = "Download documentazione";
            await testManager.DownloadTestDocumentation();


        }
    }
}