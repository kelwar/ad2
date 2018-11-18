namespace MainExample
{
    partial class EditVagonDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblVagonId = new System.Windows.Forms.Label();
            this.numVagonId = new System.Windows.Forms.NumericUpDown();
            this.lblVagonNumber = new System.Windows.Forms.Label();
            this.txtVagonNumber = new System.Windows.Forms.TextBox();
            this.lblPsType = new System.Windows.Forms.Label();
            this.cmbPsType = new System.Windows.Forms.ComboBox();
            this.lblVagonType = new System.Windows.Forms.Label();
            this.cmbVagonType = new System.Windows.Forms.ComboBox();
            this.lblLength = new System.Windows.Forms.Label();
            this.numLength = new System.Windows.Forms.NumericUpDown();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numVagonId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLength)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.9805F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.0195F));
            this.tableLayoutPanel1.Controls.Add(this.lblVagonId, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.numVagonId, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblVagonNumber, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtVagonNumber, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblPsType, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.cmbPsType, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblVagonType, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmbVagonType, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblLength, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.numLength, 1, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(276, 143);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblVagonId
            // 
            this.lblVagonId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVagonId.AutoSize = true;
            this.lblVagonId.Location = new System.Drawing.Point(3, 7);
            this.lblVagonId.Name = "lblVagonId";
            this.lblVagonId.Size = new System.Drawing.Size(195, 13);
            this.lblVagonId.TabIndex = 0;
            this.lblVagonId.Text = "Положение вагона в составе поезда";
            // 
            // numVagonId
            // 
            this.numVagonId.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.numVagonId.Location = new System.Drawing.Point(204, 4);
            this.numVagonId.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numVagonId.Name = "numVagonId";
            this.numVagonId.Size = new System.Drawing.Size(69, 20);
            this.numVagonId.TabIndex = 1;
            // 
            // lblVagonNumber
            // 
            this.lblVagonNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVagonNumber.AutoSize = true;
            this.lblVagonNumber.Location = new System.Drawing.Point(3, 35);
            this.lblVagonNumber.Name = "lblVagonNumber";
            this.lblVagonNumber.Size = new System.Drawing.Size(79, 13);
            this.lblVagonNumber.TabIndex = 2;
            this.lblVagonNumber.Text = "Номер вагона";
            // 
            // txtVagonNumber
            // 
            this.txtVagonNumber.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.txtVagonNumber.Location = new System.Drawing.Point(204, 32);
            this.txtVagonNumber.Name = "txtVagonNumber";
            this.txtVagonNumber.Size = new System.Drawing.Size(69, 20);
            this.txtVagonNumber.TabIndex = 3;
            // 
            // lblPsType
            // 
            this.lblPsType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPsType.AutoSize = true;
            this.lblPsType.Location = new System.Drawing.Point(3, 63);
            this.lblPsType.Name = "lblPsType";
            this.lblPsType.Size = new System.Drawing.Size(134, 13);
            this.lblPsType.TabIndex = 4;
            this.lblPsType.Text = "Тип подвижного состава";
            // 
            // cmbPsType
            // 
            this.cmbPsType.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmbPsType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPsType.FormattingEnabled = true;
            this.cmbPsType.Location = new System.Drawing.Point(204, 59);
            this.cmbPsType.Name = "cmbPsType";
            this.cmbPsType.Size = new System.Drawing.Size(69, 21);
            this.cmbPsType.TabIndex = 5;
            // 
            // lblVagonType
            // 
            this.lblVagonType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVagonType.AutoSize = true;
            this.lblVagonType.Location = new System.Drawing.Point(3, 91);
            this.lblVagonType.Name = "lblVagonType";
            this.lblVagonType.Size = new System.Drawing.Size(64, 13);
            this.lblVagonType.TabIndex = 6;
            this.lblVagonType.Text = "Тип вагона";
            // 
            // cmbVagonType
            // 
            this.cmbVagonType.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmbVagonType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVagonType.FormattingEnabled = true;
            this.cmbVagonType.Location = new System.Drawing.Point(204, 87);
            this.cmbVagonType.Name = "cmbVagonType";
            this.cmbVagonType.Size = new System.Drawing.Size(69, 21);
            this.cmbVagonType.TabIndex = 7;
            // 
            // lblLength
            // 
            this.lblLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLength.AutoSize = true;
            this.lblLength.Location = new System.Drawing.Point(3, 121);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(78, 13);
            this.lblLength.TabIndex = 8;
            this.lblLength.Text = "Длина вагона";
            // 
            // numLength
            // 
            this.numLength.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.numLength.Location = new System.Drawing.Point(204, 117);
            this.numLength.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numLength.Name = "numLength";
            this.numLength.Size = new System.Drawing.Size(69, 20);
            this.numLength.TabIndex = 9;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(117, 146);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(198, 146);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // EditVagonDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 172);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "EditVagonDialog";
            this.Text = "Характеристики вагона";
            this.Load += new System.EventHandler(this.AddVagonDialog_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numVagonId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLength)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblVagonId;
        private System.Windows.Forms.NumericUpDown numVagonId;
        private System.Windows.Forms.Label lblVagonNumber;
        private System.Windows.Forms.TextBox txtVagonNumber;
        private System.Windows.Forms.Label lblPsType;
        private System.Windows.Forms.ComboBox cmbPsType;
        private System.Windows.Forms.Label lblVagonType;
        private System.Windows.Forms.ComboBox cmbVagonType;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.NumericUpDown numLength;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}