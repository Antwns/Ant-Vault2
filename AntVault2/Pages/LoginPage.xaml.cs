using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace AntVault2Client.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ClientWorkers.LoginClientWorker.SetUpClients();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string Username = UserNameTextBox.Text;
            string Password = PasswordTextBox.Password;
            Thread AuthThread = new Thread(() => ClientWorkers.LoginClientWorker.DoAuthentication(Username, Password));
            AuthThread.Start();
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ConnectButton_Click(PasswordTextBox, e);
            }
        }
    }
}
