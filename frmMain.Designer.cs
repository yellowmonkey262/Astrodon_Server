namespace Server {
    partial class frmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.rtfStatus = new System.Windows.Forms.RichTextBox();
            this.btnSettings = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.searchEmailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adminToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testUploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendLettersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendSMSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sMSReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getSMSResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateWebToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateALLWebToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.purgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendEmailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.offAdminPurgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stephenUploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wARNINGForceRentalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtfStatus
            // 
            this.rtfStatus.BackColor = System.Drawing.Color.Black;
            this.rtfStatus.ForeColor = System.Drawing.Color.White;
            this.rtfStatus.Location = new System.Drawing.Point(12, 37);
            this.rtfStatus.Name = "rtfStatus";
            this.rtfStatus.Size = new System.Drawing.Size(781, 496);
            this.rtfStatus.TabIndex = 0;
            this.rtfStatus.Text = "";
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.Red;
            this.btnSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.Location = new System.Drawing.Point(12, 539);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(781, 33);
            this.btnSettings.TabIndex = 1;
            this.btnSettings.Text = "Change settings";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchEmailsToolStripMenuItem,
            this.adminToolStripMenuItem,
            this.clearScreenToolStripMenuItem,
            this.stephenUploadToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(805, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // searchEmailsToolStripMenuItem
            // 
            this.searchEmailsToolStripMenuItem.Name = "searchEmailsToolStripMenuItem";
            this.searchEmailsToolStripMenuItem.Size = new System.Drawing.Size(119, 20);
            this.searchEmailsToolStripMenuItem.Text = "Generate Email List";
            this.searchEmailsToolStripMenuItem.Click += new System.EventHandler(this.searchEmailsToolStripMenuItem_Click);
            // 
            // adminToolStripMenuItem
            // 
            this.adminToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testUploadToolStripMenuItem,
            this.sendLettersToolStripMenuItem,
            this.sendSMSToolStripMenuItem,
            this.sMSReportToolStripMenuItem,
            this.getSMSResultsToolStripMenuItem,
            this.updateWebToolStripMenuItem,
            this.updateALLWebToolStripMenuItem,
            this.purgeToolStripMenuItem,
            this.sendEmailsToolStripMenuItem,
            this.offAdminPurgeToolStripMenuItem,
            this.wARNINGForceRentalToolStripMenuItem});
            this.adminToolStripMenuItem.Name = "adminToolStripMenuItem";
            this.adminToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.adminToolStripMenuItem.Text = "Admin";
            // 
            // testUploadToolStripMenuItem
            // 
            this.testUploadToolStripMenuItem.Name = "testUploadToolStripMenuItem";
            this.testUploadToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.testUploadToolStripMenuItem.Text = "Send Statements";
            this.testUploadToolStripMenuItem.Click += new System.EventHandler(this.testUploadToolStripMenuItem_Click);
            // 
            // sendLettersToolStripMenuItem
            // 
            this.sendLettersToolStripMenuItem.Name = "sendLettersToolStripMenuItem";
            this.sendLettersToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.sendLettersToolStripMenuItem.Text = "Send letters";
            this.sendLettersToolStripMenuItem.Click += new System.EventHandler(this.sendLettersToolStripMenuItem_Click);
            // 
            // sendSMSToolStripMenuItem
            // 
            this.sendSMSToolStripMenuItem.Name = "sendSMSToolStripMenuItem";
            this.sendSMSToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.sendSMSToolStripMenuItem.Text = "Send SMS";
            this.sendSMSToolStripMenuItem.Click += new System.EventHandler(this.sendSMSToolStripMenuItem_Click);
            // 
            // sMSReportToolStripMenuItem
            // 
            this.sMSReportToolStripMenuItem.Name = "sMSReportToolStripMenuItem";
            this.sMSReportToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.sMSReportToolStripMenuItem.Text = "SMS Report";
            this.sMSReportToolStripMenuItem.Click += new System.EventHandler(this.sMSReportToolStripMenuItem_Click);
            // 
            // getSMSResultsToolStripMenuItem
            // 
            this.getSMSResultsToolStripMenuItem.Name = "getSMSResultsToolStripMenuItem";
            this.getSMSResultsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.getSMSResultsToolStripMenuItem.Text = "Get SMS results";
            this.getSMSResultsToolStripMenuItem.Click += new System.EventHandler(this.getSMSResultsToolStripMenuItem_Click);
            // 
            // updateWebToolStripMenuItem
            // 
            this.updateWebToolStripMenuItem.Name = "updateWebToolStripMenuItem";
            this.updateWebToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.updateWebToolStripMenuItem.Text = "Update Web";
            this.updateWebToolStripMenuItem.Click += new System.EventHandler(this.updateWebToolStripMenuItem_Click);
            // 
            // updateALLWebToolStripMenuItem
            // 
            this.updateALLWebToolStripMenuItem.Name = "updateALLWebToolStripMenuItem";
            this.updateALLWebToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.updateALLWebToolStripMenuItem.Text = "Update ALL Web";
            this.updateALLWebToolStripMenuItem.Click += new System.EventHandler(this.updateALLWebToolStripMenuItem_Click);
            // 
            // purgeToolStripMenuItem
            // 
            this.purgeToolStripMenuItem.Name = "purgeToolStripMenuItem";
            this.purgeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.purgeToolStripMenuItem.Text = "Purge";
            this.purgeToolStripMenuItem.Click += new System.EventHandler(this.purgeToolStripMenuItem_Click);
            // 
            // sendEmailsToolStripMenuItem
            // 
            this.sendEmailsToolStripMenuItem.Name = "sendEmailsToolStripMenuItem";
            this.sendEmailsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.sendEmailsToolStripMenuItem.Text = "Send Emails";
            this.sendEmailsToolStripMenuItem.Click += new System.EventHandler(this.sendEmailsToolStripMenuItem_Click);
            // 
            // offAdminPurgeToolStripMenuItem
            // 
            this.offAdminPurgeToolStripMenuItem.Name = "offAdminPurgeToolStripMenuItem";
            this.offAdminPurgeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.offAdminPurgeToolStripMenuItem.Text = "Off admin purge";
            this.offAdminPurgeToolStripMenuItem.Click += new System.EventHandler(this.offAdminPurgeToolStripMenuItem_Click);
            // 
            // clearScreenToolStripMenuItem
            // 
            this.clearScreenToolStripMenuItem.Name = "clearScreenToolStripMenuItem";
            this.clearScreenToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.clearScreenToolStripMenuItem.Text = "Clear Screen";
            this.clearScreenToolStripMenuItem.Click += new System.EventHandler(this.clearScreenToolStripMenuItem_Click);
            // 
            // stephenUploadToolStripMenuItem
            // 
            this.stephenUploadToolStripMenuItem.Name = "stephenUploadToolStripMenuItem";
            this.stephenUploadToolStripMenuItem.Size = new System.Drawing.Size(111, 20);
            this.stephenUploadToolStripMenuItem.Text = "Stephen - Upload";
            this.stephenUploadToolStripMenuItem.Click += new System.EventHandler(this.stephenUploadToolStripMenuItem_Click);
            // 
            // wARNINGForceRentalToolStripMenuItem
            // 
            this.wARNINGForceRentalToolStripMenuItem.Name = "wARNINGForceRentalToolStripMenuItem";
            this.wARNINGForceRentalToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.wARNINGForceRentalToolStripMenuItem.Text = "WARNING : Force Rental";
            this.wARNINGForceRentalToolStripMenuItem.Click += new System.EventHandler(this.wARNINGForceRentalToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 571);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.rtfStatus);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pastel Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtfStatus;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem searchEmailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adminToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testUploadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendSMSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getSMSResultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendLettersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateWebToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateALLWebToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem purgeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sMSReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendEmailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stephenUploadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offAdminPurgeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wARNINGForceRentalToolStripMenuItem;
    }
}

