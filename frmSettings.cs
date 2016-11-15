using System;
using System.Windows.Forms;

namespace Server {

    public partial class frmSettings : Form {

        public frmSettings() {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            int email = 0;
            int sms = 0;

            if (cmbEmail.SelectedItem != null && int.TryParse(cmbEmail.SelectedItem.ToString(), out email)) {
                Server.Properties.Settings.Default.SendReceive = (email * 60000).ToString();
            } else {
                Server.Properties.Settings.Default.SendReceive = "60000";
            }
            if (cmbSMS.SelectedItem != null && int.TryParse(cmbSMS.SelectedItem.ToString(), out sms)) {
                Server.Properties.Settings.Default.CheckSMS = (sms * 60000).ToString();
            } else {
                Server.Properties.Settings.Default.CheckSMS = "60000";
            }
            Server.Properties.Settings.Default.Save();
            BindSettings();
            this.Invalidate();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            BindSettings();
            this.Invalidate();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void frmSettings_Load(object sender, EventArgs e) {
            BindSettings();
        }

        private void BindSettings() {
            cmbEmail.SelectedItem = (int.Parse(Server.Properties.Settings.Default.SendReceive) / 60000).ToString();
            cmbSMS.SelectedItem = (int.Parse(Server.Properties.Settings.Default.CheckSMS) / 60000).ToString();
        }
    }
}