using WatsonTcp;
using System.Windows;
using System.Threading;

namespace AntVault2Client.ClientWorkers
{
    class LoginClientWorker
    {
        public static bool SetUpConnection = false;
        public static string CurrentUser;
        public static WatsonTcpClient AntVaultClient = new WatsonTcpClient("AntMax.ddns.net", 25566);

        internal static void SetUpClients()
        {
            AntVaultClient.Events.MessageReceived += MainClientWorker.AntVaultClient_DataReceived;
        }

        internal static void DoAuthentication(string Username, string Password)
        {
            Thread.CurrentThread.IsBackground = true;
            CurrentUser = Username;
            AntVaultClient.Keepalive.EnableTcpKeepAlives = true;
            AntVaultClient.Keepalive.TcpKeepAliveInterval = 5;
            AntVaultClient.Keepalive.TcpKeepAliveTime = 5;
            AntVaultClient.Keepalive.TcpKeepAliveRetryCount = 5;
            AntVaultClient.Start();
            try
            {
                AntVaultClient.Send("/NewConnection -U " + Username + " -P " + Password + ".");//NewConnection -U Username -P Password.
            }
            catch
            {
                MessageBox.Show("Server is offline, therefore the authentication process could not take place, please try again later!", "Error! Server offline!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
