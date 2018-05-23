#region Using directives
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using libutilscore.Logging;
#endregion

namespace FTPWorker
{
    class TcpServer
    {
        #region Private Properties

        private static object TaskLockObj = new object();

        private int _port = 55505;

        private IPAddress _address = IPAddress.Loopback;

        /// <summary>
        /// A Stack for holding tasks
        /// </summary>
        private static Stack stack = new Stack();

        /// <summary>
        /// A Flag for controlling thread synchronization
        /// </summary>
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        //private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        #endregion

        #region Public Properties

        public static object ResultLockObj = new object();

        public static Hashtable ResultsTable = new Hashtable();

        //public static ManualResetEvent executeDone = new ManualResetEvent(false);

        #endregion

        #region Construtors

        public TcpServer(int port)
        {
            this._port = port;
        }

        #endregion

        /// <summary>
        /// Parse data from the request
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string[] ParseData(string content)
        {
            string[] paramsArgs = content.Split(',');

            // if paramsArgs's count is less than 2, throw exception
            if (paramsArgs.Count() < 2)
            {
                throw new Exception("Client Data Error");
            }
            return paramsArgs;
        }

        /// <summary>
        /// Handle Client
        /// </summary>
        /// <param name="obj"></param>
        private void _HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            try
            {
                StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                string content = reader.ReadToEnd();
                reader.Close();

                Log.Logger.Info("Content from client: {0}", content);

                string[] paramArgs = ParseData(content);
                Log.Logger.Info("Content parts: {0}", paramArgs.Length);

                string operation = paramArgs[0].Trim();

                Log.Logger.Debug("operation: {0}", operation);

                if (operation == "cfg")
                {
                    if (paramArgs.Count() - 2 < 3)
                    {
                        throw new ArgumentException("couldn't config FTP: need more inforation");
                    }
                    //    host          user         passwd      remotePath
                    Main.ftpInfo = new ArrayList { paramArgs[2], paramArgs[3],
                        paramArgs[4], paramArgs[5] ??"/" };
                }
                else if (operation == "resp")
                {
                    string operationId = paramArgs[1];
                    Tuple<bool, string> result = Tuple.Create(false, "当前没有数据");
                    lock (ResultLockObj)
                    {
                        if (ResultsTable.Count > 0)
                        {
                            foreach (DictionaryEntry entry in ResultsTable)
                            {
                                if (operationId.Equals(entry.Key))
                                {
                                    Tuple<bool, string> record = (Tuple<bool, string>)entry.Value;
                                    result = Tuple.Create(record.Item1, record.Item2);
                                    ResultsTable.Remove(entry.Key); // remove result after finding it
                                    break;
                                }
                            }

                            using (StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8))
                            {
                                string response = result.Item1 + "," + result.Item2;
                                //byte[] buffer = Encoding.UTF8.GetBytes(response);
                                //writerStream.Write(buffer, 0, buffer.Length);
                                writer.Write(response);
                            }
                        }
                    }
                }
                else
                {
                    FTPTask task = new FTPTask
                    {
                        Operation = operation
                    };

                    // put params from index 1 into the operationArgs
                    for (int i = 1; i < paramArgs.Length; i++)
                    {
                        task.OperaionArgs.Add(paramArgs[i]);
                    }
                    lock (TaskLockObj)
                    {
                        stack.Push(task);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Handle Client Data failed: {0}", ex.Message);
            }
            finally
            {
                client.Close();
            }

        }

        /// <summary>
        /// Executing Task Thread
        /// </summary>
        private void _ExecuteTask()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(100);
                    lock (TaskLockObj)
                    {
                        if (stack.Count > 0)
                        {
                            Log.Logger.Info("Starting executing a task");
                            FTPTask task = (FTPTask)stack.Pop();
                            //task.Exec();

                            TaskForm taskForm = new TaskForm(task);
                            taskForm.Show();

                            Log.Logger.Info("Successfully execute a task.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Execute task failed: {0}", ex.StackTrace);
                }
            }
        }


        // State object for reading client data asynchronously
        internal class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;

            // Size of receive buffer.
            public const int BufferSize = 1024;

            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];

            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        /// <summary>
        /// Start the Server
        /// </summary>
        public void _StartSocket()
        {
            try
            {
                IPEndPoint localEndpoint = new IPEndPoint(_address, _port);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // bind
                listener.Bind(localEndpoint);
                listener.Listen(100); // max listeners
                Log.Logger.Info("Server started(host: {0}, port: {1})", _address.ToString(), _port);

                // start a thread to listen to the stack
                //Thread executeThread = new Thread(new ThreadStart(_ExecuteTask));
                //executeThread.Start();

                while (!Main.cancellationTokenSource.IsCancellationRequested)
                {
                    Log.Logger.Info("Waiting connection ...");

                    // Set the event to nonsignaled state
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Info("Server Socket error: {0}", ex.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject
            {
                workSocket = handler
            };

            // Receive client's command, and convert to string
            int byteread = handler.Receive(state.buffer);
            state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, byteread));
            string content = state.sb.ToString();
            Log.Logger.Info("Content from client: {0}", content);

            try
            {
                string[] paramArgs = ParseData(content);
                Log.Logger.Info("Content parts: {0}", paramArgs.Length);

                string operation = paramArgs[0].Trim();
                Log.Logger.Debug("operation: {0}", operation);


                switch (operation)
                {
                    case "cfg":
                        {
                            if (paramArgs.Count() - 2 < 3)
                            {
                                throw new ArgumentException("couldn't config FTP: need more inforation");
                            }
                            //    host          user         passwd      remotePath
                            Main.ftpInfo = new ArrayList { paramArgs[2], paramArgs[3],
                            paramArgs[4], paramArgs[5] ??"/" };
                            break;
                        }
                    case "resp":
                        {
                            string operationId = paramArgs[1];
                            Tuple<bool, string> result = Tuple.Create(false, "没有获取到执行结果");
                            lock (ResultLockObj)
                            {
                                if (ResultsTable.Count > 0)
                                {
                                    foreach (DictionaryEntry entry in ResultsTable)
                                    {
                                        if (operationId.Equals(entry.Key))
                                        {
                                            Tuple<bool, string> record = (Tuple<bool, string>)entry.Value;
                                            result = Tuple.Create(record.Item1, record.Item2);
                                            ResultsTable.Remove(entry.Key); // remove result after finding it
                                            break;
                                        }
                                    }
                                }
                            }

                            // send execution result to the client
                            string response = result.Item1 + "," + result.Item2;

                            Send(handler, response);

                            break;
                        }
                    default:
                        {
                            FTPTask task = new FTPTask
                            {
                                Operation = operation
                            };

                            // put params from index 1 into the operationArgs
                            for (int i = 1; i < paramArgs.Length; i++)
                            {
                                task.OperaionArgs.Add(paramArgs[i]);
                            }
                            //lock (TaskLockObj)
                            //{
                            //    stack.Push(task);
                            //}
                            Main.StartFTPTaskWindow(task);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.StackTrace);
            }
        }

        /// <summary>
        /// Send data to client synchrously
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="data"></param>
        private void Send(Socket handler, String data)
        {
            try
            {
                // Convert the string data to byte data using UTF8 encoding.
                byte[] byteData = Encoding.UTF8.GetBytes(data);

                // Send data to the client synchrously
                int byteWritten = handler.Send(byteData);

                Log.Logger.Debug("Sent {0} bytes to client.", byteWritten);

                // Shutdown client socket
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Send Response to client failed: {0}", ex.ToString());
            }
        }
    }
}
