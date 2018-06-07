using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FTPClient
{
    public partial class ConfigurationForm : Form
    {
        public ConfigurationForm()
        {
            InitializeComponent();
        }

        // Get FTP Configuration From Settings.settings file
        protected override void OnLoad(EventArgs e)
        {
            AddressTextbox.Text = Properties.Settings.Default.Address;
            UserTextbox.Text = Properties.Settings.Default.User;
            PasswordTextbox.Text = Properties.Settings.Default.Password;
        }

        /// <summary>
        /// Check whether address is valid
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool IsAddressValid(string address)
        {
            string pattern = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            return Regex.IsMatch(address, pattern);
        }

        private void ConfirmBtn_Click(object sender, EventArgs e)
        {
            string address = AddressTextbox.Text;
            string user = UserTextbox.Text;
            string passwd = PasswordTextbox.Text;
            if (!IsAddressValid(address))
            {
                MessageBox.Show(this, "IP地址无效");
            }

            // Save user settings
            Properties.Settings.Default.Address = address;
            Properties.Settings.Default.User = user;
            Properties.Settings.Default.Password = passwd;
        }
    }
}
