using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public partial class frmDeleteStatements : Form
    {
        public frmDeleteStatements()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var sql = "exec usp_ExportStatementDelete " + numFrom.Value.ToString() + " , " + numTo.Value.ToString();
            string stats;
            DataHandler.setData(sql, out stats);

            MessageBox.Show("Delete executed : " + stats);

            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
