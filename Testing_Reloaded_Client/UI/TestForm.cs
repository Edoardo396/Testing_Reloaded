﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Statics;
using Testing_Reloaded_Client.Networking;
using Testing_Reloaded_Server.Exceptions;

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
                if (testManager.CurrentTest == null) return;

                lblTestName.Text = testManager.CurrentTest.TestName;
                lblTestDuration.Text = testManager.CurrentTest.Time.ToString();
                lblRemainingTime.Text = testManager.TestState.RemainingTime.ToString();

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

            try {
                lblCurrentOperation.Text = "Connessione";
                await testManager.Connect();

                lblCurrentOperation.Text = "Download Dati Test";

                await testManager.DownloadTestData();
                ReloadUi();
            } catch (VersionMismatchException vme) {
                MessageBox.Show(
                    $"Errore di connessione, il erver e i client deveono utlizzare la stessa versione del software.\r\nVersione Server: {vme.ServerVersion}\r\nVersione del client: {vme.ClientVersion}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
                this.Close();
                return;
            } catch (Exception ex) {
                MessageBox.Show("Connessione al server fallita. Messaggio di errore: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
                this.Close();
                return;
            }

            lblTestDir.Text = "Attendo inizio del test";
            lblCurrentOperation.Text = "Attendo Inizio";

            try {
                await testManager.WaitForTestStart();
                ReloadUi();

                lblCurrentOperation.Text = "Download documentazione";
                await testManager.DownloadTestDocumentation();
            } catch (Exception ex) {
                MessageBox.Show("Download dei dati del test fallito. Messaggio di errore: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
                this.Close();
                return;
            }

            lblCurrentOperation.Visible = false;
            progressBar1.Visible = false;

            string message =
                $"Il test è iniziato.\r\nLa cartella del test è {testManager.ResolvedTestPath} puoi trovare la documentazione del test nella sottocartella Documentation se è disponibile. Quando consegnerai veraano inviati tutti i file che si trovano nella cartella del test. in bocca al lupo!";

            testManager.TestState.State = UserTestState.UserState.Testing;

            await testManager.SendStateUpdate();

            MessageBox.Show(message, "Test iniziato", MessageBoxButtons.OK, MessageBoxIcon.Information);

            testTimer.Start();

            testManager.TestStarted();

            ReloadUi();
        }

        private async void TestTimer_Tick(object sender, EventArgs e) {
            try {
                await testManager.TimeElapsed((uint) (testTimer.Interval / 1000));
            } catch (Exception ex) {
                MessageBox.Show(
                    $"Impossibile inviare al server aggiornamenti sullo stato del test, controlla che il server sia online. Il test è stato messo in pausa",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ReloadUi();
            }

            lblRemainingTime.Text = testManager.TestState.RemainingTime.ToString();

            TimeSpan remainingTime = testManager.TestState.RemainingTime;

            if (Math.Abs(remainingTime.TotalSeconds) < 1) {
                if (testManager.CurrentTest.ReclaimTestImmediately)
                    MessageBox.Show(
                        $"Il tempo è scaduto, hai un minuto per salvare e chiudere tutti i programmi che stanno usando la cartella {testManager.ResolvedTestPath}, dopodichè il file verrà inviato al server e tutte le modifiche andranno inevitabilmente perse",
                        "Tempo Scaduto", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                else {
                    MessageBox.Show(
                        $"Il tempo è scaduto",
                        "Tempo Scaduto", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    testManager.TestState.State = UserTestState.UserState.Finished;
                    testTimer.Stop();
                }
            }

            if (Math.Abs(remainingTime.TotalSeconds + 60.0) < 1 && testManager.CurrentTest.ReclaimTestImmediately) {
                progressBar1.Visible = true;
                lblCurrentOperation.Visible = true;
                lblCurrentOperation.Text = "Consegna in corso";

                testTimer.Stop();
                MessageBox.Show("Tempo scaduto, invio in corso...", "Fine della prova", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                try {
                    await testManager.Handover();
                } catch (Exception ex) {
                    MessageBox.Show("Errore nella consegna, controlla lo stato del server", "Fine della prova",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    testManager.TestState.State = UserTestState.UserState.OnHold;
                }

                progressBar1.Visible = false;
                lblCurrentOperation.Visible = false;

                MessageBox.Show("Consegnato, ora si può chiudere RTesting", "Done", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            ReloadUi();
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

            try {
                testTimer.Stop();
                await testManager.Handover();
            } catch (Exception ex) {
                MessageBox.Show(
                    "La consegna è fallita, riprova oppure richiedi la consegna manuale. Il test è stato messo in pausa",
                    "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return;
            }

            progressBar1.Visible = false;
            lblCurrentOperation.Visible = false;

            testManager.TestState.State = UserTestState.UserState.Finished;
            await testManager.SendStateUpdate();
            ReloadUi();
            testManager.Disconnect();
            MessageBox.Show("Consegnato, ora si può chiudere RTesting", "Done", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            testManager.Disconnect();
        }
    }
}