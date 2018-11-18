namespace MainExample
{
    partial class StationsForm
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
            this.dgv_Stations = new System.Windows.Forms.DataGridView();
            this.btnAddDirection = new System.Windows.Forms.Button();
            this.btnRemoveDirection = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.lvDirections = new System.Windows.Forms.ListView();
            this.btnAddStation = new System.Windows.Forms.Button();
            this.btnRemoveStation = new System.Windows.Forms.Button();
            this.btnMoveDirectionUp = new System.Windows.Forms.Button();
            this.btnMoveDirectionDown = new System.Windows.Forms.Button();
            this.btnMoveStationUp = new System.Windows.Forms.Button();
            this.MoveStationDown = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Stations)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_Stations
            // 
            this.dgv_Stations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Stations.Location = new System.Drawing.Point(293, 36);
            this.dgv_Stations.MultiSelect = false;
            this.dgv_Stations.Name = "dgv_Stations";
            this.dgv_Stations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Stations.Size = new System.Drawing.Size(588, 460);
            this.dgv_Stations.TabIndex = 0;
            this.dgv_Stations.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Stations_CellContentClick);
            this.dgv_Stations.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_Stations_RowEnter);
            // 
            // btnAddDirection
            // 
            this.btnAddDirection.Location = new System.Drawing.Point(12, 7);
            this.btnAddDirection.Name = "btnAddDirection";
            this.btnAddDirection.Size = new System.Drawing.Size(75, 23);
            this.btnAddDirection.TabIndex = 2;
            this.btnAddDirection.Text = "Добавить";
            this.btnAddDirection.UseVisualStyleBackColor = true;
            this.btnAddDirection.Click += new System.EventHandler(this.btnAddDirection_Click);
            // 
            // btnRemoveDirection
            // 
            this.btnRemoveDirection.Location = new System.Drawing.Point(93, 7);
            this.btnRemoveDirection.Name = "btnRemoveDirection";
            this.btnRemoveDirection.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveDirection.TabIndex = 4;
            this.btnRemoveDirection.Text = "Удалить";
            this.btnRemoveDirection.UseVisualStyleBackColor = true;
            this.btnRemoveDirection.Click += new System.EventHandler(this.btnRemoveDirection_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(887, 471);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(887, 413);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(887, 442);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 7;
            this.btnApply.Text = "Применить";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lvDirections
            // 
            this.lvDirections.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.lvDirections.AllowColumnReorder = true;
            this.lvDirections.AutoArrange = false;
            this.lvDirections.FullRowSelect = true;
            this.lvDirections.GridLines = true;
            this.lvDirections.LabelEdit = true;
            this.lvDirections.Location = new System.Drawing.Point(12, 36);
            this.lvDirections.MultiSelect = false;
            this.lvDirections.Name = "lvDirections";
            this.lvDirections.ShowGroups = false;
            this.lvDirections.Size = new System.Drawing.Size(239, 460);
            this.lvDirections.TabIndex = 8;
            this.lvDirections.UseCompatibleStateImageBehavior = false;
            this.lvDirections.View = System.Windows.Forms.View.List;
            this.lvDirections.SelectedIndexChanged += new System.EventHandler(this.lvDirections_SelectedIndexChanged);
            // 
            // btnAddStation
            // 
            this.btnAddStation.Location = new System.Drawing.Point(293, 7);
            this.btnAddStation.Name = "btnAddStation";
            this.btnAddStation.Size = new System.Drawing.Size(75, 23);
            this.btnAddStation.TabIndex = 9;
            this.btnAddStation.Text = "Добавить";
            this.btnAddStation.UseVisualStyleBackColor = true;
            this.btnAddStation.Click += new System.EventHandler(this.btnAddStation_Click);
            // 
            // btnRemoveStation
            // 
            this.btnRemoveStation.Location = new System.Drawing.Point(374, 7);
            this.btnRemoveStation.Name = "btnRemoveStation";
            this.btnRemoveStation.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveStation.TabIndex = 10;
            this.btnRemoveStation.Text = "Удалить";
            this.btnRemoveStation.UseVisualStyleBackColor = true;
            this.btnRemoveStation.Click += new System.EventHandler(this.btnRemoveStation_Click);
            // 
            // btnMoveDirectionUp
            // 
            this.btnMoveDirectionUp.Location = new System.Drawing.Point(257, 36);
            this.btnMoveDirectionUp.Name = "btnMoveDirectionUp";
            this.btnMoveDirectionUp.Size = new System.Drawing.Size(30, 23);
            this.btnMoveDirectionUp.TabIndex = 11;
            this.btnMoveDirectionUp.Text = "▲";
            this.btnMoveDirectionUp.UseVisualStyleBackColor = true;
            this.btnMoveDirectionUp.Click += new System.EventHandler(this.btnMoveDirectionUp_Click);
            // 
            // btnMoveDirectionDown
            // 
            this.btnMoveDirectionDown.Location = new System.Drawing.Point(257, 65);
            this.btnMoveDirectionDown.Name = "btnMoveDirectionDown";
            this.btnMoveDirectionDown.Size = new System.Drawing.Size(30, 23);
            this.btnMoveDirectionDown.TabIndex = 12;
            this.btnMoveDirectionDown.Text = "▼";
            this.btnMoveDirectionDown.UseVisualStyleBackColor = true;
            this.btnMoveDirectionDown.Click += new System.EventHandler(this.btnMoveDirectionDown_Click);
            // 
            // btnMoveStationUp
            // 
            this.btnMoveStationUp.Location = new System.Drawing.Point(887, 36);
            this.btnMoveStationUp.Name = "btnMoveStationUp";
            this.btnMoveStationUp.Size = new System.Drawing.Size(30, 23);
            this.btnMoveStationUp.TabIndex = 11;
            this.btnMoveStationUp.Text = "▲";
            this.btnMoveStationUp.UseVisualStyleBackColor = true;
            this.btnMoveStationUp.Click += new System.EventHandler(this.btnMoveStationUp_Click);
            // 
            // MoveStationDown
            // 
            this.MoveStationDown.Location = new System.Drawing.Point(887, 65);
            this.MoveStationDown.Name = "MoveStationDown";
            this.MoveStationDown.Size = new System.Drawing.Size(30, 23);
            this.MoveStationDown.TabIndex = 12;
            this.MoveStationDown.Text = "▼";
            this.MoveStationDown.UseVisualStyleBackColor = true;
            this.MoveStationDown.Click += new System.EventHandler(this.MoveStationDown_Click);
            // 
            // StationsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(973, 506);
            this.Controls.Add(this.MoveStationDown);
            this.Controls.Add(this.btnMoveDirectionDown);
            this.Controls.Add(this.btnMoveStationUp);
            this.Controls.Add(this.btnMoveDirectionUp);
            this.Controls.Add(this.btnRemoveStation);
            this.Controls.Add(this.btnAddStation);
            this.Controls.Add(this.lvDirections);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRemoveDirection);
            this.Controls.Add(this.btnAddDirection);
            this.Controls.Add(this.dgv_Stations);
            this.Name = "StationsForm";
            this.Text = " Форма внесения и загрузки станций";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StationsForm_FormClosing);
            this.Load += new System.EventHandler(this.StationsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Stations)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_Stations;
        private System.Windows.Forms.Button btnAddDirection;
        private System.Windows.Forms.Button btnRemoveDirection;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.ListView lvDirections;
        private System.Windows.Forms.Button btnAddStation;
        private System.Windows.Forms.Button btnRemoveStation;
        private System.Windows.Forms.Button btnMoveDirectionUp;
        private System.Windows.Forms.Button btnMoveDirectionDown;
        private System.Windows.Forms.Button btnMoveStationUp;
        private System.Windows.Forms.Button MoveStationDown;
    }
}