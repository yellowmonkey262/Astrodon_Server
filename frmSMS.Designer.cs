namespace Server {
    partial class frmSMS {
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtCredits = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbCustomer = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.sDatePicker = new System.Windows.Forms.DateTimePicker();
            this.eDatePicker = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.btnRetrieve = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.cmbBuilding = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Available Credits";
            // 
            // txtCredits
            // 
            this.txtCredits.Location = new System.Drawing.Point(103, 6);
            this.txtCredits.Name = "txtCredits";
            this.txtCredits.ReadOnly = true;
            this.txtCredits.Size = new System.Drawing.Size(100, 20);
            this.txtCredits.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(260, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select Customer:";
            // 
            // cmbCustomer
            // 
            this.cmbCustomer.FormattingEnabled = true;
            this.cmbCustomer.Items.AddRange(new object[] {
            "All Customers"});
            this.cmbCustomer.Location = new System.Drawing.Point(351, 35);
            this.cmbCustomer.Name = "cmbCustomer";
            this.cmbCustomer.Size = new System.Drawing.Size(151, 21);
            this.cmbCustomer.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "From:";
            // 
            // sDatePicker
            // 
            this.sDatePicker.CustomFormat = "yyyy/MM/dd";
            this.sDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.sDatePicker.Location = new System.Drawing.Point(103, 67);
            this.sDatePicker.Name = "sDatePicker";
            this.sDatePicker.Size = new System.Drawing.Size(100, 20);
            this.sDatePicker.TabIndex = 5;
            // 
            // eDatePicker
            // 
            this.eDatePicker.CustomFormat = "yyyy/MM/dd";
            this.eDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.eDatePicker.Location = new System.Drawing.Point(262, 67);
            this.eDatePicker.Name = "eDatePicker";
            this.eDatePicker.Size = new System.Drawing.Size(100, 20);
            this.eDatePicker.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(213, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "To:";
            // 
            // btnRetrieve
            // 
            this.btnRetrieve.Location = new System.Drawing.Point(15, 103);
            this.btnRetrieve.Name = "btnRetrieve";
            this.btnRetrieve.Size = new System.Drawing.Size(75, 23);
            this.btnRetrieve.TabIndex = 8;
            this.btnRetrieve.Text = "Get Records";
            this.btnRetrieve.UseVisualStyleBackColor = true;
            this.btnRetrieve.Click += new System.EventHandler(this.btnRetrieve_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(15, 132);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(1079, 383);
            this.dataGridView1.TabIndex = 9;
            // 
            // cmbBuilding
            // 
            this.cmbBuilding.FormattingEnabled = true;
            this.cmbBuilding.Items.AddRange(new object[] {
            "All Buildings"});
            this.cmbBuilding.Location = new System.Drawing.Point(103, 35);
            this.cmbBuilding.Name = "cmbBuilding";
            this.cmbBuilding.Size = new System.Drawing.Size(151, 21);
            this.cmbBuilding.TabIndex = 11;
            this.cmbBuilding.SelectedIndexChanged += new System.EventHandler(this.cmbBuilding_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 38);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Select Building:";
            // 
            // frmSMS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1106, 527);
            this.Controls.Add(this.cmbBuilding);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnRetrieve);
            this.Controls.Add(this.eDatePicker);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.sDatePicker);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbCustomer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtCredits);
            this.Controls.Add(this.label1);
            this.Name = "frmSMS";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SMS Report";
            this.Load += new System.EventHandler(this.frmSMS_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCredits;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbCustomer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker sDatePicker;
        private System.Windows.Forms.DateTimePicker eDatePicker;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnRetrieve;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ComboBox cmbBuilding;
        private System.Windows.Forms.Label label5;
    }
}