using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Testing_Reloaded_Client {
    public partial class frmTest : Form {

        private TestManager testManager;


        public frmTest(Server selectedServer) {
            InitializeComponent();

            testManager = new TestManager(selectedServer);
        }



        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);


        }
    }
}