#region Using directives
using System;
using System.Collections;
using libutilscore.FTP;
using libutilscore.Logging;
#endregion

namespace CoreFTPHelper
{
    class FTPTask
    {
        public string Operation { get; set; }
        public ArrayList OperaionArgs { get; set; }

        public FTPTask()
        {
            OperaionArgs = new ArrayList();
        }

        private static ArrayList ValidOperations = new ArrayList { "get", "put" };

        public void Exec()
        {
            Array ftpInfo = Program.ftpInfo.ToArray();
            SharpFTP.SetFtpInfo((string)ftpInfo.GetValue(0), (string)ftpInfo.GetValue(1),
                (string)ftpInfo.GetValue(2), (string)ftpInfo.GetValue(3));
            Array args = OperaionArgs.ToArray();
            string operation = Operation;
            string operationId = (string)args.GetValue(0);
            try
            {
                if (ValidOperations.Contains(operation))
                {
                    switch (operation)
                    {
                        case "put":
                            {
                                Tuple<bool, string> result = SharpFTP.UploadToRemote(operationId,
                                    (string)args.GetValue(1), (string)args.GetValue(2), bool.Parse((string)args.GetValue(3)));

                                lock (TcpServer.ResultLockObj)
                                {
                                    TcpServer.ResultsTable.Add(operationId, result);
                                }
                                break;
                            }
                        case "get":
                            {
                                Tuple<bool, string> result = SharpFTP.DownloadFromRemote(operationId,
                                    (string)args.GetValue(1), (string)args.GetValue(2));
                                lock (TcpServer.ResultLockObj)
                                {
                                    TcpServer.ResultsTable.Add(operationId, result);
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                    Log.Logger.Info("Successfully Execute Command");
                }
                else
                {
                    Log.Logger.Error("Unsupported Command: {0}", operation);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Error occurred when executing the task: {0}", ex.Message);
                throw ex;
            } finally
            {
                TcpServer.executeDone.Set();
            }
        }
    }
}