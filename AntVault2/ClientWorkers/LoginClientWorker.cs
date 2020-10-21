using WatsonTcp;
using System.Windows;

namespace AntVault2Client.ClientWorkers
{
    class LoginClientWorker
    {
        public static bool SetUpConnection = false;
        public static string CurrentUser;
        public static WatsonTcpClient AntVaultClient = new WatsonTcpClient("AntMax.ddns.net", 8910);

        internal static void SetUpClients()
        {
            AntVaultClient.Events.MessageReceived += MainClientWorker.AntVaultClient_DataReceived;
        }

        internal static void DoAuthentication(string Username, string Password)
        {
            CurrentUser = Username;
            AntVaultClient.Keepalive.EnableTcpKeepAlives = true;
            AntVaultClient.Keepalive.TcpKeepAliveInterval = 5;
            AntVaultClient.Keepalive.TcpKeepAliveTime = 5;
            AntVaultClient.Keepalive.TcpKeepAliveRetryCount = 5;
            AntVaultClient.Start();
            AntVaultClient.Send("/NewConnection -U " + Username + " -P " + Password + ".");//NewConnection -U Username -P Password.
            MessageBox.Show("Server is offline, therefore the authentication process could not take place, please try again later!", "Error! Server offline!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
