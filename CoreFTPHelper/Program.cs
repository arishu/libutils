using System;
using System.Collections;
using System.Configuration;
using libutilscore.Logging;

namespace CoreFTPHelper
{
    class Program
    {
        public static ArrayList ftpInfo = new ArrayList
            {
                "127.0.0.1", "anonymous",
                "V1ZjMWRtSnViSFJpTTFaNlVVUkZlVTU1TkhkTWFrRjFUVkU5UFE9PQ ==", "/" };
        static void Main(string[] args)
        {
            try
            {
                // read configuration, and save ftp information
                string ftpHost       = ConfigurationManager.AppSettings["host"];
                string ftpUser       = ConfigurationManager.AppSettings["user"];
                string ftpPasswd     = ConfigurationManager.AppSettings["passwd"];
                string ftpRemotePath = ConfigurationManager.AppSettings["remotePath"];
                if (ftpRemotePath == "")
                    ftpRemotePath = "/";
                ftpInfo = new ArrayList { ftpHost, ftpUser, ftpPasswd, ftpRemotePath };

                DefaultStart();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Server Exception: {0}", ex.StackTrace);
            }
        }

        static void DefaultStart()
        {
            Log.Logger.Debug("Start Server in default");
            TcpServer server = new TcpServer(55505);
            server._StartSocket();
        }
    }
}
