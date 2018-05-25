using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using libutilscore.Logging;

namespace FTPWorker
{
    public partial class Main : Form
    {
        /// <summary>
        /// Default Ftp info
        /// </summary>
        public static ArrayList ftpInfo = new ArrayList
        {
                "127.0.0.1", "anonymous",
                "V1ZjMWRtSnViSFJpTTFaNlVVUkZlVTU1TkhkTWFrRjFUVkU5UFE9PQ ==", "/"
        };

        public static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public Main()
        {
            InitializeComponent();

            ThreadPool.QueueUserWorkItem(new WaitCallback(DefaultStart));
            //DefaultStart();
        }

        static void DefaultStart(object obj)
        {
            try
            {
                // read configuration, and save ftp information
                string ftpHost = ConfigurationManager.AppSettings["host"];
                string ftpUser = ConfigurationManager.AppSettings["user"];
                string ftpPasswd = ConfigurationManager.AppSettings["passwd"];
                string ftpRemotePath = ConfigurationManager.AppSettings["remotePath"];
                if (ftpRemotePath == "")
                    ftpRemotePath = "/";

                ftpInfo = new ArrayList { ftpHost, ftpUser, ftpPasswd, ftpRemotePath };

                Log.Logger.Debug("Start Server in default");
                TcpServer server = new TcpServer(55505);
                server._StartSocket();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Server Exception: {0}", ex.StackTrace);
            }
        }

        // Start FTP Task Window
        public static void StartFTPTaskWindow(FTPTask task)
        {
            TaskForm taskForm = new TaskForm(task);
            taskForm.ShowDialog();
            taskForm.Activate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            cancellationTokenSource.Cancel();
        }
    }
}
