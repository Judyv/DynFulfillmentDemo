namespace TestDynGPConnect
{
    partial class MainForm
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
            this.btStart = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbInputFile = new System.Windows.Forms.ComboBox();
            this.edInputFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btDynConnect = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbCompany = new System.Windows.Forms.ComboBox();
            this.edDynPort = new System.Windows.Forms.TextBox();
            this.edDynServer = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbMessages = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(836, 98);
            this.btStart.Margin = new System.Windows.Forms.Padding(4);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(103, 28);
            this.btStart.TabIndex = 0;
            this.btStart.Text = "Test";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbInputFile);
            this.groupBox1.Controls.Add(this.edInputFolder);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btDynConnect);
            this.groupBox1.Controls.Add(this.btStart);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.cbCompany);
            this.groupBox1.Controls.Add(this.edDynPort);
            this.groupBox1.Controls.Add(this.edDynServer);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(952, 198);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dynamics Settings";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(424, 82);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 17);
            this.label2.TabIndex = 17;
            this.label2.Text = "Input File";
            // 
            // cbInputFile
            // 
            this.cbInputFile.FormattingEnabled = true;
            this.cbInputFile.Location = new System.Drawing.Point(427, 102);
            this.cbInputFile.Margin = new System.Windows.Forms.Padding(4);
            this.cbInputFile.Name = "cbInputFile";
            this.cbInputFile.Size = new System.Drawing.Size(384, 24);
            this.cbInputFile.TabIndex = 16;
            this.cbInputFile.SelectedIndexChanged += new System.EventHandler(this.cbInputFile_SelectedIndexChanged);
            // 
            // edInputFolder
            // 
            this.edInputFolder.Location = new System.Drawing.Point(427, 50);
            this.edInputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.edInputFolder.Name = "edInputFolder";
            this.edInputFolder.Size = new System.Drawing.Size(384, 22);
            this.edInputFolder.TabIndex = 15;
            this.edInputFolder.Leave += new System.EventHandler(this.edInputFolder_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(424, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 17);
            this.label1.TabIndex = 14;
            this.label1.Text = "Input Folder";
            // 
            // btDynConnect
            // 
            this.btDynConnect.Location = new System.Drawing.Point(259, 47);
            this.btDynConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btDynConnect.Name = "btDynConnect";
            this.btDynConnect.Size = new System.Drawing.Size(103, 28);
            this.btDynConnect.TabIndex = 13;
            this.btDynConnect.Text = "Connect";
            this.btDynConnect.UseVisualStyleBackColor = true;
            this.btDynConnect.Click += new System.EventHandler(this.btDynConnect_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 133);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(132, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "Dynamics Company";
            // 
            // cbCompany
            // 
            this.cbCompany.FormattingEnabled = true;
            this.cbCompany.Location = new System.Drawing.Point(17, 151);
            this.cbCompany.Margin = new System.Windows.Forms.Padding(4);
            this.cbCompany.Name = "cbCompany";
            this.cbCompany.Size = new System.Drawing.Size(256, 24);
            this.cbCompany.TabIndex = 4;
            this.cbCompany.SelectedIndexChanged += new System.EventHandler(this.cbCompany_SelectedIndexChanged);
            // 
            // edDynPort
            // 
            this.edDynPort.Location = new System.Drawing.Point(17, 101);
            this.edDynPort.Margin = new System.Windows.Forms.Padding(4);
            this.edDynPort.Name = "edDynPort";
            this.edDynPort.Size = new System.Drawing.Size(205, 22);
            this.edDynPort.TabIndex = 3;
            this.edDynPort.TextChanged += new System.EventHandler(this.edDynPort_TextChanged);
            // 
            // edDynServer
            // 
            this.edDynServer.Location = new System.Drawing.Point(17, 49);
            this.edDynServer.Margin = new System.Windows.Forms.Padding(4);
            this.edDynServer.Name = "edDynServer";
            this.edDynServer.Size = new System.Drawing.Size(205, 22);
            this.edDynServer.TabIndex = 2;
            this.edDynServer.TextChanged += new System.EventHandler(this.edDynServer_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 81);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(99, 17);
            this.label7.TabIndex = 1;
            this.label7.Text = "Dynamics Port";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 30);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(115, 17);
            this.label6.TabIndex = 0;
            this.label6.Text = "Dynamics Server";
            // 
            // lbMessages
            // 
            this.lbMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMessages.FormattingEnabled = true;
            this.lbMessages.HorizontalScrollbar = true;
            this.lbMessages.ItemHeight = 16;
            this.lbMessages.Items.AddRange(new object[] {
            " "});
            this.lbMessages.Location = new System.Drawing.Point(0, 198);
            this.lbMessages.Margin = new System.Windows.Forms.Padding(4);
            this.lbMessages.Name = "lbMessages";
            this.lbMessages.Size = new System.Drawing.Size(952, 284);
            this.lbMessages.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 482);
            this.Controls.Add(this.lbMessages);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Test Dynamics API";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btDynConnect;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbCompany;
        private System.Windows.Forms.TextBox edDynPort;
        private System.Windows.Forms.TextBox edDynServer;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListBox lbMessages;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbInputFile;
        private System.Windows.Forms.TextBox edInputFolder;
        private System.Windows.Forms.Label label1;
    }
}

