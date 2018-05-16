
using System;
using libutilscore.Logging;
using libutilscore.Common;

namespace libutilscore.FTP
{
    public class SharpFTP
    {
        private FTPInfo ftpInfo = null;

        public string ShowHello()
        {
            return "Hello From C Sharp.";
        }

        public Tuple<bool, string> SetFtpInfo(string host, string user, string passwd, string remotePath)
        {
            Log.logger.Info("FTP: host={0}, user={1}, passwd={2}, remotePath={3}", host, user, passwd, remotePath);
            if (ftpInfo == null)
                ftpInfo = new FTPInfo(host, user, passwd);
            if (remotePath != null && !remotePath.Equals(""))
                ftpInfo.RemotePath = remotePath;
            return Results.GetResultsTuple(true);
        }

        /* upload file to ftp */
        public Tuple<bool, string> UploadToRemote(string localFilePath, string remotePath, bool createIfNotExist)
        {
            FTPClient client = new FTPClient(ftpInfo);
            try
            {
                bool success = client.ChangeRemoteDir(remotePath, createIfNotExist);
                if (!success)
                    throw client.LastErrorException;

                success = client.PutFile(localFilePath, remotePath);

                if (!success)
                    throw client.LastErrorException;

                success = client.Disconnect();
            }
            catch (Exception ex)
            {
                return Results.GetResultsTuple(false, ex);
            }
            finally
            {
                client.Disconnect();
            }
            return Results.GetResultsTuple(true);
        }

        /* download file from remote server */
        public Tuple<bool, string> DownloadFromRemote(string remotePath, string localFilePath)
        {
            FTPClient client = new FTPClient(ftpInfo);
            try
            {
                bool success = client.GetFile(remotePath, localFilePath);
                if (!success)
                    throw client.LastErrorException;

                success = client.Disconnect();
            }
            catch (Exception ex)
            {
                return Results.GetResultsTuple(false, ex);
            }
            finally
            {
                client.Disconnect();
            }
            return Results.GetResultsTuple(true);
        }
    }
}
