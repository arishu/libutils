#region Using directives
using libutilscore.Common;
using libutilscore.FTP;
using libutilscore.Logging;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
#endregion

namespace libutilscore.FTP
{
    public class FTPClient
    {
        public class FtpState
        {
            /// <summary>
            /// A FtpRequest
            /// </summary>
            public FtpWebRequest Request;

            /// <summary>
            /// Local File Absolute Path
            /// </summary>
            public string LocalFilePath;

            /// <summary>
            /// The Buffer size,default is 4KB
            /// </summary>
            private static int BufferSize = 4 * 1024;

            public int BUFFER_SIZE {
                get
                {
                    return BufferSize;
                }

                private set
                {
                    BufferSize = value;
                }
            }

            /// <summary>
            /// A buffer for holding data
            /// </summary>
            public byte[] buffer = new byte[BufferSize];

            public void ResizeBufferSize(int newBufferSize)
            {
                BUFFER_SIZE = newBufferSize;
                buffer = new byte[BufferSize];
            }

            /// <summary>
            /// A string buffer
            /// </summary>
            public StringBuilder sb = new StringBuilder();

            /// <summary>
            /// Total length of the stream
            /// </summary>
            public long TotalSize { get; set; }

            /// <summary>
            /// Current Write Size
            /// </summary>
            public long TransferedBytes = 0;
        }

        #region Public Properties

        /// <summary>
        /// FTP Operations
        /// </summary>
        public enum FtpOperation
        {
            [Description("Upload Operation")]
            UPLOAD = 1,
            [Description("Download Operation")]
            DOWNLOAD
        };

        /// <summary>
        /// Set Or Get FTP Operation
        /// </summary>
        public FtpOperation Operation { get; set; }

        /// <summary>
        /// Operation args
        /// </summary>
        public ArrayList OperationArgs { get; set; }

        public FtpState state = new FtpState();

        #endregion

        #region Private Properties

        /// <summary>
        /// Base Uri
        /// </summary>
        private string Base { get; set; }

        /// <summary>
        /// The Credentials to connect to the FTP server
        /// </summary>
        private ICredentials Credential { get; set; }

        /// <summary>
        /// Current Remote Directory
        /// </summary>
        private string CurrentDir { get; set; }

        /// <summary>
        /// Last Error Message
        /// </summary>
        private string LastErrorText { get; set; }

        private FtpWebResponse response = null;

        /// <summary>
        /// Last Exception Object
        /// </summary>
        private Exception exception;

        public Exception LastErrorException
        {
            get { return exception; }
            set
            {
                LastErrorText = value.ToString();
                exception = value;
            }
        }

        private static ManualResetEvent UploadDone = new ManualResetEvent(false);

        private static ManualResetEvent DownloadDone = new ManualResetEvent(false);

        #endregion

        #region Constructor

        public FTPClient(FTPInfo ftpInfo)
        {
            Credential = new NetworkCredential(ftpInfo.User, ftpInfo.Passwd);
            Base = "ftp://" + ftpInfo.Host;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get a new Ftp request object
        /// </summary>
        /// <param name="strUri"></param>
        /// <returns></returns>
        public FtpWebRequest GetFtpRequest(string strUri)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(strUri);
            // Set Connect Timeout
            request.Timeout = 20000;    // 20 seconds
            // Default is true. Set it to false, which makes connection closed after each operation
            request.KeepAlive = false;
            // Add credentials
            request.Credentials = Credential;
            // Set read and write timeout to 20 minute
            request.ReadWriteTimeout = 2000 * 1000;
            return request;
        }

        /// <summary>
        /// Get Uri String for specific remote path
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public string GetStrUri(string strPath)
        {
            string absolutePath = ("/" + strPath);
            absolutePath = absolutePath.Replace("//", "/");
            Log.Logger.Info("The Request Uri is: {0}", Base + absolutePath);
            return Base + absolutePath;
        }

        private string NomalizeFilePath(string filePath)
        {
            return filePath.Replace("\\\\", "\\");
        }

        public string GetRemoteDirPath(string remotePath)
        {
            if (remotePath.EndsWith("/"))
            {
                return remotePath;
            }
            Log.Logger.Info("Remote Directory: {0}", remotePath.Substring(0, remotePath.LastIndexOf("/")));
            return remotePath.Substring(0, remotePath.LastIndexOf("/"));
        }

        private string GetRemoteFileName(string remotePath)
        {
            Log.Logger.Info("Remote FileName: {0}", Path.GetFileName(Base + remotePath));
            return Path.GetFileName(Base + remotePath);
        }

