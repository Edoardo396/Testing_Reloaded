namespace Testing_Reloaded_Server.UI
{
    partial class StartTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtTestName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chbTime = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chbDelete = new System.Windows.Forms.CheckBox();
            this.chbRitira = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtDataDownloadPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtConsegneDir = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDocsDir = new System.Windows.Forms.TextBox();
            this.btnStartTest = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtTestName
            // 
            this.txtTestName.Location = new System.Drawing.Point(94, 29);
            this.txtTestName.Name = "txtTestName";
            this.txtTestName.Size = new System.Drawing.Size(252, 20);
            this.txtTestName.TabIndex = 0;
            this.txtTestName.Tag = "onlybefore";
            this.txtTestName.Text = "SuperTest";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Nome Prova:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Tempo:";
            // 
            // chbTime
            // 
            this.chbTime.Location = new System.Drawing.Point(94, 55);
            this.chbTime.Name = "chbTime";
            this.chbTime.Size = new System.Drawing.Size(252, 20);
            this.chbTime.TabIndex = 2;
            this.chbTime.Tag = "onlybefore";
            this.chbTime.Text = "00:02:00";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chbDelete);
            this.groupBox1.Controls.Add(this.chbRitira);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtTestName);
            this.groupBox1.Controls.Add(this.chbTime);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(374, 169);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Impostazione Prova";
            // 
            // chbDelete
            // 
            this.chbDelete.AutoSize = true;
            this.chbDelete.Location = new System.Drawing.Point(22, 122);
            this.chbDelete.Name = "chbDelete";
            this.chbDelete.Size = new System.Drawing.Size(216, 17);
            this.chbDelete.TabIndex = 5;
            this.chbDelete.Tag = "onlybefore";
            this.chbDelete.Text = "Cancella file dal client dopo la consegna";
            this.chbDelete.UseVisualStyleBackColor = true;
            this.chbDelete.CheckedChanged += new System.EventHandler(this.ChbDelete_CheckedChanged);
            // 
            // chbRitira
            // 
            this.chbRitira.AutoSize = true;
            this.chbRitira.Checked = true;
            this.chbRitira.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbRitira.Location = new System.Drawing.Point(22, 99);
            this.chbRitira.Name = "chbRitira";
            this.chbRitira.Size = new System.Drawing.Size(177, 17);
            this.chbRitira.TabIndex = 4;
            this.chbRitira.Tag = "onlybefore";
            this.chbRitira.Text = "Ritira immediatamente al termine";
            this.chbRitira.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtDataDownloadPath);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 187);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(374, 66);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Impostazioni del client";
            // 
            // txtDataDownloadPath
            // 
            this.txtDataDownloadPath.Location = new System.Drawing.Point(145, 31);
            this.txtDataDownloadPath.Name = "txtDataDownloadPath";
            this.txtDataDownloadPath.Size = new System.Drawing.Size(217, 20);
            this.txtDataDownloadPath.TabIndex = 1;
            this.txtDataDownloadPath.Tag = "onlybefore";
            this.txtDataDownloadPath.Text = "%USERPROFILE%\\Desktop\\Temporanea\\$test_name_$surname";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Indirizzo download dati:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.txtConsegneDir);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.txtDocsDir);
            this.groupBox3.Location = new System.Drawing.Point(12, 259);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(374, 97);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Percorsi Server";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(36, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Directory Consegne:";
            // 
            // txtConsegneDir
            // 
            this.txtConsegneDir.Location = new System.Drawing.Point(145, 26);
            this.txtConsegneDir.Name = "txtConsegneDir";
            this.txtConsegneDir.Size = new System.Drawing.Size(223, 20);
            this.txtConsegneDir.TabIndex = 8;
            this.txtConsegneDir.Tag = "onlybefore";
            this.txtConsegneDir.Text = "C:\\Users\\edoar\\Desktop\\Progetti\\Test_RT\\consegne";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Directory documentazione:";
            // 
            // txtDocsDir
            // 
            this.txtDocsDir.Location = new System.Drawing.Point(145, 52);
            this.txtDocsDir.Name = "txtDocsDir";
            this.txtDocsDir.Size = new System.Drawing.Size(223, 20);
            this.txtDocsDir.TabIndex = 6;
            this.txtDocsDir.Tag = "onlybefore";
            this.txtDocsDir.Text = "C:\\Users\\edoar\\Desktop\\Progetti\\Test_RT\\docs";
            // 
            // btnStartTest
            // 
            this.btnStartTest.Location = new System.Drawing.Point(141, 362);
            this.btnStartTest.Name = "btnStartTest";
            this.btnStartTest.Size = new System.Drawing.Size(113, 55);
            this.btnStartTest.TabIndex = 8;
            this.btnStartTest.Text = "Avvia Prova";
            this.btnStartTest.UseVisualStyleBackColor = true;
            this.btnStartTest.Click += new System.EventHandler(this.BtnStartTest_Click);
            // 
            // StartTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 439);
            this.Controls.Add(this.btnStartTest);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "StartTestForm";
            this.Text = "Impostazioni test";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtTestName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox chbTime;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chbRitira;
        private System.Windows.Forms.CheckBox chbDelete;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtDataDownloadPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDocsDir;
        private System.Windows.Forms.Button btnStartTest;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtConsegneDir;
    }
}

