#region Using Directives
using System;
using System.ComponentModel;
using System.Configuration;
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

        private static ApplicationLoader.PROCESS_INFORMATION procInfo;

        private static string WORK_DIR = @"C:\";
        private static string MAIN_PROC_NAME = "FTPWorker.exe";

        public FTPService(string[] args)
        {
            InitializeComponent();

            // Get settings from App.config file
            ServiceName = ConfigurationManager.AppSettings["ServiceName"];
            WORK_DIR = ConfigurationManager.AppSettings["WorkDir"];

            // Set Event log's source and log name
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("FTPWorker"))
            {
                EventLog.CreateEventSource(
                    "FTPWorker", "FTPWS");
            }
            eventLog1.Source = "FTPWorker";
            eventLog1.Log = "FTPWS";
        }

        /// <summary>
        /// Start the service
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // Update the service state to Stop Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("Starting FTP Worker Service");

            // Use CreateProcessAsUser method to create the main thread
            ApplicationLoader.StartProcessAndBypassUAC(Path.GetFullPath(WORK_DIR + @"\" + MAIN_PROC_NAME), out procInfo);

            // Update the service state to Stopped.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        protected override void OnStop()
        {
            // Update the service state to Stop Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Kill main process
            Process.GetProcessById((int)procInfo.dwProcessId).Kill();

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
            // The type of service(服务的类型)
            //      SERVICE_FILE_SYSTEM_DRIVER   文件系统驱动服务(0x00000002)
            //      SERVICE_KERNEL_DRIVER        设备驱动服务(0x00000001)
            //      SERVICE_WIN32_OWN_PROCESS    服务在自己的进程中运行(0x00000010)
            //      SERVICE_WIN32_SHARE_PROCESS  服务与其他服务共享一个进程(0x00000020)
            //      SERVICE_USER_OWN_PROCESS     服务运行在当前登录的账户下的一个进程中(0x00000050)
            //      SERVICE_USER_SHARE_PROCESS   服务运行在当前登录账户下的一个进程中,并与其他服务共享该进程(0x00000060)
            public int dwServiceType;

            // The State of the service(服务的当前状态)
            //      SERVICE_STOP_PENDING        正在停止服务(0x00000003)
            //      SERVICE_STOPPED             服务已停止(0x00000001)
            //      SERVICE_START_PENDING       正在启动服务(0x00000002)
            //      SERVICE_RUNNING             服务已运行(0x00000004)
            //      SERVICE_CONTINUE_PENDING    服务恢复(0x00000005)
            //      SERVICE_PAUSE_PENDING       正在暂停服务(0x00000006)
            //      SERVICE_PAUSED              服务已暂停(0x00000007)
            public ServiceState dwCurrentState;

            
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;

            // The estimated time required (执行下面操作所需要花费的时间, 以毫秒为单位)
            // for a pending start, stop, pause, or continue operation, in milliseconds
            public int dwWaitHint;
        };
    }
}