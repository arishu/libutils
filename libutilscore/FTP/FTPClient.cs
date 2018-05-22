#region Using directives
using System;
using System.Net;
using System.IO;
using System.Collections;
using System.ComponentModel;
using libutilscore.Logging;
using libutilscore.Common;
#endregion

namespace libutilscore.FTP
{
    public class FTPClient
    {
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

        private Exception LastErrorException
        {
            get { return exception; }
            set
            {
                LastErrorText = value.ToString();
                exception = value;
            }
        }

        #endregion

        #region Constructor

        public FTPClient(FTPInfo ftpInfo)
        {
            Credential = new NetworkCredential(ftpInfo.User, ftpInfo.Passwd);
            Base = "ftp://" + ftpInfo.Host;
        }

        #endregion

        #region Private Methods

        private FtpWebRequest GetFtpRequest(string strUri)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(strUri);
            // Set Connect Timeout
            request.Timeout = 20000;    // 20 seconds
            // Default is true. Set it to false, which makes connection closed after each operation
            request.KeepAlive = false;
            // Add credentials
            request.Credentials = Credential;
            return request;
        }

        private string GetStrUri(string strPath)
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

        private string GetRemoteDirPath(string remotePath)
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
        private bool ChangeRemoteDir(string remotePath, bool createIfNotExist)
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
        public bool PutFile(string localFilePath)
        {
            return PutFile(localFilePath, null);
        }

        /// <summary>
        /// Put File To Server Specified Path
        /// </summary>
        /// <param name="localFilePath">Absolute Local File Path</param>
        /// <param name="remotePath">Absolute Remote Save Directory Path or Save File Path</param>
        /// <returns>A Boolean value, true indicates success, otherwise failed</returns>
        public bool PutFile(string localFilePath, string remotePath)
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


                //byte[] fileContents;
                //StreamReader sourceStream = new StreamReader(localFilePath);
                //fileContents = Encoding.Default.GetBytes(sourceStream.ReadToEnd());

                // get the file size and set remote Content Length
                FileStream instream = File.OpenRead(localFilePath);
                byte[] fileContents = new byte[instream.Length];
                instream.Read(fileContents, 0, fileContents.Length);
                instream.Close();
                request.ContentLength = fileContents.Length;

                Log.Logger.Info("Writing to remote ...");
                // Get Out Stream and Write content to the ftp stream
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();
                Log.Logger.Info("Writing Done!");

                Log.Logger.Info("Getting response ...");

                response = (FtpWebResponse)request.GetResponse();
                Log.Logger.Info("Successfully Upload file {0} to remote {1} ", Path.GetFileName(localFilePath), response.ResponseUri);
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Failed to Upload file {0}: {1}", Path.GetFileName(localFilePath), ex.Message);
                Log.Logger.Error("\n" + ex.StackTrace);
                LastErrorException = ex;
                Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Get File From Server
        /// </summary>
        /// <param name="remoteFilePath">Absolute Remote File Path</param>
        /// <param name="localFilePath">Absolute Local Save Path, optional containing local file name</param>
        /// <returns>A Boolean value, true indicates success, otherwise failed</returns>
        public bool GetFile(string remoteFilePath, string localFilePath)
        {
            string remoteFileName = GetRemoteFileName(remoteFilePath);
            try
            {
                Log.Logger.Info("Starting download remote file: {0}", remoteFileName);

                FtpWebRequest request = GetFtpRequest(GetStrUri(remoteFilePath));
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                response = (FtpWebResponse)request.GetResponse();

                // get filename from localFilePath
                string localFileName = Path.GetFileName(localFilePath);
                if (localFileName == String.Empty) // if localFilePath is a directory
                {
                    localFilePath += ("\\" + remoteFileName);
                }

                Log.Logger.Info("Writing to local file: {0}", localFilePath);
                byte[] buffer = new byte[2048];
                int byteRead = 0;
                using (Stream reader = response.GetResponseStream())
                using (Stream fstream = new FileStream(localFilePath, FileMode.Create))
                {
                    while (true)
                    {
                        byteRead = reader.Read(buffer, 0, buffer.Length);
                        if (byteRead == 0)
                            break;
                        fstream.Write(buffer, 0, byteRead);
                    }
                }
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

        /// <summary>
        /// Current Not Used
        /// </summary>
        public void UploadFileUsingWebClient(string localFilePath)
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