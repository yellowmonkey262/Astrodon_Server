using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public partial class frmMain : Form
    {
        #region Variables

        public static Pastel pastel;
        private SMSPoll smsPoll;
        private Statement statementRunner;
        private MailReceiver receiver;

        #endregion Variables

        #region Constructor

        public frmMain()
        {
            InitializeComponent();
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Application.Exit();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                pastel = new Pastel();
                pastel.Message += new Pastel.MessageHandler(pastel_Message);
                pastel.InitialisePastel();
                String msg;
                receiver = new MailReceiver(out msg);
                smsPoll = new SMSPoll();
                smsPoll.NewMessageEvent += new EventHandler<MessageArgs>(smsPoll_NewMessageEvent);
                smsPoll.InitializePolling();
                statementRunner = new Statement();
                statementRunner.Message += new Statement.MessageHandler(statementRunner_Message);
                Purger purger = new Purger();
            }
            catch { }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (receiver != null && receiver.timer.Enabled) { receiver.ToggleTimer(); }
            if (pastel != null) { pastel.Close(); }
            Application.DoEvents();
            Thread.Sleep(5000);
        }

        #endregion Constructor

        #region events

        private void statementRunner_Message(object sender, PastelArgs e)
        {
            SetText(e.message);
        }

        private void smsPoll_NewMessageEvent(object sender, MessageArgs e)
        {
            SetText(e.message);
        }

        private void receiver_NewMessageEvent(object sender, MessageArgs e)
        {
            SetText(e.message);
        }

        private void pastel_Message(object sender, PastelArgs e)
        {
            SetText(e.message);
        }

        #endregion events

        #region Class Methods

        private delegate void SetTextDelegate(String text);

        public void SetText(String text)
        {
            if (InvokeRequired)
            {
                Invoke(new SetTextDelegate(SetText), text);
            }
            else
            {
                if (text == "clear")
                {
                    rtfStatus.Text = "";
                }
                else
                {
                    if (rtfStatus.TextLength >= rtfStatus.MaxLength - 20) { rtfStatus.Text = ""; }
                    rtfStatus.Text += text + Environment.NewLine;
                }
                rtfStatus.SelectionStart = rtfStatus.TextLength;
                rtfStatus.ScrollToCaret();
                Application.DoEvents();
            }
        }

        private void ResetMail()
        {
            if (receiver.timer.Enabled) { receiver.ToggleTimer(); }
            receiver.timer.Interval = int.Parse(Server.Properties.Settings.Default.SendReceive);
            receiver.ToggleTimer();
        }

        #endregion Class Methods

        #region UI Methods

        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Altering the settings on this screen may cause the program to fail! Continue?", "Server Application", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
            {
                frmSettings settings = new frmSettings();
                if (settings.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    pastel.InitialisePastel();
                    ResetMail();
                    smsPoll.InitializePolling();
                }
            }
        }

        private void searchEmailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Customer> customers = pastel.GetCustomers(false, null);
            String reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports", "astrodon email addresses.csv");
            StreamWriter writer = new StreamWriter(reportPath);
            foreach (Customer c in customers)
            {
                String line = c.accNumber + ",";
                foreach (String email in c.Email)
                {
                    if (!String.IsNullOrEmpty(email)) { line += email; }
                }
                writer.WriteLine(line);
            }
            writer.Close();
            MessageBox.Show("Done");
        }

        private void testUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statementRunner.SendStatements();
            statementRunner.UploadFiles();
        }

        private void sendSMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            smsPoll.SendMessages();
        }

        private void getSMSResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            smsPoll.QueryStatus();
        }

        private void sendLettersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statementRunner.SendLetters();
        }

        private void updateWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmUpdateBuilding updBuild = new frmUpdateBuilding();
            updBuild.ShowDialog();
        }

        private void updateALLWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statementRunner.UpdateCustomers();
        }

        private void purgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statementRunner.Purge();
        }

        private void sMSReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSMS smsReport = new frmSMS();
            smsReport.ShowDialog();
        }

        private void sendEmailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statementRunner.SendBulkMails();
        }

        private void clearScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetText("clear");
        }

        #endregion UI Methods

        private void stephenUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statementRunner.UploadFiles(DateTime.Now.AddDays(-1));
        }

        private void offAdminPurgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Purger purger = new Purger();
            purger.TransferFiles();
            MessageBox.Show("Complete");
        }

        private void wARNINGForceRentalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pastel.runRental();
        }
    }
}