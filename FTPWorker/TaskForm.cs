#region Using Directives
using libutilscore.FTP;
using libutilscore.Logging;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
#endregion

namespace FTPWorker
{
    public partial class TaskForm : Form
    {
        //private static ArrayList ValidOperations = new ArrayList { "get", "put" };
        private string filePath = null;

        public TaskForm(FTPTask task)
        {
            InitializeComponent();

            string operation = task.Operation;
            if (operation == "get")
            {
                string localFilePath = (string)task.OperaionArgs.ToArray().GetValue(2);
                label1.Text = "下载：" + Path.GetFileName(localFilePath);
                ViewBtn.Visible = true;
                ViewBtn.Enabled = false;
            }
            else if (operation == "put")
            {
                string localFilePath = (string)task.OperaionArgs.ToArray().GetValue(1);
                label1.Text = "上传：" + Path.GetFileName(localFilePath);
                ViewBtn.Visible = false;
            }

            // Start task asynchronously
            backgroundWorker1.RunWorkerAsync(task);
        }

        protected override void OnLoad(EventArgs e)
        {
            PlaceLowerRight();
            base.OnLoad(e);
        }

        /// <summary>
        /// Make form show at the bottom corner
        /// </summary>
        private void PlaceLowerRight()
        {
            //Determine "rightmost" screen

            // Get First Screen
            Screen rightmost = Screen.AllScreens[0];

            //Screen mainScreen = Screen.PrimaryScreen;

            // Iterate each screen, 
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                    rightmost = screen;
            }

            this.Left = rightmost.WorkingArea.Right - this.Width;
            this.Top = rightmost.WorkingArea.Bottom - this.Height;
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

        /// <summary>
        /// Get Correct Buffer Size 
        /// </summary>
        /// <param name="originnal"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        private int GetCorrectBufferSize(int originnal, long totalSize)
        {
            int adjustedBufferSize = 1024;
            int times = (int)(Math.Sqrt(totalSize) / originnal);
            Log.Logger.Debug("File total size is {0} times than the buffer size", times);
            if (times > 10)
            {
                adjustedBufferSize = originnal * 10;
            }
            else if (times >= 4)
            {
                adjustedBufferSize = originnal;
            }
            else
            {
                adjustedBufferSize = 1024;
            }
            Log.Logger.Info("The used buffer size is {0} bytes", adjustedBufferSize);
            return adjustedBufferSize;
        }

        /// <summary>
        /// Do Upload
        /// </summary>
        /// <param name="task"></param>
        /// <param name="e"></param>
        private void DoUpload(FTPTask task, DoWorkEventArgs e)
        {
            FTPClient client = null;
            try
            {
                Array ftpCfg = Main.ftpInfo.ToArray();
                Array args = task.OperaionArgs.ToArray();

                string operationId = (string)args.GetValue(0);
                string localFilePath = (string)args.GetValue(1);
                string remotePath = (string)args.GetValue(2);
                bool createRemotePath = bool.Parse((string)args.GetValue(3));

                // Create a ftpinfo object and get a FTPClient
                FTPInfo ftpInfo = new FTPInfo((string)ftpCfg.GetValue(0), (string)ftpCfg.GetValue(1),
                    (string)ftpCfg.GetValue(2), (string)ftpCfg.GetValue(3));
                client = new FTPClient(ftpInfo)
                {
                    Operation = FTPClient.FtpOperation.UPLOAD,
                    OperationArgs = new ArrayList { (string)args.GetValue(1), (string)args.GetValue(2), (string)args.GetValue(3) }
                };

                // Try change remote directory, if createRemotePath is set true,
                // will create remote path
                bool success = client.ChangeRemoteDir(remotePath, createRemotePath);
                if (!success)
                    throw client.LastErrorException;

                // Try adjust local file path
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
                state.FileName = fileInf.Name;

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

                        // If user click the cancel button, cancel the task
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            backgroundWorker1.ReportProgress(0);
                            return;
                        }

                        count += bytesSent;

                        requestStream.Write(state.buffer, 0, bytesSent);

                        progress = (int)(count * 100.0 / totalSize);

                        backgroundWorker1.ReportProgress(progress);
                    }
                }

                Log.Logger.Info("Successfully upload file {0} to server", state.FileName);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Upload File Failed: {0}", ex.StackTrace);
                throw ex;
            }
            finally
            {
                if (client != null)
                    client.Disconnect();

                e.Result = null;
            }
        }

        /// <summary>
        /// Do Download Operation
        /// </summary>
        /// <param name="task"></param>
        /// <param name="e"></param>
        private void DoDownload(FTPTask task, DoWorkEventArgs e)
        {
#if __DEBUG
            int sum = 0;
            for (int i = 1; i <= 100; i++)
            {
                Thread.Sleep(100);
                sum += i;

                backgroundWorker1.ReportProgress(i);

                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    backgroundWorker1.ReportProgress(0);
                    return;
                }
            }