        /* Make Remote Directory */
        private bool MakeRemoteDir(string remoteDir)
        {
            try
            {
                FtpWebRequest request = GetFtpRequest(GetStrUri(remoteDir));
                // Try list selected directory
                request.Method = WebRequestMethods.Ftp.MakeDirectory;

                Log.Logger.Info("Making Remote Directory: {0}", remoteDir);
                response = (FtpWebResponse)request.GetResponse();
                CurrentDir = remoteDir;
                Log.Logger.Info("Successfully Making Remote Directory: {0}", remoteDir);
                return true;
            }
            catch (Exception ex) // making directory failed
            {
                Log.Logger.Error("Making Directory Failed: {0}", remoteDir);
                Log.Logger.Error("\n" + ex.StackTrace);
                LastErrorException = ex;
                return false;
            }
        }

        /// <summary>
        /// List Remote Directory
        /// </summary>
        /// <param name="remotePath">Absolute Remote Directory Path</param>
        /// <param name="createIfNotExist">Whether or not create the directory when remote directory not exists</param>
        /// <returns></returns>
        public bool ChangeRemoteDir(string remotePath, bool createIfNotExist)
        {
            if (remotePath == null)
                remotePath = "/";
            string remoteDir = GetRemoteDirPath(remotePath);

            try
            {
                FtpWebRequest request = GetFtpRequest(GetStrUri(remoteDir));
                // Try list selected directory
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                Log.Logger.Info("Entering Directory: {0}", remoteDir);
                response = (FtpWebResponse)request.GetResponse();

                CurrentDir = remoteDir;
                Log.Logger.Info("Entered Directory: {0}", remoteDir);
                return true;
            }
            catch (Exception ex) // no remote directory
            {
                if (createIfNotExist) // try to create it
                {
                    Log.Logger.Info("Failed to Enter Directory: {0}", remoteDir);
                    Log.Logger.Info("Try Making Directory: {0}", remoteDir);
                    bool success = MakeRemoteDir(remoteDir);
                    if (success)
                        return true;
                    else              // Making Directory failed
                    {
                        Disconnect();
                        return false;
                    }
                }
                else
                {
                    Log.Logger.Error("Failed to Enter Directory: {0}", remoteDir);
                    Log.Logger.Error("\n" + ex.StackTrace);
                    LastErrorException = ex;
                    Disconnect();
                    return false;
                }
            }
        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
        /// <returns>Always return true</returns>
        public bool Disconnect()
        {
            try
            {
                if (response != null)
                    response.Close();
            }
            catch (Exception ex)
            {
                LastErrorException = ex;
                Log.Logger.Error("\n" + ex.StackTrace);
            }
            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Put File To Server default path
        /// </summary>
        /// <param name="localFilePath">Absolute Local File Path</param>
        /// <returns>A Boolean value, true indicates success, otherwise failed</returns>
        private void PutFile(string localFilePath)
        {
            PutFile(localFilePath, null);
        }

        /// <summary>
        /// Put File To Server Specified Path
        /// </summary>
        /// <param name="localFilePath">Absolute Local File Path</param>
        /// <param name="remotePath">Absolute Remote Save Directory Path or Save File Path</param>
        /// <returns>A Boolean value, true indicates success, otherwise failed</returns>
        private bool PutFile(string localFilePath, string remotePath)
        {
            try
            {
                FileInfo fileInf = new FileInfo(localFilePath);
                string StrFilePath = "";
                if (remotePath == null)
                    StrFilePath += fileInf.Name;
                else
                {
                    StrFilePath += (GetRemoteDirPath(remotePath) + "/" + fileInf.Name);
                }

                FtpWebRequest request = GetFtpRequest(GetStrUri(StrFilePath));
                Log.Logger.Info("Starting uploading file: {0} ...", fileInf.Name);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                // set type to image 'i'
                request.UseBinary = true;
                // Set local client to passive
                request.UsePassive = true;

                ////get the file size and set remote Content Length
                //FileStream instream = File.OpenRead(localFilePath);
                //byte[] fileContents = new byte[instream.Length];
                //instream.Read(fileContents, 0, fileContents.Length);
                //instream.Close();
                //request.ContentLength = fileContents.Length;

                Log.Logger.Info("Writing to remote ...");

                state = new FtpState
                {
                    Request = request,
                    LocalFilePath = localFilePath
                };

                UploadDone.Reset();

                request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), state);

                UploadDone.WaitOne();

                Log.Logger.Info("Successfully Upload file {0} to remote {1} ", Path.GetFileName(localFilePath), state.Request.RequestUri);

                // Get Out Stream and Write content to the ftp stream
                //Stream requestStream = request.GetRequestStream();
                //requestStream.BeginWrite(fileContents, 0, fileContents.Length, PutFileCallback, requestStream);
                //requestStream.Write(fileContents, 0, fileContents.Length);
                //requestStream.Close();
                //Log.Logger.Info("Writing Done!");

                //Log.Logger.Info("Getting response ...");

                //response = (FtpWebResponse)request.GetResponse();
                //Log.Logger.Info("Successfully Upload file {0} to remote {1} ", Path.GetFileName(localFilePath), response.ResponseUri);
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Failed to Upload file {0}: {1}", Path.GetFileName(localFilePath), ex.Message);
                Log.Logger.Error("\n" + ex.StackTrace);
                LastErrorException = ex;
                return false;
            }
            finally
            {
                Disconnect();
                UploadDone.Set();
            }
        }

