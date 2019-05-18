using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Testing_Reloaded_Server {
    public partial class StartTestForm : Form {

        private ServerPublishingManager publishManager;

        public StartTestForm() {
            InitializeComponent();
        }

        private void BtnWaitForClients_Click(object sender, EventArgs e) {
            var test = new Test {
                DataDownloadPath = txtDataDownloadPath.Text,
                DeleteFilesAfterEnd = chbDelete.Checked,
                ReclaimTestImmediately = chbRitira.Checked,
                TestName = txtTestName.Text,
                Time = TimeSpan.Parse(chbTime.Text)
            };

            publishManager = new ServerPublishingManager(test) {AllowClientsOnHold = true};
        }
    }
}