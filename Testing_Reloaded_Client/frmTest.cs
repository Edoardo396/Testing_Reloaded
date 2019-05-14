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



        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);


        }
    }
}