        private void GetRequestStreamCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;
            FtpWebRequest request = state.Request;
            try
            {
                int sentBytes;
                using (Stream requestStream = request.EndGetRequestStream(ar))
                using (FileStream instream = File.OpenRead(state.LocalFilePath))
                {
                    request.ContentLength = instream.Length;
                    state.TotalSize = instream.Length;
                    do
                    {
                        sentBytes = instream.Read(state.buffer, 0, FtpState.BufferSize);

                        state.TransferedBytes += sentBytes; // set transfered bytes

                        requestStream.BeginWrite(state.buffer, 0, sentBytes, SendCallback, requestStream);

                    } while (sentBytes != 0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                ((Stream)ar.AsyncState).EndWrite(ar);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void EndGetStreamCallback(IAsyncResult ar)
        //{
        //    FtpState state = (FtpState)ar.AsyncState;
        //    FtpWebRequest request = state.Request;

        //    Stream requestStream = null;

        //    try
        //    {
        //        requestStream = state.Request.EndGetRequestStream(ar);
        //        const int bufferLength = 2048;
        //        byte[] buffer = new byte[bufferLength];
        //        int count = 0;
        //        int readBytes = 0;

        //        using (FileStream readStream = File.OpenRead(state.LocalFilePath))
        //        {
        //            do
        //            {
        //                readBytes = readStream.Read(buffer, 0, bufferLength);
        //                readStream.Write(buffer, 0, readBytes);
        //                count += readBytes;
        //            } while (readBytes != 0);

        //            Log.Logger.Debug("Writing {0} bytes to the ftp server", count);
        //        }

        //        requestStream.Close();

        //        state.Request.BeginGetResponse(EndGetResponseCallback, state);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Error("Could not get the request stream: {0}", ex.StackTrace);
        //        state.OperationException = ex;
        //        state.StatusDescription = ex.Message;
        //        LastErrorException = ex;
        //    }
        //}

        //private void EndGetResponseCallback(IAsyncResult ar)
        //{
        //    FtpState state = (FtpState)ar.AsyncState;
        //    FtpWebResponse response = null;

        //    try
        //    {
        //        response = (FtpWebResponse)state.Request.EndGetResponse(ar);
        //        response.Close();
        //        state.StatusDescription = response.StatusDescription;
        //    }
        //    // Return exceptions to the main application thread.
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Error("[Upload]Error getting response.");
        //        state.OperationException = ex;
        //    }
        //    finally
        //    {
        //        // Signal the main application thread that 
        //        // the operation is complete.
        //        state.UploadDone.Set();
        //    }
        //}

        /// <summary>
        /// Get Remote File's Size
        /// </summary>
        /// <param name="remoteFilePath"></param>
        /// <returns></returns>
        public bool GetRemoteFileSize(string remoteFilePath)
        {
            string remoteFileName = GetRemoteFileName(remoteFilePath);

            try
            {
                Log.Logger.Info("Starting Calculate Remote File's size");

                FtpWebRequest request = GetFtpRequest(GetStrUri(remoteFilePath));
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.UseBinary = true;

                response = (FtpWebResponse)request.GetResponse();

                long size = response.ContentLength;

                state.TotalSize = size;

                Log.Logger.Info("Remote File {0} size: {1}", remoteFileName, size);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Get Remote File Size failed: {0}", ex.StackTrace);
                LastErrorException = ex;
                return false;
            }
        }


        /// <summary>
        /// Get File From Server
        /// </summary>
        /// <param name="remoteFilePath">Absolute Remote File Path</param>
        /// <param name="localFilePath">Absolute Local Save Path, optional containing local file name</param>
        /// <returns>A Boolean value, true indicates success, otherwise failed</returns>
        private bool GetFile(string remoteFilePath, string localFilePath)
        {
            string remoteFileName = GetRemoteFileName(remoteFilePath);
            try
            {
                Log.Logger.Info("Starting download remote file: {0}", remoteFileName);

                state = new FtpState();

                FtpWebRequest request = GetFtpRequest(GetStrUri(remoteFilePath));
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                //response = (FtpWebResponse)request.GetResponse();

                // get filename from localFilePath
                string localFileName = Path.GetFileName(localFilePath);
                if (localFileName == String.Empty) // if localFilePath is a directory
                {
                    localFilePath += ("\\" + remoteFileName);
                }

                // Get remote file's size
                bool success = GetRemoteFileSize(remoteFilePath);
                if (!success)
                    throw LastErrorException;

                state.Request = request;
                state.LocalFilePath = localFilePath;
                state.TransferedBytes = 0;

                DownloadDone.Reset();

                request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);

                DownloadDone.WaitOne();

                //byte[] buffer = new byte[2048];
                //int byteRead = 0;
                //using (Stream reader = response.GetResponseStream())
                //using (Stream fstream = new FileStream(localFilePath, FileMode.Create))
                //{
                //    while (true)
                //    {
                //        byteRead = reader.Read(buffer, 0, buffer.Length);
                //        if (byteRead == 0)
                //            break;
                //        fstream.Write(buffer, 0, byteRead);
                //    }
                //}
                Log.Logger.Info("Done!");
                Log.Logger.Info("Successfully Download file {0} from server!", remoteFileName);
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Failed to Download file {0}: {1}", remoteFileName, ex.Message);
                Log.Logger.Error("\n" + ex.StackTrace);
                LastErrorException = ex;
                Disconnect();
                return false;
            }
        }

        private void GetResponseCallback(IAsyncResult ar)
        {
            try
            {
                //FtpState state = (FtpState)ar.AsyncState;
                string localFilePath = state.LocalFilePath;
                Log.Logger.Info("Writing to local file: {0}", localFilePath);

                using (Stream respStream = ((FtpWebResponse)state.Request.EndGetResponse(ar)).GetResponseStream())
                using (FileStream fsStream = new FileStream(localFilePath, FileMode.Create))
                {
                    int bytesRecv = 0;
                    while (true)
                    {
                        bytesRecv = respStream.Read(state.buffer, 0, FtpState.BufferSize);

                        if (bytesRecv == 0)
                            break;

                        state.TransferedBytes += bytesRecv;

                        fsStream.Write(state.buffer, 0, bytesRecv);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Receive data error: {0}", ex.StackTrace);
                throw ex;
            }
            finally
            {
                DownloadDone.Set();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Current Not Used")]
        private void UploadFileUsingWebClient(string localFilePath)
        {
            WebClient client = new WebClient
            {
                Credentials = Credential
            };
            client.UploadFile(Base + "/" + Path.GetFileName(localFilePath), localFilePath);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public Tuple<bool, string> HandleOperation(object obj)
        {
            Log.Logger.Info("Starting Do FTPOperation...");

            Tuple<bool, string> result = Tuple.Create(false, "");

            Array paramArray = ((ArrayList)obj).ToArray();
            object operationId = paramArray.GetValue(0);

            Array argArray = OperationArgs.ToArray();
            bool success = false;
            try
            {
                switch (Operation)
                {
                    case FtpOperation.UPLOAD:
                        {
                            if (argArray.Length == 3)
                            {
                                success = ChangeRemoteDir((string)argArray.GetValue(1), (bool)argArray.GetValue(2));
                                if (!success)
                                    throw LastErrorException;

                                success = PutFile((string)argArray.GetValue(0), (string)argArray.GetValue(1));

                                if (!success)
                                    throw LastErrorException;

                                success = Disconnect();
                            }
                            else
                            {
                                throw new ArgumentException("Too many arguments for Upload Fuction.");
                            }
                            break;
                        }
                    case FtpOperation.DOWNLOAD:
                        {
                            if (argArray.Length == 2)
                            {
                                success = GetFile((string)argArray.GetValue(0), (string)argArray.GetValue(1));
                                if (!success)
                                    throw LastErrorException;

                                success = Disconnect();
                            }
                            else
                            {
                                throw new ArgumentException("Too many arguments for Download Fuction.");
                            }
                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException("Unsupported FTP Operation" + EnumHelper.GetEnumDescription(Operation));
                        }
                }
            }
            catch (Exception ex)
            {
                success = false;
                LastErrorException = ex;
            }

            result = Tuple.Create(success, LastErrorText);

            // Add Result to Global Holder
            //Results.AddExecResult(operationId, result);

            return result;
        }
    }

    #endregion
}