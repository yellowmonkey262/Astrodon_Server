﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class frmUpdateBuilding : Form
    {
        public frmUpdateBuilding()
        {
            InitializeComponent();
        }

        private void frmUpdateBuilding_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'astrodonDataSet.tblBuildings' table. You
            //       can move, or remove it, as needed.
            this.tblBuildingsTableAdapter.Fill(this.astrodonDataSet.tblBuildings);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbBuilding.SelectedItem != null)
            {
                try
                {
                    int idx = cmbBuilding.SelectedIndex;
                    DataRow dr = this.astrodonDataSet.tblBuildings[idx];
                    String building = dr["Building"].ToString();
                    String code = dr["Code"].ToString();
                    String datapath = dr["DataPath"].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

      

       
    }
}