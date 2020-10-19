using SimpleTcp;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace AntVault2Client.ClientWorkers
{
    class LoginClientWorker
    {
        public static bool SetUpConnection = false;
        public static string CurrentUser;
        public static bool CanConnect = false;
        public static SimpleTcpClient AntVaultClient = new SimpleTcpClient("AntMax.ddns.net", 8910, false, null, null);

        internal static void SetUpClients()
        {
            Thread StatusCheckThread = new Thread(StatusCheckThreadWork);

            StatusCheckThread.Start();
            AntVaultClient.Events.DataReceived += MainClientWorker.AntVaultClient_DataReceived;
        }
        internal static void StatusCheckThreadWork()
        {
            try
            {
                Thread.Sleep(1000);
                SimpleTcpClient AntVaultStatusChecker = new SimpleTcpClient("AntMax.ddns.net", 8911, false, null, null);
                AntVaultStatusChecker.Connect();
                CanConnect = true;
                WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
                {
                    WindowControllers.MainWindowController.LoginPage.ServerStatusEllipse.Fill = Brushes.Green;
                });
            }
            catch
            {
                Thread.Sleep(1000);
                StatusCheckThreadWork();
                CanConnect = false;
                WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
                {
                    WindowControllers.MainWindowController.LoginPage.ServerStatusEllipse.Fill = Brushes.Red;
                });
            }
            StatusCheckThreadWork();
        }

        internal static void DoAuthentication(string Username, string Password)
        {
            CurrentUser = Username;
            if(CanConnect == true)
            {
                if (SetUpConnection == false)
                {
                    AntVaultClient.Connect();
                    AntVaultClient.Keepalive.EnableTcpKeepAlives = true;
                    AntVaultClient.Keepalive.TcpKeepAliveInterval = 5;
                    AntVaultClient.Keepalive.TcpKeepAliveTime = 5;
                    AntVaultClient.Keepalive.TcpKeepAliveRetryCount = 5;
                    SetUpConnection = true;
                }
                AntVaultClient.Send("/NewConnection -U " + Username + " -P " + Password + ".");//NewConnection -U Username -P Password.
            }
            else
            {
                MessageBox.Show("Server is offline, therefore the authentication process could not take place, please try again later!", "Error! Server offline!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
