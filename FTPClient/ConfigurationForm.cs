using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static FTPClient.Main;

namespace FTPClient
{
    public partial class ConfigurationForm : Form
    {
        public ConfigurationForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            AddressTextbox.Text = ftpInfo.Address;
            UserTextbox.Text = ftpInfo.User;
            PasswordTextbox.Text = ftpInfo.Passwd;
        }

        private void ConfirmBtn_Click(object sender, EventArgs e)
        {

        }
    }
}
