namespace FTPClient
{
    partial class Main
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
            this.DownloadBtn = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.navigateLabel = new System.Windows.Forms.Label();
            this.pathLabel = new System.Windows.Forms.Label();
            this.pathList = new System.Windows.Forms.ListView();
            this.logLabel = new System.Windows.Forms.Label();
            this.logList = new System.Windows.Forms.ListView();
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ConnectBtn = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DownloadBtn
            // 
            this.DownloadBtn.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.DownloadBtn.Location = new System.Drawing.Point(496, 20);
            this.DownloadBtn.Name = "DownloadBtn";
            this.DownloadBtn.Size = new System.Drawing.Size(97, 23);
            this.DownloadBtn.TabIndex = 3;
            this.DownloadBtn.Text = "Download";
            this.DownloadBtn.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(-3, 73);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.navigateLabel);
            this.splitContainer1.Panel1.Controls.Add(this.pathLabel);
            this.splitContainer1.Panel1.Controls.Add(this.pathList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.logLabel);
            this.splitContainer1.Panel2.Controls.Add(this.logList);
            this.splitContainer1.Size = new System.Drawing.Size(603, 400);
            this.splitContainer1.SplitterDistance = 222;
            this.splitContainer1.TabIndex = 5;
            // 
            // navigateLabel
            // 
            this.navigateLabel.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.navigateLabel.Location = new System.Drawing.Point(78, 0);
            this.navigateLabel.Name = "navigateLabel";
            this.navigateLabel.Size = new System.Drawing.Size(141, 23);
            this.navigateLabel.TabIndex = 2;
            this.navigateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pathLabel
            // 
            this.pathLabel.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pathLabel.Location = new System.Drawing.Point(-1, 0);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(80, 23);
            this.pathLabel.TabIndex = 1;
            this.pathLabel.Text = "当前路径:";
            this.pathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pathList
            // 
            this.pathList.Location = new System.Drawing.Point(3, 26);
            this.pathList.Name = "pathList";
            this.pathList.Size = new System.Drawing.Size(216, 374);
            this.pathList.TabIndex = 0;
            this.pathList.UseCompatibleStateImageBehavior = false;
            // 
            // logLabel
            // 
            this.logLabel.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.logLabel.Location = new System.Drawing.Point(3, 3);
            this.logLabel.Name = "logLabel";
            this.logLabel.Size = new System.Drawing.Size(371, 23);
            this.logLabel.TabIndex = 2;
            this.logLabel.Text = "日志信息";
            this.logLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // logList
            // 
            this.logList.Location = new System.Drawing.Point(3, 29);
            this.logList.Name = "logList";
            this.logList.Size = new System.Drawing.Size(379, 371);
            this.logList.TabIndex = 0;
            this.logList.UseCompatibleStateImageBehavior = false;
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.DownloadBtn);
            this.groupBox1.Controls.Add(this.ConnectBtn);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(0, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(600, 65);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "连接信息";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(3, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "FTP地址:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ConnectBtn
            // 
            this.ConnectBtn.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ConnectBtn.Location = new System.Drawing.Point(379, 20);
            this.ConnectBtn.Name = "ConnectBtn";
            this.ConnectBtn.Size = new System.Drawing.Size(97, 23);
            this.ConnectBtn.TabIndex = 1;
            this.ConnectBtn.Text = "Connect";
            this.ConnectBtn.UseVisualStyleBackColor = true;
            this.ConnectBtn.Click += new System.EventHandler(this.ConnectBtn_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(79, 20);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(274, 23);
            this.textBox1.TabIndex = 0;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 474);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Main";
            this.Text = "FTP客户端";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button DownloadBtn;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView pathList;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ConnectBtn;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label navigateLabel;
        private System.Windows.Forms.Label logLabel;
        private System.Windows.Forms.ListView logList;
        private System.Windows.Forms.Label label3;
    }
}

