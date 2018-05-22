
#region Using directives
using System.ComponentModel;
using libutilscore.Common;
#endregion

namespace libutilscore.FTP
{
    public class FTPInfo
    {
        #region Constructors

        public FTPInfo()
        {
            Host = "127.0.0.1";
            User = "anonymous";
            Passwd = "V1ZjMWRtSnViSFJpTTFaNlVVUkZlVTU1TkhkTWFrRjFUVkU5UFE9PQ =="; // anonymous@127.0.0.1
            RemotePath = "/";
        }

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

        #endregion

        private string passwd;

        [DefaultValue("127.0.0.1")]
        public string Host { get; set; }

        [DefaultValue("anonymous")]
        public string User { get; set; }

        // anonymous@127.0.0.1
        [DefaultValue("V1ZjMWRtSnViSFJpTTFaNlVVUkZlVTU1TkhkTWFrRjFUVkU5UFE9PQ ==")]
        public string Passwd
        {
            get { return passwd; }
            set
            {
                if (value == null || value.Equals(""))
                    passwd = "anonymous@127.0.0.1";
                else
                    passwd = Codec.Base64Decode(Codec.Base64Decode(Codec.Base64Decode(value)));
            }
        }

        [DefaultValue("/")]
        public string RemotePath { get; set; }
    }
}
