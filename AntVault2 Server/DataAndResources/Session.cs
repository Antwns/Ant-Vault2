using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace AntVault2Server.DataAndResources
{
    [Serializable()]
    public class Session
    {
        public string IpPort { get; set; }
        public string Username { get; set; }

        public DateTime LoginTime { get; set; }

        public DateTime LogoutTime { get; set; }

        public Bitmap ProfilePicture { get; set; }

        public string Status { get; set; }

        public Collection<string> Friends { get; set; }
    }
}
