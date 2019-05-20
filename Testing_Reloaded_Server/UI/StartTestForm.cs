using System;
using System.Windows.Forms;
using SharedLibrary.Models;
using Testing_Reloaded_Server.Models;
using Testing_Reloaded_Server.Networking;

namespace Testing_Reloaded_Server.UI {
    public partial class StartTestForm : Form {
        private ServerPublishingManager publishManager;
        private TestManager testManager;

        public StartTestForm() {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
        }

        private void BtnStartTest_Click(object sender, EventArgs e) {
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

            publishManager = new ServerPublishingManager(test) { AllowClientsOnHold = true };
            testManager = new TestManager(test);

            var testForm = new TestForm(testManager);
            testForm.Closed += (o, args) => this.Close();

            testForm.Show();
            this.Hide();
        }
    }
}