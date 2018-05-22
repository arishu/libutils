#region Using directives
using System;
using System.Diagnostics;
using libutilscore.Logging;
#endregion

namespace libutilscore.Core
{
    public class SharpProcess
    {

        /// <summary>
        /// Create a new Process
        /// </summary>
        /// <param name="fileName">Program's name or absolute path</param>
        /// <param name="showHide">true to create window</param>
        /// <param name="windowStyle">0:background 1:minimize 2:normal 3:maximize</param>
        /// <param name="args"></param>
        public static Tuple<bool, string> Create(string fileName, bool showHide,
            int windowStyle, string args)
        {
            Log.Logger.Info("Program:{0}, create window: {1}, windowStyle: {2}, args: {3}",
                fileName, showHide, windowStyle, args);
            bool success = false;
            string errMsg = "";
            Process newProcess = new Process();
            try
            {
                newProcess.StartInfo.UseShellExecute = false;
                newProcess.StartInfo.FileName = fileName;
                //string workingDir = new FileInfo();
                newProcess.StartInfo.WorkingDirectory = @"C:\Program Files\Lua";
                newProcess.StartInfo.CreateNoWindow = true;
                newProcess.StartInfo.Arguments = args;
                //newProcess.StartInfo.RedirectStandardOutput = true;

                switch (windowStyle)
                {
                    case 1:
                        {
                            newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            break;
                        }
                    case 2:
                        {
                            newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                            break;
                        }
                    case 3:
                        {
                            newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                            break;
                        }
                    default:
                        {
                            newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                            break;
                        }
                }

                newProcess.Start();

                Log.Logger.Info("Started a new Process");

                success = true;
                errMsg = newProcess.Id.ToString();
            }
            catch (Exception ex)
            {
                success = false;
                errMsg = ex.Message;
                Log.Logger.Error(ex.ToString());
            }

            return Tuple.Create(success, errMsg);
        }
    }
}
