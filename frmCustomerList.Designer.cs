namespace Server {
    partial class frmCustomerList {
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
            this.components = new System.ComponentModel.Container();
            this.cmbBuilding = new System.Windows.Forms.ComboBox();
            this.tblBuildingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.astrodonDataSet = new Server.AstrodonDataSet();
            this.label1 = new System.Windows.Forms.Label();
            this.tblBuildingsTableAdapter = new Server.AstrodonDataSetTableAdapters.tblBuildingsTableAdapter();
            this.btnGo = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tblBuildingsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.astrodonDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbBuilding
            // 
            this.cmbBuilding.DataSource = this.tblBuildingsBindingSource;
            this.cmbBuilding.DisplayMember = "Building";
            this.cmbBuilding.FormattingEnabled = true;
            this.cmbBuilding.Location = new System.Drawing.Point(62, 12);
            this.cmbBuilding.Name = "cmbBuilding";
            this.cmbBuilding.Size = new System.Drawing.Size(280, 21);
            this.cmbBuilding.TabIndex = 3;
            this.cmbBuilding.ValueMember = "Code";
            // 
            // tblBuildingsBindingSource
            // 
            this.tblBuildingsBindingSource.DataMember = "tblBuildings";
            this.tblBuildingsBindingSource.DataSource = this.astrodonDataSet;
            // 
            // astrodonDataSet
            // 
            this.astrodonDataSet.DataSetName = "AstrodonDataSet";
            this.astrodonDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Building";
            // 
            // tblBuildingsTableAdapter
            // 
            this.tblBuildingsTableAdapter.ClearBeforeFill = true;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(348, 10);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 5;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(15, 53);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(967, 512);
            this.dataGridView1.TabIndex = 6;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(907, 571);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnAll
            // 
            this.btnAll.Location = new System.Drawing.Point(429, 12);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(75, 23);
            this.btnAll.TabIndex = 8;
            this.btnAll.Text = "All Customers";
            this.btnAll.UseVisualStyleBackColor = true;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // frmCustomerList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(994, 606);
            this.Controls.Add(this.btnAll);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.cmbBuilding);
            this.Controls.Add(this.label1);
            this.Name = "frmCustomerList";
            this.Text = "Customer List";
            this.Load += new System.EventHandler(this.frmCustomerList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tblBuildingsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.astrodonDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbBuilding;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource tblBuildingsBindingSource;
        private AstrodonDataSet astrodonDataSet;
        private AstrodonDataSetTableAdapters.tblBuildingsTableAdapter tblBuildingsTableAdapter;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnAll;
    }
}