#else
            FTPClient client = null;
            try
            {
                Array ftpCfg = Main.ftpInfo.ToArray();
                Array args = task.OperaionArgs.ToArray();

                // Get parameters
                string operationId = (string)args.GetValue(0);
                string remoteFilePath = (string)args.GetValue(1);
                string localFilePath = (string)args.GetValue(2);

                // Create a ftpinfo object and Get a FTP client
                FTPInfo ftpInfo = new FTPInfo((string)ftpCfg.GetValue(0), (string)ftpCfg.GetValue(1),
                    (string)ftpCfg.GetValue(2), (string)ftpCfg.GetValue(3));
                client = new FTPClient(ftpInfo);

                // Get the client operation State
                FTPClient.FtpState state = client.state;
                state.FileName = client.GetRemoteFileName(remoteFilePath);

                // Try Get Remote File's Total Size, 
                // its total size will be store in client state property 
                bool success = client.GetRemoteFileSize(remoteFilePath);
                if (!success)
                    throw client.LastErrorException;

                // Resize the state's buffer size according to the file's total size
                state.ResizeBufferSize(GetCorrectBufferSize(state.BUFFER_SIZE, state.TotalSize));
                Log.Logger.Info("Try using Buffer size: {0} bytes", state.BUFFER_SIZE);

                Log.Logger.Info("Starting Download file {0} from remote server", state.FileName);

                // Try to download the file
                FtpWebRequest request = client.GetFtpRequest(client.GetStrUri(remoteFilePath));
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                using (FileStream fstream = new FileStream(localFilePath, FileMode.Create))
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
                            filePath = localFilePath;
                            return;
                        }

                        // read state.BUFFER_SIZE bytes from the response stream
                        // and save them to the state's buffer
                        bytesRecv = respStream.Read(state.buffer, 0, state.BUFFER_SIZE);

                        // if get no bytes, quit the loop 
                        if (bytesRecv == 0)
                            break;

                        // Write bytes in the buffer to the file stream
                        fstream.Write(state.buffer, 0, bytesRecv);

                        // Calculate the transferred bytes
                        count += bytesRecv;

                        // Report progress
                        progress = (int)(count * 100.0 / state.TotalSize);
                        backgroundWorker1.ReportProgress(progress);
                    }
                }

                Log.Logger.Info("Successfully Download file {0} from the server", state.FileName);
                e.Result = localFilePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (client != null)
                    client.Disconnect();
            }
#endif
        }

        /// <summary>
        /// BackgroundWorker's Progress Report Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label2.Text = "进度: " + e.ProgressPercentage.ToString() + "%";
        }

        /// <summary>
        /// BackgroundWorker's Completed Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Note: when user cancelled the operation or error occurred,
            // do not use e.Result
            CancelBtn.Enabled = false;
            if (e.Cancelled)
            {
                label2.Text = "已取消";
                label2.ForeColor = Color.Red;
                label2.Font = new Font("微软雅黑", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)134));
                ViewBtn.Visible = false;
                DeleteLocalFile();
            }
            else if (e.Error != null)
            {
                label2.Text = "发生错误：" + e.Error.Message;
                label2.ForeColor = Color.Red;
                label2.Font = new Font("微软雅黑", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)134));
                ViewBtn.Visible = false;
            }
            else
            {
                label1.Text = "已完成";
                label1.Font = new Font("微软雅黑", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)134));
                if (e.Result != null)
                {
                    filePath = (string)e.Result;
                    ViewBtn.Enabled = true;
                    ViewBtn.PerformClick();
                }
            }
            
        }

        /// <summary>
        /// Delete cancelled local file
        /// </summary>
        private void DeleteLocalFile()
        {
            if (filePath != null)
                File.Delete(Path.GetFullPath(filePath));
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
        /// View File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewBtn_Click(object sender, EventArgs e)
        {
            if (filePath != null)
            {
                string localFilePath = Path.GetFullPath(filePath);
                string fileExt = Path.GetExtension(localFilePath).ToLower();

                switch (fileExt)
                {
                    case ".jpg":case ".jpe":case ".jpeg":
                    case ".png":case ".bmp":
                    case ".ico":
                    case ".tif":case ".tiff":
                    case ".gif":
                        {
                            Process.Start("explorer.exe ", localFilePath);
                            Log.Logger.Info("Open Picture Command: {0}", "explorer.exe " + localFilePath);
                            break;
                        }
                    default:
                        {
                            string args = " /select, " + localFilePath;
                            Process.Start("explorer.exe", args);
                            Log.Logger.Info("Open File Command: {0}", "explorer.exe " + args);
                            break;
                        }
                }
            }
        }
    }
}
