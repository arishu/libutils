using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using libutilscore.Logging;

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
            Tuple<bool, string> result = Tuple.Create(false, "Send web request failed"); 
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

                byte[] buffer = new byte[256];
                buffer = Encoding.UTF8.GetBytes(content.ToArray());
                int bytesSent = socket.Send(buffer);
                if (bytesSent != buffer.Length)
                    throw new Exception("Client Data send failed: datapackage loss");
                result = Tuple.Create(true, "");
            }
            catch (Exception ex)
            {
                result = Tuple.Create(false, ex.Message);
                Log.Logger.Error("Failed to Send message to the server: {0}", ex.Message);
            }
            return result;
        }
    }
}
