#region Using directives
using System;
using System.Text;
using System.Collections;
using System.Net.Sockets;
using libutilscore.Logging;
#endregion

namespace libutilscore.FTP
{
    public class SharpFTP
    {
        private static FTPInfo ftpInfo = null;

        public static string ShowHello()
        {
            return "Hello From C Sharp.";
        }

        private static string[] ParseResponse(string response)
        {
            string[] retValue = { "False", "No result" };
            object[] parsedArr = response.Split(',');
            if (parsedArr.Length == 2)
            {
                retValue[0] = parsedArr[0].ToString();
                retValue[1] = parsedArr[1].ToString();
            }
            return retValue;
        }

        /// <summary>
        /// Get Execute Result
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        public static Tuple<bool, string> GetExecResult(string operationId)
        {
            Tuple<bool, string> ret = Tuple.Create(false, "没有获取到执行结果");

            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect("127.0.0.1", 55505);

                // send command to the server
                string command = "resp," + operationId;
                socket.Send(Encoding.UTF8.GetBytes(command));

                // Receive execute result, transform to string format
                byte[] buffer = new byte[256];
                int byteRecv = socket.Receive(buffer);
                string responseStr = Encoding.UTF8.GetString(buffer);

                string[] result = ParseResponse(responseStr);

                ret = Tuple.Create(bool.Parse(result[0]), result[1]);
            } catch (Exception ex)
            {
                Log.Logger.Error("Get Execution Result failed: {0}", ex.ToString());
                ret = Tuple.Create(false, ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// FTP Server Configuration
        /// </summary>
        /// <param name="host">The host name or ip</param>
        /// <param name="user">The user name</param>
        /// <param name="passwd">The user's passwd</param>
        /// <param name="remotePath">Remote Server's Initial Path</param>
        /// <returns></returns>
        public static Tuple<bool, string> SetFtpInfo(string host, string user, string passwd, string remotePath)
        {
            Log.Logger.Info("FTP: host={0}, user={1}, passwd={2}, remotePath={3}", host, user, passwd, remotePath);
            ftpInfo = new FTPInfo(host, user, passwd);
            if (remotePath != null && !remotePath.Equals(""))
                ftpInfo.RemotePath = remotePath;
            return Tuple.Create(true, "");
        }

        /// <summary>
        /// Upload file to FTP Server
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="localFilePath"></param>
        /// <param name="remotePath"></param>
        /// <param name="createIfNotExist"></param>
        /// <returns></returns>
        public static Tuple<bool, string> UploadToRemote(string operationId, string localFilePath, string remotePath, bool createIfNotExist)
        {
            try
            {
                ArrayList argList = new ArrayList(3) {
                    localFilePath,
                    remotePath,
                    createIfNotExist
                };

                FTPClient client = new FTPClient(ftpInfo)
                {
                    Operation = FTPClient.FtpOperation.UPLOAD,
                    OperationArgs = argList
                };

                ArrayList paramList = new ArrayList() { operationId };

                return client.HandleOperation(paramList);
            } catch (Exception ex)
            {
                return Tuple.Create(false, ex.Message);
            }
        }

        /// <summary>
        /// Download file from Remote Server
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="localFilePath"></param>
        /// <returns></returns>
        public static Tuple<bool, string> DownloadFromRemote(string operationId, string remotePath, string localFilePath)
        {
            try
            {
                ArrayList argList = new ArrayList(2) {
                    remotePath,
                    localFilePath
                };
                FTPClient client = new FTPClient(ftpInfo)
                {
                    Operation = FTPClient.FtpOperation.DOWNLOAD,
                    OperationArgs = argList
                };

                ArrayList paramList = new ArrayList() { operationId };

                return client.HandleOperation(paramList);
            } catch (Exception ex)
            {
                return Tuple.Create(false, ex.ToString());
            }
        }
    }
}
