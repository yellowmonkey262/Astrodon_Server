using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Server {

    public partial class frmSMS : Form {
        private const String username = "astrodon_sms";
        private const String password = "[sms@66r94e!@#]";
        private String status;
        private BindingSource bs = new BindingSource();

        public frmSMS() {
            InitializeComponent();
        }

        private void frmSMS_Load(object sender, EventArgs e) {
            txtCredits.Text = GetCredits(out status).ToString();
            String buildingQuery = "SELECT distinct building FROM tblSMS ORDER BY building";
            String customerQuery = "SELECT distinct customer FROM tblSMS ORDER BY customer";
            DataSet dsBuildings = DataHandler.getData(buildingQuery, out status);
            DataSet dsCustomers = DataHandler.getData(customerQuery, out status);
            cmbBuilding.SelectedIndexChanged -= cmbBuilding_SelectedIndexChanged;
            cmbBuilding.Items.Clear();
            cmbBuilding.Items.Add("All buildings");
            if (dsBuildings != null && dsBuildings.Tables.Count > 0 && dsBuildings.Tables[0].Rows.Count > 0) {
                foreach (DataRow drB in dsBuildings.Tables[0].Rows) {
                    cmbBuilding.Items.Add(drB["building"].ToString());
                }
            }
            cmbBuilding.SelectedIndex = 0;
            cmbBuilding.SelectedIndexChanged += cmbBuilding_SelectedIndexChanged;
            cmbCustomer.Items.Clear();
            cmbCustomer.Items.Add("All customers");
            if (dsCustomers != null && dsCustomers.Tables.Count > 0 && dsCustomers.Tables[0].Rows.Count > 0) {
                foreach (DataRow drB in dsCustomers.Tables[0].Rows) {
                    cmbCustomer.Items.Add(drB["customer"].ToString());
                }
            }
            cmbCustomer.SelectedIndex = 0;
            dataGridView1.DataSource = bs;
        }

        private void btnRetrieve_Click(object sender, EventArgs e) {
            String building = String.Empty;
            String customer = String.Empty;
            if (cmbBuilding.SelectedItem != null) { building = cmbBuilding.SelectedItem.ToString(); }
            if (cmbCustomer.SelectedItem != null) { customer = cmbCustomer.SelectedItem.ToString(); }
            DateTime sDate = sDatePicker.Value;
            DateTime eDate = eDatePicker.Value;
            DateTime startDate = new DateTime(sDate.Year, sDate.Month, sDate.Day, 0, 0, 0);
            DateTime endDate = new DateTime(eDate.Year, eDate.Month, eDate.Day, 23, 59, 59);
            String query = "SELECT s.building, s.customer, s.number, s.reference, s.message, s.sent, u.username, s.status FROM tblSMS AS s LEFT OUTER JOIN";
            query += " tblUsers AS u ON s.sender = u.id WHERE sent >= '" + startDate.ToString() + "' AND sent <= '" + endDate.ToString() + "'";
            if (!String.IsNullOrEmpty(building) && building == "All buildings") {
                if (!String.IsNullOrEmpty(customer) && customer == "All customers") {
                } else if (!String.IsNullOrEmpty(customer)) {
                    query += " AND s.customer = '" + customer + "'";
                }
            } else if (!String.IsNullOrEmpty(building) && !String.IsNullOrEmpty(customer) && customer == "All customers") {
                query += " AND s.building = '" + building + "'";
            } else if (!String.IsNullOrEmpty(building) && !String.IsNullOrEmpty(customer)) {
                query += " AND s.building = '" + building + "' AND s.customer = '" + customer + "'";
            }
            query += " ORDER BY s.sent";
            //MessageBox.Show(query);
            DataSet results = DataHandler.getData(query, out status);
            if (results != null && results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0) {
                Parallel.ForEach(results.Tables[0].AsEnumerable(), dr => {
                    DataRecords drec = new DataRecords();
                    drec.Building = dr["building"].ToString();
                    drec.Customer = dr["customer"].ToString();
                    drec.Cell = dr["number"].ToString();
                    drec.Reference = dr["reference"].ToString();
                    drec.Message = dr["message"].ToString();
                    drec.Sent = DateTime.Parse(dr["sent"].ToString());
                    drec.Sender = dr["username"].ToString();
                    drec.Status = dr["status"].ToString();
                    bs.Add(drec);
                });
            } else {
                MessageBox.Show("No results");
            }
        }

        private class DataRecords {
            public String Building { get; set; }

            public String Customer { get; set; }

            public String Cell { get; set; }

            public String Reference { get; set; }

            public String Message { get; set; }

            public DateTime Sent { get; set; }

            public String Sender { get; set; }

            public String Status { get; set; }
        }

        private double GetCredits(out String status) {
            string url = "http://bulksms.2way.co.za:5567/eapi/user/get_credits/1/1.1";
            string data = "";
            data += "username=" + HttpUtility.UrlEncode(username, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            data += "&password=" + HttpUtility.UrlEncode(password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            String result = Post(url, data);
            string[] parts = result.Split('|');
            if (parts.Length > 1) {
                string statusCode = parts[0];
                string statusString = parts[1];
                if (statusCode == "0") {
                    status = "";

                    return double.Parse(statusString);
                } else {
                    status = statusString;
                    return -1;
                }
            } else {
                status = parts[0];
                return -1;
            }
        }

        public string Post(string url, string data) {
            string result = null;
            try {
                byte[] buffer = Encoding.Default.GetBytes(data);

                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.Method = "POST";
                WebReq.ContentType = "application/x-www-form-urlencoded";
                WebReq.ContentLength = buffer.Length;
                Stream PostData = WebReq.GetRequestStream();

                PostData.Write(buffer, 0, buffer.Length);
                PostData.Close();
                HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                Console.WriteLine(WebResp.StatusCode);

                Stream Response = WebResp.GetResponseStream();
                StreamReader _Response = new StreamReader(Response);
                result = _Response.ReadToEnd();
            } catch (Exception ex) {
                result += "\n" + ex.Message;
            }
            return result.Trim() + "\n";
        }

        private void cmbBuilding_SelectedIndexChanged(object sender, EventArgs e) {
            if (cmbBuilding.SelectedItem != null) {
                String building = cmbBuilding.SelectedItem.ToString();
                String customerQuery = "SELECT distinct customer FROM tblSMS WHERE building = '" + building + "' ORDER BY customer";
                DataSet dsCustomers = DataHandler.getData(customerQuery, out status);
                cmbCustomer.Items.Clear();
                cmbCustomer.Items.Add("All customers");
                if (dsCustomers != null && dsCustomers.Tables.Count > 0 && dsCustomers.Tables[0].Rows.Count > 0) {
                    foreach (DataRow drB in dsCustomers.Tables[0].Rows) {
                        cmbCustomer.Items.Add(drB["customer"].ToString());
                    }
                }
            }
        }
    }
}