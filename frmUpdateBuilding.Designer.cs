namespace Server {
    partial class frmUpdateBuilding {
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmbBuilding = new System.Windows.Forms.ComboBox();
            this.tblBuildingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.astrodonDataSet = new Server.AstrodonDataSet();
            this.tblBuildingsTableAdapter = new Server.AstrodonDataSetTableAdapters.tblBuildingsTableAdapter();
            this.btnUpdate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tblBuildingsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.astrodonDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Building";
            // 
            // cmbBuilding
            // 
            this.cmbBuilding.DataSource = this.tblBuildingsBindingSource;
            this.cmbBuilding.DisplayMember = "Building";
            this.cmbBuilding.FormattingEnabled = true;
            this.cmbBuilding.Location = new System.Drawing.Point(74, 6);
            this.cmbBuilding.Name = "cmbBuilding";
            this.cmbBuilding.Size = new System.Drawing.Size(185, 21);
            this.cmbBuilding.TabIndex = 1;
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
            // tblBuildingsTableAdapter
            // 
            this.tblBuildingsTableAdapter.ClearBeforeFill = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(184, 33);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // frmUpdateBuilding
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(280, 69);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.cmbBuilding);
            this.Controls.Add(this.label1);
            this.Name = "frmUpdateBuilding";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmUpdateBuilding";
            this.Load += new System.EventHandler(this.frmUpdateBuilding_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tblBuildingsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.astrodonDataSet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbBuilding;
        private AstrodonDataSet astrodonDataSet;
        private System.Windows.Forms.BindingSource tblBuildingsBindingSource;
        private AstrodonDataSetTableAdapters.tblBuildingsTableAdapter tblBuildingsTableAdapter;
        private System.Windows.Forms.Button btnUpdate;
    }
}