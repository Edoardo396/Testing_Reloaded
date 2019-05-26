using System;
using System.Windows.Forms;
using Testing_Reloaded_Client.UI;

namespace Testing_Reloaded_Client {
    internal static class Program {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new JoinTestForm());
        }
    }
}