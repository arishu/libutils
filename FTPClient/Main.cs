using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTPClient
{
    public partial class Main : Form
    {
        public struct GFTPInfo
        {
            public string Address { get; set; }
            public string User { get; set; }
            public string Passwd { get; set; }

            public GFTPInfo(string address, string user, string passwd)
            {
                Address = address;
                User = user;
                Passwd = passwd;
            }
        }

        internal static GFTPInfo ftpInfo = new GFTPInfo();

        public Main()
        {
            InitializeComponent();

            GetFtpConfiguration();

        }

        /// <summary>
        /// Get FTP Configuration From Settings.settings file
        /// </summary>
        private void GetFtpConfiguration()
        {
            string address = Properties.Settings.Default.Address;
            string user = Properties.Settings.Default.User;
            string passwd = Properties.Settings.Default.Password;

            ftpInfo.Address = address;
            ftpInfo.User = user;
            ftpInfo.Passwd = passwd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            ConfigurationForm configurationForm = new ConfigurationForm();
            configurationForm.ShowDialog();
        }
    }
}
