using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class frmCustomerList : Form
    {
        private List<CustomerConstruct> allCustomers;
        private List<WebConstruct> webCustomers;
        private Dictionary<String, DataConstruct> dataCustomers;

        public frmCustomerList()
        {
            InitializeComponent();
        }

        private void frmCustomerList_Load(object sender, EventArgs e)
        {
            this.tblBuildingsTableAdapter.Fill(this.astrodonDataSet.tblBuildings);
            allCustomers = new List<CustomerConstruct>();
            webCustomers = new List<WebConstruct>();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            allCustomers = new List<CustomerConstruct>();
            dataCustomers = new Dictionary<string, DataConstruct>();
            webCustomers = new List<WebConstruct>();
            String abbr = cmbBuilding.SelectedValue.ToString();
            if (!String.IsNullOrEmpty(abbr))
            {
                if (dataCustomers.Count > 0)
                {
                    BindingSource bs = new BindingSource();
                    foreach (DataConstruct dc in dataCustomers.Values) { bs.Add(dc); }
                    dataGridView1.DataSource = bs;
                }
            }
            else
            {
                dataGridView1.DataSource = null;
            }
        }

        private StreamWriter sw;

        private void btnAll_Click(object sender, EventArgs e)
        {
          
        }


        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xls)|*.xls";
            sfd.FileName = "export.xls";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //ToCsV(dataGridView1, @"c:\export.xls");
                ToCsV(dataGridView1, sfd.FileName); // Here dataGridview1 is your grid view name
            }
        }

        private void ToCsV(DataGridView dGV, string filename)
        {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            for (int j = 0; j < dGV.Columns.Count; j++)
                sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
            stOutput += sHeaders + "\r\n";
            // Export data.
            for (int i = 0; i < dGV.RowCount - 1; i++)
            {
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

    public class DataConstruct
    {
        public String Building { get; set; }

        public String Account { get; set; }

        public String Pastel_Emails { get; set; }

        public String Web_Email { get; set; }

        public String Web_User { get; set; }

        public bool Web_User_Active { get; set; }
    }
}