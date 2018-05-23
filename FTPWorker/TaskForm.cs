using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using libutilscore.FTP;
using System.Net;
using System.Diagnostics;
using libutilscore.Logging;

namespace FTPWorker
{
    public partial class TaskForm : Form
    {
        private static ArrayList ValidOperations = new ArrayList { "get", "put" };
        private static string localFilePath = null;

        public TaskForm(FTPTask task)
        {
            InitializeComponent();

            string operation = task.Operation;
            if (operation == "get")
            {
                string localFilePath = (string)task.OperaionArgs.ToArray().GetValue(2);
                label1.Text = "正在下载：" + Path.GetFileName(localFilePath);
                ViewBtn.Visible = true;
            }
            else if (operation == "put")
            {
                string localFilePath = (string)task.OperaionArgs.ToArray().GetValue(1);
                label1.Text = "正在上传：" + Path.GetFileName(localFilePath);
                ViewBtn.Visible = false;
            }


            backgroundWorker1.RunWorkerAsync(task);
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            FTPTask task = (FTPTask)e.Argument;

            string operation = task.Operation;

            switch (operation)
            {
                case "put":
                    {
                        DoUpload(task, e);
                        break;
                    }
                case "get":
                    {
                        DoDownload(task, e);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private int GetCorrectBufferSize(int originnal, long totalSize)
        {
            int times = (int)(Math.Sqrt(totalSize) / originnal);
            if (times > 10)
            {
                return originnal * 10;
            }
            else if (times >= 4)
            {
                return originnal;
            }
            else
            {
                return 1024;
            }
        }

        private void DoUpload(FTPTask task, DoWorkEventArgs e)
        {
            Array ftpCfg = Main.ftpInfo.ToArray();
            FTPInfo ftpInfo = new FTPInfo((string)ftpCfg.GetValue(0), (string)ftpCfg.GetValue(1),
                (string)ftpCfg.GetValue(2), (string)ftpCfg.GetValue(3));
            Array args = task.OperaionArgs.ToArray();
            string operationId = (string)args.GetValue(0);
            string localFilePath = (string)args.GetValue(1);
            string remotePath = (string)args.GetValue(2);
            bool createRemotePath = (bool)args.GetValue(2);

            FTPClient client = new FTPClient(ftpInfo)
            {
                Operation = FTPClient.FtpOperation.UPLOAD,
                OperationArgs = new ArrayList { (string)args.GetValue(1), (string)args.GetValue(2), (string)args.GetValue(3) }
            };

            bool success = client.ChangeRemoteDir(remotePath, createRemotePath);
            if (!success)
                throw client.LastErrorException;

            FileInfo fileInf = new FileInfo(localFilePath);
            string StrFilePath = "";
            if (remotePath == null)
                StrFilePath += fileInf.Name;
            else
            {
                StrFilePath += (client.GetRemoteDirPath(remotePath) + "/" + fileInf.Name);
            }

            FtpWebRequest request = client.GetFtpRequest(client.GetStrUri(StrFilePath));
            Log.Logger.Info("Starting uploading file: {0} ...", fileInf.Name);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            // set type to image 'i'
            request.UseBinary = true;
            // Set local client to passive
            request.UsePassive = true;
            Log.Logger.Info("Writing to remote ...");

            FTPClient.FtpState state = client.state;

            try
            {
                using (FileStream fstream = File.OpenRead(localFilePath))
                using (Stream requestStream = request.GetRequestStream())
                {
                    int bytesSent = 0;
                    int count = 0;
                    int progress = 0;
                    long totalSize = fstream.Length;
                    
                    state.ResizeBufferSize(GetCorrectBufferSize(state.BUFFER_SIZE, totalSize));
                    while (true)
                    {
                        bytesSent = fstream.Read(state.buffer, 0, state.BUFFER_SIZE);

                        if (bytesSent == 0)
                            break;

                        count += bytesSent;

                        requestStream.Write(state.buffer, 0, bytesSent);

                        progress = (int)(count * 100.0 / totalSize);

                        backgroundWorker1.ReportProgress(progress);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                client.Disconnect();
            }
            e.Result = null;
        }

        private void DoDownload(FTPTask task, DoWorkEventArgs e)
        {
            //int sum = 0;
            //for (int i = 1; i <= 100; i++)
            //{
            //    Thread.Sleep(100);
            //    sum += i;

            //    backgroundWorker1.ReportProgress(i);

            //    if (backgroundWorker1.CancellationPending)
            //    {
            //        e.Cancel = true;
            //        backgroundWorker1.ReportProgress(0);
            //        return;
            //    }
            //}

            Array ftpCfg = Main.ftpInfo.ToArray();

            FTPInfo ftpInfo = new FTPInfo((string)ftpCfg.GetValue(0), (string)ftpCfg.GetValue(1),
                (string)ftpCfg.GetValue(2), (string)ftpCfg.GetValue(3));

            Array args = task.OperaionArgs.ToArray();

            string operationId = (string)args.GetValue(0);
            string remoteFilePath = (string)args.GetValue(1);
            string localFileFilePath = (string)args.GetValue(2);

            FTPClient client = new FTPClient(ftpInfo);

            bool success = client.GetRemoteFileSize(remoteFilePath);

            if (!success)
                throw client.LastErrorException;

            FTPClient.FtpState state = client.state;
            FtpWebRequest request = client.GetFtpRequest(client.GetStrUri(remoteFilePath));
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            backgroundWorker1.ReportProgress(0);

            try
            {
                state.ResizeBufferSize(GetCorrectBufferSize(state.BUFFER_SIZE, state.TotalSize));

                using (FileStream fstream = new FileStream(localFileFilePath, FileMode.Create))
                using (Stream respStream = request.GetResponse().GetResponseStream())
                {
                    int progress = 0;
                    int bytesRecv = 0;
                    long count = 0;
                    while (true)
                    {
                        // If user click the cancel button, cancel the task
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            backgroundWorker1.ReportProgress(0);
                            return;
                        }

                        bytesRecv = respStream.Read(state.buffer, 0, state.BUFFER_SIZE);

                        if (bytesRecv == 0)
                            break;

                        count += bytesRecv;

                        progress = (int)(count * 100.0 / state.TotalSize);

                        backgroundWorker1.ReportProgress(progress);

                        fstream.Write(state.buffer, 0, bytesRecv);
                    }
                }

                e.Result = localFileFilePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                client.Disconnect();
            }
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label2.Text = "进度: " + e.ProgressPercentage.ToString() + "%";
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                label2.Text = "已取消";
            }
            else if (e.Error != null)
            {
                label2.Text = "发生错误：" + e.Error.Message;
            }
            else
            {
                label1.Text = "已完成";
            }

            CancelBtn.Enabled = false;
            if (e.Result != null)
            {
                localFilePath = (string)e.Result;
                //ViewBtn.PerformClick();
            }
        }

        /// <summary>
        /// Cancel the Task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewBtn_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/select, " + Path.GetFullPath(localFilePath));
        }
    }
}
