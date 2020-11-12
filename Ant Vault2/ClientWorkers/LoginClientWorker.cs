using SimpleSockets;
using SimpleSockets.Client;
using System.Windows;

namespace AntVault2Client.ClientWorkers
{
    class LoginClientWorker
    {
        public static bool SetUpConnection = false;
        public static string CurrentUser;
        internal static SimpleSocketClient AntVaultClient = new SimpleSocketTcpClient();


        internal static void DoAuthentication(string Username, string Password)
        {
            AntVaultClient.BytesReceived += MainClientWorker.AntVaultClient_BytesReceived;
            AntVaultClient.StartClient("AntVault.ddns.net", 8910);
            CurrentUser = Username;
            try
            {
                AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/NewConnection -U " + Username + " -P " + Password + "."));//NewConnection -U Username -P Password.
            }
            catch
            {
                MessageBox.Show("Server is offline, therefore the authentication process could not take place, please try again later!", "Error! Server offline!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
