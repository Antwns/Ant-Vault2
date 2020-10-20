﻿using WatsonTcp;
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
        public static WatsonTcpClient AntVaultClient = new WatsonTcpClient("AntMax.ddns.net", 8910);

        internal static void SetUpClients()
        {
            Thread StatusCheckThread = new Thread(StatusCheckThreadWork);
            StatusCheckThread.Start();
            AntVaultClient.Events.MessageReceived += MainClientWorker.AntVaultClient_DataReceived;
        }
        internal static void StatusCheckThreadWork()
        {
            try
            {
                Thread.Sleep(1000);
                WatsonTcpClient AntVaultStatusChecker = new WatsonTcpClient("AntMax.ddns.net", 8911);
                AntVaultStatusChecker.Start();
                CanConnect = true;
                WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
                {
                    WindowControllers.MainWindowController.LoginPage.ServerStatusEllipse.Fill = Brushes.Green;
                });
            }
            catch
            {
                Thread.Sleep(1000);
                CanConnect = false;
                WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
                {
                    WindowControllers.MainWindowController.LoginPage.ServerStatusEllipse.Fill = Brushes.Red;
                });
            }
            Thread StatusCheckThread = new Thread(StatusCheckThreadWork);
            StatusCheckThread.Start();
        }

        internal static void DoAuthentication(string Username, string Password)
        {
            CurrentUser = Username;
            if(CanConnect == true)
            {
                if (SetUpConnection == false)
                {
                    AntVaultClient.Start();
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
