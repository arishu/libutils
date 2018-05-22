#region Using directives
using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using libutilscore.Logging;
#endregion

namespace libutilscore.Net
{
    public class SharpWebClient
    {
        /// <summary>
        /// Send Web Request to the server
        /// </summary>
        /// <param name="content"></param>
        /// <param name="opts">
        ///     This is optional argument, but if specified,
        ///     The first is host, second is port
        /// </param>
        public Tuple<bool, string> SendWebRequest(string content, params string[] opts)
        {
            string hostname = "127.0.0.1";
            int port = 55505;
            if (opts.Length != 0)
            {
                hostname = opts[0];
                port = int.Parse(opts[1]);
            }

            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(hostname), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipEndPoint);
                
                Log.Logger.Info("Send command to server: {0}", content);

                byte[] buffer = new byte[256];
                buffer = Encoding.UTF8.GetBytes(content.ToArray());
                socket.Send(buffer);

                // close stream and close the socket
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return Tuple.Create(true, "");
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Failed to Send message to the server: {0}", ex.Message);
                return Tuple.Create(false, ex.Message);
            }
        }
    }
}
