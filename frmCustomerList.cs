using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server {

    public partial class frmCustomerList : Form {
        private List<CustomerConstruct> allCustomers;
        private List<WebConstruct> webCustomers;
        private Dictionary<String, DataConstruct> dataCustomers;

        public frmCustomerList() {
            InitializeComponent();
        }

        private void frmCustomerList_Load(object sender, EventArgs e) {
            this.tblBuildingsTableAdapter.Fill(this.astrodonDataSet.tblBuildings);
            allCustomers = new List<CustomerConstruct>();
            webCustomers = new List<WebConstruct>();
        }

        public void GetWebAccounts(String buildingAbbr, bool showMe) {
            String status = String.Empty;
            String astroQuery = "SELECT b.name, m.account_no, m.owner_email as email, f.username, f.disable FROM tx_astro_complex b inner join tx_astro_account_user_mapping m";
            astroQuery += " on m.complex_id = b.uid inner join fe_users f on m.cruser_id = f.uid ";
            if (!String.IsNullOrEmpty(buildingAbbr)) { astroQuery += " WHERE b.abbr = '" + buildingAbbr + "'"; }
            astroQuery += " order by b.name, m.account_no";
            MySqlConnector mySql = new MySqlConnector();
            DataSet ds = mySql.GetData(astroQuery, null, out status);
            int webCount = 0;
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                Parallel.ForEach(ds.Tables[0].AsEnumerable(), dr => {
                    String acc = dr["account_no"].ToString();
                    bool active = (int.Parse(dr["disable"].ToString()) == 0 ? true : false);
                    String owner_email = dr["email"].ToString();
                    String username = dr["username"].ToString();
                    if (dataCustomers.Keys.Contains(acc)) {
                        dataCustomers[acc].Web_Email = owner_email;
                        dataCustomers[acc].Web_User = username;
                        dataCustomers[acc].Web_User_Active = active;
                        webCount++;
                    }
                });
            }
            if (showMe) { MessageBox.Show(webCount.ToString() + " customers from web"); }
        }

        public void GetPastelAccounts(String buildingCode, bool showMe) {
            String buildQ = "SELECT Building, DataPath FROM tblBuildings ";
            if (!String.IsNullOrEmpty(buildingCode)) { buildQ += "WHERE code = '" + buildingCode + "'"; }
            buildQ += " ORDER by Building";
            String status = String.Empty;
            DataSet ds = DataHandler.getData(buildQ, out status);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                MySqlConnector mySql = new MySqlConnector();
                mySql.ToggleConnection(true);
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    String buildingName = dr["Building"].ToString();
                    List<Customer> customers = frmMain.pastel.AddCustomers("", dr["DataPath"].ToString());
                    foreach (Customer c in customers) {
                        CustomerConstruct cc = new CustomerConstruct();
                        cc.buildingName = buildingName;
                        cc.acc = c.accNumber;
                        cc.emails = c.Email;
                        allCustomers.Add(cc);
                    }
                }
            }
            foreach (CustomerConstruct c in allCustomers) {
                DataConstruct dc = new DataConstruct();
                dc.Building = c.buildingName;
                dc.Account = c.acc;
                String email = String.Empty;
                foreach (String e in c.emails) { if (!e.Contains("imp.ad-one")) { email += e + ";"; } }
                dc.Pastel_Emails = email;
                if (!dataCustomers.Keys.Contains(c.acc)) {
                    dataCustomers.Add(c.acc, dc);
                }
            }
            if (showMe) { MessageBox.Show(dataCustomers.Count.ToString() + " customers retrieved"); }
        }

        private void btnGo_Click(object sender, EventArgs e) {
            allCustomers = new List<CustomerConstruct>();
            dataCustomers = new Dictionary<string, DataConstruct>();
            webCustomers = new List<WebConstruct>();
            String abbr = cmbBuilding.SelectedValue.ToString();
            if (!String.IsNullOrEmpty(abbr)) {
                GetPastelAccounts(abbr, true);
                GetWebAccounts(abbr, true);
                if (dataCustomers.Count > 0) {
                    BindingSource bs = new BindingSource();
                    foreach (DataConstruct dc in dataCustomers.Values) { bs.Add(dc); }
                    dataGridView1.DataSource = bs;
                }
            } else {
                dataGridView1.DataSource = null;
            }
        }

        private StreamWriter sw;

        private void btnAll_Click(object sender, EventArgs e) {
            String pathName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pastelusers.csv");
            sw = new StreamWriter(pathName);
            sw.WriteLine("Building,Acc,Emails");
            allCustomers = new List<CustomerConstruct>();
            foreach (DataRow dr in this.astrodonDataSet.tblBuildings.Rows) {
                String abbr = dr["code"].ToString();
                if (!String.IsNullOrEmpty(abbr)) {
                    GetPastelAccounts(abbr);
                }
            }
            sw.Close();

            MailSender.SendMail("noreply@astrodon.co.za", "stephen@metathought.co.za", "Customer List", "Here it is", false, new String[] { pathName });
        }

        public void GetPastelAccounts(String buildingCode) {
            String buildQ = "SELECT Building, DataPath FROM tblBuildings ";
            if (!String.IsNullOrEmpty(buildingCode)) { buildQ += "WHERE code = '" + buildingCode + "'"; }
            buildQ += " ORDER by Building";
            String status = String.Empty;
            DataSet ds = DataHandler.getData(buildQ, out status);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                MySqlConnector mySql = new MySqlConnector();
                mySql.ToggleConnection(true);
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    String buildingName = dr["Building"].ToString();
                    List<Customer> customers = frmMain.pastel.AddCustomers("", dr["DataPath"].ToString());
                    foreach (Customer c in customers) {
                        CustomerConstruct cc = new CustomerConstruct();
                        cc.buildingName = buildingName;
                        cc.acc = c.accNumber;
                        cc.emails = c.Email;
                        String email = "";
                        foreach (String e in c.Email) {
                            email += e + ";";
                        }
                        sw.WriteLine(buildingName + "," + c.accNumber + "," + email);
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xls)|*.xls";
            sfd.FileName = "export.xls";
            if (sfd.ShowDialog() == DialogResult.OK) {
                //ToCsV(dataGridView1, @"c:\export.xls");
                ToCsV(dataGridView1, sfd.FileName); // Here dataGridview1 is your grid view name
            }
        }

        private void ToCsV(DataGridView dGV, string filename) {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            for (int j = 0; j < dGV.Columns.Count; j++)
                sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
            stOutput += sHeaders + "\r\n";
            // Export data.
            for (int i = 0; i < dGV.RowCount - 1; i++) {
                string stLine = "";
                for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                    stLine = stLine.ToString() + Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t";
                stOutput += stLine + "\r\n";
            }
            Encoding utf16 = Encoding.GetEncoding(1254);
            byte[] output = utf16.GetBytes(stOutput);
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(output, 0, output.Length); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();
        }
    }

    public class DataConstruct {
        public String Building { get; set; }

        public String Account { get; set; }

        public String Pastel_Emails { get; set; }

        public String Web_Email { get; set; }

        public String Web_User { get; set; }

        public bool Web_User_Active { get; set; }
    }
}