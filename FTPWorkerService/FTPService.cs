#region Using Directives
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
#endregion

namespace FTPWorkerService
{
    public partial class FTPService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        Process main = null;

        private static ApplicationLoader.PROCESS_INFORMATION procInfo;

        private static string WORK_DIR = @"D:\workspace\github\libutils\FTPWorker\bin\x64\Debug";
        private static string MAIN_PROC_NAME = "FTPWorker.exe";

        public FTPService()
        {
            InitializeComponent();

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("MySource"))
            {
                EventLog.CreateEventSource(
                    "MySource", "FTPWS");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "FTPWS";
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Stop Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("Starting FTP Worker Service");

            //backgroundWorker1.RunWorkerAsync();
            ApplicationLoader.StartProcessAndBypassUAC(Path.GetFullPath(WORK_DIR + @"\" + MAIN_PROC_NAME), out procInfo);

            // Update the service state to Stopped.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        private void RunApplication(/*object sender, DoWorkEventArgs e*/)
        {
            
            main = new Process();
            main.StartInfo.FileName = MAIN_PROC_NAME;
            main.StartInfo.WorkingDirectory = WORK_DIR;
            main.StartInfo.ErrorDialog = true;

            main.Start();

            //main.WaitForExit();
        }

        protected override void OnStop()
        {
            // Kill the main thread
            //main.Kill();

            Process.GetProcessById((int)procInfo.dwProcessId).Kill();


            // Update the service state to Stop Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("Stopping FTP Worker Service");

            // Update the service state to Stopped.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
    }
}