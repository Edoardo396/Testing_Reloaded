﻿using System.Windows.Forms;

namespace Testing_Reloaded_Server.UI
{
    partial class TestForm : Form
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.grpClientControls = new System.Windows.Forms.GroupBox();
            this.btnToggleStateClient = new System.Windows.Forms.Button();
            this.btnForceHandover = new System.Windows.Forms.Button();
            this.lblSelectedClient = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnTimeAdd = new System.Windows.Forms.Button();
            this.txtTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lvClients = new System.Windows.Forms.ListView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnTestPause = new System.Windows.Forms.Button();
            this.btnTestStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.grpClientControls.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.grpClientControls);
            this.groupBox1.Controls.Add(this.lvClients);
            this.groupBox1.Location = new System.Drawing.Point(12, 76);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(609, 294);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Persone Collegate";
            // 
            // grpClientControls
            // 
            this.grpClientControls.Controls.Add(this.btnToggleStateClient);
            this.grpClientControls.Controls.Add(this.btnForceHandover);
            this.grpClientControls.Controls.Add(this.lblSelectedClient);
            this.grpClientControls.Controls.Add(this.label3);
            this.grpClientControls.Controls.Add(this.btnTimeAdd);
            this.grpClientControls.Controls.Add(this.txtTime);
            this.grpClientControls.Controls.Add(this.label2);
            this.grpClientControls.Location = new System.Drawing.Point(340, 16);
            this.grpClientControls.Name = "grpClientControls";
            this.grpClientControls.Size = new System.Drawing.Size(255, 272);
            this.grpClientControls.TabIndex = 9;
            this.grpClientControls.TabStop = false;
            this.grpClientControls.Text = "Client";
            // 
            // btnToggleStateClient
            // 
            this.btnToggleStateClient.Location = new System.Drawing.Point(146, 223);
            this.btnToggleStateClient.Name = "btnToggleStateClient";
            this.btnToggleStateClient.Size = new System.Drawing.Size(90, 33);
            this.btnToggleStateClient.TabIndex = 12;
            this.btnToggleStateClient.Text = "Pausa";
            this.btnToggleStateClient.UseVisualStyleBackColor = true;
            this.btnToggleStateClient.Click += new System.EventHandler(this.BtnToggleStateClient_Click);
            // 
            // btnForceHandover
            // 
            this.btnForceHandover.Location = new System.Drawing.Point(14, 223);
            this.btnForceHandover.Name = "btnForceHandover";
            this.btnForceHandover.Size = new System.Drawing.Size(90, 33);
            this.btnForceHandover.TabIndex = 11;
            this.btnForceHandover.Text = "Ritira";
            this.btnForceHandover.UseVisualStyleBackColor = true;
            this.btnForceHandover.Click += new System.EventHandler(this.BtnForceHandover_Click);
            // 
            // lblSelectedClient
            // 
            this.lblSelectedClient.AutoSize = true;
            this.lblSelectedClient.Location = new System.Drawing.Point(89, 26);
            this.lblSelectedClient.Name = "lblSelectedClient";
            this.lblSelectedClient.Size = new System.Drawing.Size(0, 13);
            this.lblSelectedClient.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Selezionato:";
            // 
            // btnTimeAdd
            // 
            this.btnTimeAdd.Location = new System.Drawing.Point(146, 68);
            this.btnTimeAdd.Name = "btnTimeAdd";
            this.btnTimeAdd.Size = new System.Drawing.Size(90, 33);
            this.btnTimeAdd.TabIndex = 8;
            this.btnTimeAdd.Text = "Aggiungi tempo";
            this.btnTimeAdd.UseVisualStyleBackColor = true;
            this.btnTimeAdd.Click += new System.EventHandler(this.BtnTimeAdd_Click);
            // 
            // txtTime
            // 
            this.txtTime.Location = new System.Drawing.Point(27, 81);
            this.txtTime.Name = "txtTime";
            this.txtTime.Size = new System.Drawing.Size(113, 20);
            this.txtTime.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Aggiungi tempo";
            // 
            // lvClients
            // 
            this.lvClients.FullRowSelect = true;
            this.lvClients.HideSelection = false;
            this.lvClients.Location = new System.Drawing.Point(11, 16);
            this.lvClients.Name = "lvClients";
            this.lvClients.Size = new System.Drawing.Size(317, 270);
            this.lvClients.TabIndex = 5;
            this.lvClients.UseCompatibleStateImageBehavior = false;
            this.lvClients.SelectedIndexChanged += new System.EventHandler(this.LvClients_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnTestPause);
            this.groupBox2.Controls.Add(this.btnTestStart);
            this.groupBox2.Location = new System.Drawing.Point(640, 76);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(283, 294);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Opzioni Test";
            // 
            // btnTestPause
            // 
            this.btnTestPause.Location = new System.Drawing.Point(172, 239);
            this.btnTestPause.Name = "btnTestPause";
            this.btnTestPause.Size = new System.Drawing.Size(90, 33);
            this.btnTestPause.TabIndex = 14;
            this.btnTestPause.Text = "Pausa Test";
            this.btnTestPause.UseVisualStyleBackColor = true;
            this.btnTestPause.Click += new System.EventHandler(this.Button4_Click);
            // 
            // btnTestStart
            // 
            this.btnTestStart.Location = new System.Drawing.Point(16, 239);
            this.btnTestStart.Name = "btnTestStart";
            this.btnTestStart.Size = new System.Drawing.Size(90, 33);
            this.btnTestStart.TabIndex = 13;
            this.btnTestStart.Text = "Avvia Test";
            this.btnTestStart.UseVisualStyleBackColor = true;
            this.btnTestStart.Click += new System.EventHandler(this.BtnTestStart_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(231, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Clients";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(745, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 29);
            this.label4.TabIndex = 3;
            this.label4.Text = "Test";
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(935, 375);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.groupBox1.ResumeLayout(false);
            this.grpClientControls.ResumeLayout(false);
            this.grpClientControls.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lvClients;
        private System.Windows.Forms.Button btnTimeAdd;
        private System.Windows.Forms.TextBox txtTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox grpClientControls;
        private System.Windows.Forms.Label lblSelectedClient;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnToggleStateClient;
        private System.Windows.Forms.Button btnForceHandover;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnTestStart;
        private System.Windows.Forms.Button btnTestPause;
    }
}