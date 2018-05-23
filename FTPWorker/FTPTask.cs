#region Using directives
using System;
using System.Collections;
using libutilscore.FTP;
using libutilscore.Logging;
using libutilscore.Core;
#endregion

namespace FTPWorker
{
    public class FTPTask
    {
        public string Operation { get; set; }
        public ArrayList OperaionArgs { get; set; }

        public FTPTask()
        {
            OperaionArgs = new ArrayList();
        }
    }
}