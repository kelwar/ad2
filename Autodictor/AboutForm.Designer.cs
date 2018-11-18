﻿namespace MainExample
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_ActivationInfo = new System.Windows.Forms.TextBox();
            this.btn_activationWindow = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(68, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(405, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Продукт: Автоинформатор - программа для";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(56, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(425, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "динамического информирования пассажиров";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(31, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(482, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "о движении поездов и других звуковых сообщений.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(12, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(338, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "Производитель: ООО \"АС-СИБИРЬ\"";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(12, 184);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(272, 24);
            this.label5.TabIndex = 4;
            this.label5.Text = "Все права защищены @ 2016.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(205, 113);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(220, 24);
            this.label6.TabIndex = 5;
            this.label6.Text = "Версия:  20180803_0058";
            // 
            // tb_ActivationInfo
            // 
            this.tb_ActivationInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb_ActivationInfo.Location = new System.Drawing.Point(16, 227);
            this.tb_ActivationInfo.Name = "tb_ActivationInfo";
            this.tb_ActivationInfo.ReadOnly = true;
            this.tb_ActivationInfo.Size = new System.Drawing.Size(390, 26);
            this.tb_ActivationInfo.TabIndex = 6;
            // 
            // btn_activationWindow
            // 
            this.btn_activationWindow.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_activationWindow.Location = new System.Drawing.Point(412, 211);
            this.btn_activationWindow.Name = "btn_activationWindow";
            this.btn_activationWindow.Size = new System.Drawing.Size(117, 45);
            this.btn_activationWindow.TabIndex = 7;
            this.btn_activationWindow.Text = "Окно активации";
            this.btn_activationWindow.UseVisualStyleBackColor = true;
            this.btn_activationWindow.Click += new System.EventHandler(this.btn_activationWindow_Click);
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 268);
            this.Controls.Add(this.btn_activationWindow);
            this.Controls.Add(this.tb_ActivationInfo);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "О программе";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_ActivationInfo;
        private System.Windows.Forms.Button btn_activationWindow;
    }
}