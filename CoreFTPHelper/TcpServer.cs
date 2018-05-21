#region Using directives
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using libutilscore.Logging;
#endregion

namespace CoreFTPHelper
{
    class TcpServer
    {
        #region Private Properties
        private static object TaskLockObj = new object();
        
        private int _port = 50000;

        private IPAddress _address = IPAddress.Loopback;

        private TcpListener _listener = null;

        private static Stack stack = new Stack();

        private static ManualResetEvent allDone = new ManualResetEvent(false);

        /// <summary>
        /// Look up timeout in milliseconds
        /// </summary>
        private static int LookupTimeout = 5000;

        //private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        #endregion

        #region Public Properties

        public static ManualResetEvent executeDone = new ManualResetEvent(false);

        public static object ResultLockObj = new object();

        public static Hashtable ResultsTable = new Hashtable();

        #endregion

        #region Construtors

        public TcpServer(int port)
        {
            this._port = port;
            //_listener = new TcpListener(_address, _port);
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
        /// Write Response to the client
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="client"></param>
        private void _ReturnResponse(string operationId, TcpClient client)
        {
            foreach (DictionaryEntry entry in ResultsTable)
            {
                if ((string)entry.Key == operationId)
                {
                    StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8);
                    Tuple<bool, string> result = (Tuple<bool, string>)entry.Value;
                    writer.Write(result.Item1.ToString());
                    writer.Write(" ");
                    writer.Write((string)result.Item2);
                    writer.Close();
                    break;
                }
            }
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

                string operation = paramArgs[0];

                if (operation == "cfg")
                {
                    if (paramArgs.Count() - 2 < 3)
                    {
                        throw new ArgumentException("couldn't config FTP: need more inforation");
                    }
                    //    host          user         passwd      remotePath
                    Program.ftpInfo = new ArrayList { paramArgs[2], paramArgs[3],
                        paramArgs[4], paramArgs[5] ??"/" };
                }
                else if (operation == "resp")
                {
                    string operationId = paramArgs[1];
                    if (ResultsTable.Count > 0)
                    {
                        _ReturnResponse(operationId, client);
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
            } catch (Exception ex)
            {
                Log.Logger.Error("Handle Client Data failed: {0}", ex.Message);
            } finally
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
                    Thread.Sleep(1000);
                    lock (TaskLockObj)
                    {
                        if (stack.Count > 0)
                        {
                            Log.Logger.Info("Starting executing a task");
                            FTPTask task = (FTPTask)stack.Pop();
                            task.Exec();
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
        public class StateObject
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
        /// Sending Data to the client synchronously
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="data"></param>
        private void SendSync(Socket handler, String data)
        {
            try
            {
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.UTF8.GetBytes(data);

                // Send data to the client
                int byteSent = handler.Send(byteData);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Synchronously Sending data to client failed: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Sending Data to the client asynchronously
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="data"></param>
        private void SendAsync(Socket handler, String data)
        {
            // Convert the string data to byte data using UTF8 encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Log.Logger.Debug("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Asynchronously Sending data to client failed: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue 
            allDone.Set();
            try
            {
                // Get the socket that handles the client request
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object
                StateObject state = new StateObject
                {
                    workSocket = handler
                };

                // Read command from the client socket
                int bytesRead = handler.Receive(state.buffer);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.UTF8.GetString(
                        state.buffer, 0, bytesRead));

                    // Get the command string
                    string command = state.sb.ToString();
                    Log.Logger.Info("Command from client: {0}", command);

                    // Parse the command into several parts
                    string[] paramArgs = ParseData(command);
                    Log.Logger.Info("Content parts: {0}", paramArgs.Length);

                    string operation = paramArgs[0];

                    if (operation == "cfg") // config ftp server infomation
                    {
                        if (paramArgs.Count() - 2 < 3)
                        {
                            throw new ArgumentException("couldn't config FTP: need more inforation");
                        }
                        //    host          user         passwd      remotePath
                        Program.ftpInfo = new ArrayList { paramArgs[2], paramArgs[3],
                        paramArgs[4], paramArgs[5] ??"/" };
                    }
                    else if (operation == "resp") // get execution result
                    {
                        executeDone.Reset();
                        executeDone.WaitOne();

                        string operationId = paramArgs[1];
                        Tuple<bool, string> execResult = Tuple.Create(false, "No matched Result");

                        lock (ResultLockObj)
                        {
                            foreach (DictionaryEntry entry in ResultsTable)
                            {
                                if ((string)entry.Key == operationId)
                                {
                                    Tuple<bool, string> temp = (Tuple<bool, string>)entry.Value;
                                    execResult = Tuple.Create(temp.Item1, temp.Item2);
                                    ResultsTable.Remove(entry.Key);
                                    break;
                                }
                            }
                        }

                        // send execution result to the client
                        SendSync(handler, execResult.Item1.ToString() + "," + execResult.Item2);
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
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Handle Client Failed: {0}", ex.ToString());
            }
        }


        public void _StartSocket()
        {
            byte[] bytes = new byte[1024];

            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(_address, _port);

            try
            {
                Socket listener = new Socket(AddressFamily.InterNetwork, 
                    SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(ipEndPoint);
                listener.Listen(100);

                // start a thread to listen to the stack
                Thread executeThread = new Thread(() => _ExecuteTask());
                executeThread.Start();

                while (true)
                {
                    // Set the event to nonsignaled state
                    allDone.Reset();

                    Log.Logger.Info("Waiting for a connection ...");

                    // Start an asynchronous socket to listen for connections
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback), 
                        listener);

                    // Wait until a connection is made before continuing
                    allDone.WaitOne();
                }
            }
            catch (SocketException ex)
            {
                Log.Logger.Error("Socket error: {0}", ex.ToString());
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Server Error: {0}", ex.ToString());
            }

        }

        /// <summary>
        /// Start Tcp Server
        /// </summary>
        public void _Start()
        {
            try
            {
                _listener.Start();
                Log.Logger.Info("Server started(host: {0}, port: {1})", _address.ToString(), _port);
            }
            catch (SocketException ex)
            {
                Log.Logger.Info("Create Server Socket faild: {0}: {1}", ex.SocketErrorCode, ex.Message);
            }

            // start a thread to listen to the stack
            Thread executeThread = new Thread(() => _ExecuteTask());
            executeThread.Start();

            TcpClient client = null;

            try
            {
                while (true)
                {
                    try
                    {
                        client = _listener.AcceptTcpClient();

                        Log.Logger.Info("Accepted");

                        ThreadPool.QueueUserWorkItem(_HandleClient, client);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("Handle Client Failed: {0}", ex.Message);
                    }
                }

#if INCLUDE_CANCELLATION
                if (_listener != null)
                {
                    // close event handler thread
                    if (executeThread != null)
                        executeThread.Interrupt();

                    // close client connection
                    if (client != null)
                        client.Close();

                    // close listener
                    _listener.Stop();
                }
#endif
            }
            catch (SocketException ex)
            {
                Log.Logger.Info("Close Server Socket faild: {0}: {1}", ex.SocketErrorCode, ex.Message);
            }
        }
    }
}
