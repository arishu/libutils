
using System.ComponentModel;
using libutilscore.Common;

namespace libutilscore.FTP
{
    class FTPInfo
    {
        public FTPInfo(string host, string user, string passwd)
        {
            Host = host;
            User = user;
            Passwd = passwd;
            RemotePath = "/";
        }

        public FTPInfo(string host, string user, string passwd, string remotePath)
        {
            Host = host;
            User = user;
            Passwd = passwd;
            RemotePath = remotePath;
        }

        private string passwd;

        [DefaultValue("127.0.0.1")]
        public string Host { get; set; }

        [DefaultValue("anonymous")]
        public string User { get; set; }

        [DefaultValue("anonymous@127.0.0.1")]
        public string Passwd
        {
            get { return passwd;  }
            set
            {
                if (value == null || value.Equals(""))
                    passwd = "anonymous@127.0.0.1";
                else
                    passwd = Codec.Base64Decode(Codec.Base64Decode(Codec.Base64Decode(value)));
            }
        }

        [DefaultValue("/")]
        public string RemotePath { get; set;}
    }
}
