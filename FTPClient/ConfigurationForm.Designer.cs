namespace FTPClient
{
    partial class ConfigurationForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AddressTextbox = new System.Windows.Forms.TextBox();
            this.UserTextbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.PasswordTextbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ConfirmBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.PasswordTextbox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.UserTextbox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.AddressTextbox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(9, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(388, 200);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(42, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP地址:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AddressTextbox
            // 
            this.AddressTextbox.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.AddressTextbox.Location = new System.Drawing.Point(119, 41);
            this.AddressTextbox.Name = "AddressTextbox";
            this.AddressTextbox.Size = new System.Drawing.Size(222, 23);
            this.AddressTextbox.TabIndex = 1;
            // 
            // UserTextbox
            // 
            this.UserTextbox.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UserTextbox.Location = new System.Drawing.Point(119, 87);
            this.UserTextbox.Name = "UserTextbox";
            this.UserTextbox.Size = new System.Drawing.Size(222, 23);
            this.UserTextbox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(42, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "用户名:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PasswordTextbox
            // 
            this.PasswordTextbox.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.PasswordTextbox.Location = new System.Drawing.Point(119, 133);
            this.PasswordTextbox.Name = "PasswordTextbox";
            this.PasswordTextbox.PasswordChar = '*';
            this.PasswordTextbox.Size = new System.Drawing.Size(222, 23);
            this.PasswordTextbox.TabIndex = 5;
            this.PasswordTextbox.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(42, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "密    码:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ConfirmBtn
            // 
            this.ConfirmBtn.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ConfirmBtn.Location = new System.Drawing.Point(262, 211);
            this.ConfirmBtn.Name = "ConfirmBtn";
            this.ConfirmBtn.Size = new System.Drawing.Size(89, 34);
            this.ConfirmBtn.TabIndex = 1;
            this.ConfirmBtn.Text = "确定";
            this.ConfirmBtn.UseVisualStyleBackColor = true;
            this.ConfirmBtn.Click += new System.EventHandler(this.ConfirmBtn_Click);
            // 
            // ConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 257);
            this.Controls.Add(this.ConfirmBtn);
            this.Controls.Add(this.groupBox1);
            this.Name = "ConfigurationForm";
            this.Text = "FTP配置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox PasswordTextbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox UserTextbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox AddressTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ConfirmBtn;
    }
}