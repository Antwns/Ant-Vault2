using AntVault2Server.ServerWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AntVault2Server.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainServerWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ServerStartButton_Click(object sender, RoutedEventArgs e)
        {
            MainServerWorker.StartAntVaultStatusServer();
            MainServerWorker.StartAntVaultServer();
            ServerStartButton.IsEnabled = false;
            ServerStopButton.IsEnabled = true;
        }

        private void ServerStopButton_Click(object sender, RoutedEventArgs e)
        {
            MainServerWorker.StopAntVaultStatusServer(MainServerWorker.AntVaultStatusServer);
            MainServerWorker.StopAntVaultServer(MainServerWorker.AntVaultServer);
            ServerStopButton.IsEnabled = false;
            ServerStartButton.IsEnabled = true;
        }

        private void UsersListTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsersListTextBox.ScrollToEnd();
        }

        private void ServerConsoleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerConsoleTextBox.ScrollToEnd();
        }

        private void ConsoleInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                MainServerWorker.DoServerCommand(ConsoleInputTextBox.Text);
                ConsoleInputTextBox.Text = null;
            }
        }

        private void UsersListTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            UsersListTextBox.Text = null;
        }

        private void ServerConsoleTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ServerConsoleTextBox.Text = null;
        }

        private void ConsoleInputTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ConsoleInputTextBox.Text = null;
        }
    }
}
