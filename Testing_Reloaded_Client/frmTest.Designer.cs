namespace Testing_Reloaded_Client
{
    partial class frmTest
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
            this.grbTestInfo = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblRemainingTime = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblTestName = new System.Windows.Forms.Label();
            this.lblTestDuration = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblCurrentOperation = new System.Windows.Forms.Label();
            this.grbTestInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // grbTestInfo
            // 
            this.grbTestInfo.Controls.Add(this.lblTestDuration);
            this.grbTestInfo.Controls.Add(this.label6);
            this.grbTestInfo.Controls.Add(this.lblTestName);
            this.grbTestInfo.Controls.Add(this.label4);
            this.grbTestInfo.Controls.Add(this.label3);
            this.grbTestInfo.Controls.Add(this.label2);
            this.grbTestInfo.Controls.Add(this.button1);
            this.grbTestInfo.Controls.Add(this.lblRemainingTime);
            this.grbTestInfo.Controls.Add(this.label1);
            this.grbTestInfo.Location = new System.Drawing.Point(12, 65);
            this.grbTestInfo.Name = "grbTestInfo";
            this.grbTestInfo.Size = new System.Drawing.Size(340, 423);
            this.grbTestInfo.TabIndex = 0;
            this.grbTestInfo.TabStop = false;
            this.grbTestInfo.Text = "Informazioni Test";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 355);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tempo Rimanente:";
            // 
            // lblRemainingTime
            // 
            this.lblRemainingTime.AutoSize = true;
            this.lblRemainingTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemainingTime.Location = new System.Drawing.Point(6, 377);
            this.lblRemainingTime.Name = "lblRemainingTime";
            this.lblRemainingTime.Size = new System.Drawing.Size(135, 33);
            this.lblRemainingTime.TabIndex = 1;
            this.lblRemainingTime.Text = "00:00:00";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(211, 364);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(107, 46);
            this.button1.TabIndex = 2;
            this.button1.Text = "Consegna";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 210);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Directory Consegna:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 232);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "C:\\Users\\user\\Desktop\\Name";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Nome Test:";
            // 
            // lblTestName
            // 
            this.lblTestName.AutoSize = true;
            this.lblTestName.Location = new System.Drawing.Point(77, 33);
            this.lblTestName.Name = "lblTestName";
            this.lblTestName.Size = new System.Drawing.Size(53, 13);
            this.lblTestName.TabIndex = 6;
            this.lblTestName.Text = "ASP.NET";
            // 
            // lblTestDuration
            // 
            this.lblTestDuration.AutoSize = true;
            this.lblTestDuration.Location = new System.Drawing.Point(77, 58);
            this.lblTestDuration.Name = "lblTestDuration";
            this.lblTestDuration.Size = new System.Drawing.Size(53, 13);
            this.lblTestDuration.TabIndex = 8;
            this.lblTestDuration.Text = "ASP.NET";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 58);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Durata Test:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 16);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(161, 34);
            this.progressBar1.TabIndex = 1;
            // 
            // lblCurrentOperation
            // 
            this.lblCurrentOperation.AutoSize = true;
            this.lblCurrentOperation.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentOperation.Location = new System.Drawing.Point(179, 16);
            this.lblCurrentOperation.Name = "lblCurrentOperation";
            this.lblCurrentOperation.Size = new System.Drawing.Size(143, 18);
            this.lblCurrentOperation.TabIndex = 9;
            this.lblCurrentOperation.Text = "Current Operation";
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 500);
            this.Controls.Add(this.lblCurrentOperation);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.grbTestInfo);
            this.Name = "frmTest";
            this.Text = "frmTest";
            this.grbTestInfo.ResumeLayout(false);
            this.grbTestInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grbTestInfo;
        private System.Windows.Forms.Label lblTestDuration;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblTestName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblRemainingTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblCurrentOperation;
    }
}